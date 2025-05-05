extern alias GameScripts;
using GameScripts.Assets.Source.Enums;
using InertialOuija.Ghosts.Database.SQL;
using SQLite;
using SqlKata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using EGF = InertialOuija.Ghosts.ExternalGhostFile;
using GameModes = GameScripts.Assets.Source.Gameplay.GameModes;

namespace InertialOuija.Ghosts.Database;

public partial class ExternalGhostDatabase : SQLiteDatabase
{
	private const int InvalidatedCount = -1;



	protected override string DatabasePath => Path.Combine(FileUtility.ModDirectory, "cache", "ghosts.db");
	protected override int? UserVersion => 1;
#if DEBUG
	protected override bool LogStatements => true;
#endif

	private volatile int _count = InvalidatedCount;
	public int Count
	{
		get
		{
			int count = _count;
			return count > InvalidatedCount ? count : UpdateCount();
		}
	}



	protected override void InitDatabase(SQLiteConnection connection)
	{
		connection.CreateTable<ExternalGhostFile>();
		TruncateCheckpoint();
		Optimize();
	}

	protected override void InitConnection(SQLiteConnection connection)
	{
		connection.Execute("PRAGMA synchronous = NORMAL"); // In WAL mode this could lose writes, but won't corrupt data
		connection.BusyTimeout = TimeSpan.FromSeconds(1); // this should only be relevant if another process is locking the db
	}

	protected override void Invalidate()
	{
		_count = InvalidatedCount;
	}

	private int UpdateCount()
	{
		const int UpdatingCount = -2;

		if (InTransaction)
			return GetCount(false); //don't cache value in transaction

		int count;
		while ((count = Interlocked.CompareExchange(ref _count, UpdatingCount, InvalidatedCount)) <= InvalidatedCount)
		{
			count = GetCount(false);
			int exchanged = Interlocked.CompareExchange(ref _count, count, UpdatingCount);
			if (exchanged == UpdatingCount || exchanged == count)
				break;
		}

		return count;
	}

	private Query BuildGhostQuery(in GhostFilter filter, bool deprioritizeLeaderboard)
	{
		var query = GetConnection(false).BuildQuery<ExternalGhostFile>()
			.Where(nameof(EGF.Track), filter.Track)
			.Where(nameof(EGF.Direction), filter.Direction);

		if (filter.PerformanceClass != null)
			query.WhereIn(nameof(EGF.Car), filter.PerformanceClass.Value.GetCars());
		if (filter.Car != null)
			query.Where(nameof(EGF.Car), filter.Car.Value);

		if (filter.User is ulong user)
			query.WhereUser(user);

		if (filter.Type == GhostType.Style)
		{
			query.Where(nameof(EGF.GameMode), nameof(GameModes.StyleMode))
				 .WhereNotNull(nameof(EGF.StyleScore))
				 .OrderByDesc(nameof(EGF.StyleScore));
		}
		else if (filter.Type == GhostType.Precision)
		{
			query.Where(nameof(EGF.GameMode), nameof(GameModes.PrecisionStyleMode))
				 .WhereNotNull(nameof(EGF.PrecisionScore))
				 .OrderByDesc(nameof(EGF.PrecisionScore));
		}

		query.OrderBy(nameof(EGF.TimeInSeconds)); // Always sort by time

		if (deprioritizeLeaderboard)
		{
			query.SelectRaw($"{nameof(EGF.Source)} = {(int)GhostSource.Leaderboard} AS FromLeaderboard")
				 .OrderBy("FromLeaderboard");
		}

		return query;
	}

