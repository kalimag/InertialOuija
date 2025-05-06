using SQLite;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace InertialOuija.Ghosts.Database.SQL
{
	public abstract class SQLiteDatabase
	{
		private readonly SemaphoreSlim _writeLock = new(1, 1);
		private readonly AsyncLocal<bool> _inTransaction = new();

		private SQLiteConnection _writeConnection;
		private SQLiteConnection _readConnection;

		public bool InTransaction => _inTransaction.Value;

		protected abstract string DatabasePath { get; }

		protected abstract int? UserVersion { get; }

		protected virtual bool LogStatements => false;



		public SQLiteDatabase()
		{

			try
			{
				InitConnections();
			}
			// Sqlite will open file as read-only if it is in use
			catch (Exception ex) when (ex is not SQLiteException { Result: SQLite3.Result.ReadOnly or SQLite3.Result.Busy or SQLite3.Result.CannotOpen } and not DllNotFoundException)
			{
				Log.Error(ex, true);
				Log.Info("First open attempt failed, trying to delete and reopen", nameof(ExternalGhostDatabase));
				DisposeConnections();
				DeleteDatabase(DatabasePath); // Try to delete outdated/possibly corrupted db
				InitConnections();
			}

			Log.Debug("Database initialized", nameof(ExternalGhostDatabase));
		}

		private void InitConnections()
		{
			var openFlags = SQLiteOpenFlags.FullMutex;

			var directory = Path.GetDirectoryName(DatabasePath);
			if (!string.IsNullOrEmpty(directory))
				Directory.CreateDirectory(directory);

			_writeConnection = OpenConnection(new(DatabasePath, openFlags | SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite, true, uncheckedUInt64Conversion: true));

			Log.Info($"SQLite version: {_writeConnection.LibVersionNumber}");

			if (UserVersion != null)
			{
				var userVersion = _writeConnection.ExecuteScalar<int>("PRAGMA user_version");
				if (userVersion != UserVersion && userVersion != 0)
					throw new InvalidDataException($"Wrong cache database version {userVersion} (current is {UserVersion})");
			}
			_writeConnection.EnableWriteAheadLogging(); // WAL allows for simultaneous reads while a write is active

			InitDatabase(_writeConnection);

			if (UserVersion != null)
				_writeConnection.ExecuteScalar<int>($"PRAGMA user_version = {UserVersion}");

			_readConnection = OpenConnection(new(DatabasePath, openFlags | SQLiteOpenFlags.ReadOnly, true, uncheckedUInt64Conversion: true));
		}

		private SQLiteConnection OpenConnection(SQLiteConnectionString connectionString)
		{
			var connection = new SQLiteConnection(connectionString);
			try
			{
				if (LogStatements)
				{
					var source = Path.GetFileName(connectionString.DatabasePath) + (connectionString.OpenFlags.HasFlag(SQLiteOpenFlags.ReadOnly) ? " R" : " W");
					connection.Tracer = msg => Log.Debug(msg, source);
					connection.Trace = true;
				}
				InitConnection(connection);
			}
			catch (Exception)
			{
				connection.Dispose();
				throw;
			}
			return connection;
		}

		protected abstract void InitDatabase(SQLiteConnection connection);

		protected virtual void InitConnection(SQLiteConnection connection)
		{ }

		private static void DeleteDatabase(string path)
		{
			File.Delete(path);
			File.Delete(path + "-journal");
			File.Delete(path + "-wal");
			File.Delete(path + "-shm");
		}

		protected SQLiteConnection GetConnection(bool write)
		{
			if (write && !InTransaction)
				throw new InvalidOperationException("Attempted to write without transaction");
			return write || InTransaction ? _writeConnection : _readConnection;
		}

		protected virtual void Invalidate()
		{ }



		public void Write(Action action)
		{
			EnsureNotNested();
			_writeLock.Wait();
			WriteHavingLock(action);
		}

		public async Task WriteAsync(Action action)
		{
			EnsureNotNested();
			await _writeLock.WaitAsync().ConfigureAwait(false);
			WriteHavingLock(action);
		}

		private void WriteHavingLock(Action action)
		{
			try
			{
				_writeConnection.BeginTransaction(TransactionMode.Immediate);
				_inTransaction.Value = true;
				try
				{
					action();
				}
				catch (Exception)
				{
					_writeConnection.Rollback();
					throw;
				}
				_writeConnection.Commit();
				Invalidate();
			}
			finally
			{
				_inTransaction.Value = false;
				_writeLock.Release();
			}
		}

		private void EnsureNotNested()
		{
			if (InTransaction)
				throw new InvalidOperationException("Attempted to enter nested transaction.");
		}



		// with PASSIVE locks shouldn't be a concern
		public void PassiveCheckpoint() => _writeConnection.ExecuteScalar<int>("PRAGMA wal_checkpoint(PASSIVE)");
		protected void TruncateCheckpoint() => _writeConnection.ExecuteScalar<int>("PRAGMA wal_checkpoint(TRUNCATE);");

		// Force optimize
		protected void Optimize() => _writeConnection.ExecuteScalar<string>("PRAGMA optimize=0x10002");

		private void DisposeConnections()
		{
			_readConnection?.Dispose();
			_writeConnection?.Dispose();
		}

		public virtual void Dispose()
		{
			DisposeConnections();
		}
	}
}