	public IReadOnlyList<ExternalGhostFile> FindGhosts(in GhostFilter filter, ExternalGhostMode mode, int count)
	{
		if (mode == ExternalGhostMode.Default)
			throw new InvalidOperationException("Cannot use external ghosts in default mode.");
		if (mode == ExternalGhostMode.None)
			return [];

		var query = BuildGhostQuery(filter, true)
			.SelectRaw("*");

		Predicate<ExternalGhostFile> pbPredicate = null;
		if (mode == ExternalGhostMode.NextBest)
		{
			var userFilter = filter with { User = GameData.SteamUser.Id };
			if (filter.Type == GhostType.Timed && GetBestTimeInSeconds(userFilter) is float pbTime)
			{
				query.Where(nameof(EGF.TimeInSeconds), "<=", pbTime);
				pbPredicate = ghost => ghost.TimeInSeconds == pbTime;
			}
			else if (filter.Type == GhostType.Precision && GetBestPrecisionScore(userFilter) is int pbPrecision)
			{
				query.Where(nameof(EGF.PrecisionScore), ">=", pbPrecision);
				pbPredicate = ghost => ghost.PrecisionScore == pbPrecision;
			}
			else if (filter.Type == GhostType.Style && GetBestStyleScore(userFilter) is float pbStyle)
			{
				query.Where(nameof(EGF.StyleScore), ">=", pbStyle);
				pbPredicate = ghost => ghost.StyleScore == pbStyle;
			}
		}

		var ghosts = query.Execute<ExternalGhostFile>()
			.Distinct<ExternalGhostFile>(RelaxedGhostComparer.Instance);

		if (filter.UniqueCars && mode != ExternalGhostMode.NextBest)
			ghosts = ghosts.Distinct<ExternalGhostFile>(GhostCarComparer.Instance);

		if (pbPredicate != null)
		{
			// This seems unreasonably inefficient but I'm not gonna figure out how to do it in SQL
			var ghostList = ghosts.ToList();
			int pbIndex = ghostList.FindIndex(pbPredicate);
			int nBestIndex = Math.Max(pbIndex - count, 0);
			ghosts = ghostList.Skip(nBestIndex);
		}

		return ghosts.Take(count).ToList();
	}

	private float? GetBestTimeInSeconds(in GhostFilter filter) =>
		BuildGhostQuery(filter, false)
		.Select(nameof(EGF.TimeInSeconds))
		.Limit(1)
		.ExecuteScalar<float?>();

	private int? GetBestPrecisionScore(in GhostFilter filter) =>
		BuildGhostQuery(filter, false)
		.Select(nameof(EGF.PrecisionScore))
		.Limit(1)
		.ExecuteScalar<int?>();

	private float? GetBestStyleScore(in GhostFilter filter) =>
		BuildGhostQuery(filter, false)
		.Select(nameof(EGF.StyleScore))
		.Limit(1)
		.ExecuteScalar<float?>();

	public GhostTime? GetPersonalBestTime(Track track, TrackDirection direction, Car? car) =>
		GetBestTimeInSeconds(new(GhostType.Timed, track, direction, car, User: GameData.SteamUser.Id)) switch
		{
			float time => new(time),
			_ => null
		};

	public int GetCount(bool includeInvalid) => GetConnection(false).ExecuteScalar<int>(
		$"SELECT COUNT({(includeInvalid ? "*" : nameof(ExternalGhostFile.Track))}) FROM {ExternalGhostFile.Table}");

	public IEnumerable<CachedFileInfo> EnumerateFiles() =>
		GetConnection(false).BuildQuery<ExternalGhostFile>()
		.Select(nameof(ExternalGhostFile.Path), nameof(ExternalGhostFile.Size), nameof(ExternalGhostFile.LastWrite))
		.Execute<CachedFileInfo>();

	public bool Contains(ExternalGhostInfo ghost) =>
		GetConnection(false).BuildQuery<ExternalGhostFile>()
		.SelectRaw("1")
		.Where(ghost.Track, ghost.Direction, ghost.Car)
		.When(ghost.Source == GhostSource.Leaderboard,
			query => query.Where(query => query
				.Where(nameof(EGF.SteamFileId), ghost.SteamFileId)
				.Or()
				// match player/export versions of leaderboard ghosts to avoid saving low-quality duplicates of ghosts that are already in the database
				.WhereNull(nameof(EGF.SteamFileId))
				.Where(nameof(EGF.TimeInSeconds), ghost.TimeInSeconds)
				.WhereUser(ghost.SteamUserId.Value)
			),
			query => query.Where(nameof(EGF.RecordingId), ghost.RecordingId)
		)
		.Limit(1)
		.ExecuteScalar<int>() == 1;

	public void Add(ExternalGhostFile ghost) => GetConnection(true).InsertOrReplace(ghost);

	public void AddError(FileInfo file) => GetConnection(true).InsertOrReplace(new CachedFileInfo(file));

	public bool Remove(string path) => GetConnection(true).Delete<ExternalGhostFile>(path) > 0;

	public void Clear() => GetConnection(true).DeleteAll<ExternalGhostFile>();
}
