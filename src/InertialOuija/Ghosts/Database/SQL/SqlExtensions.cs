extern alias GameScripts;

using GameScripts.Assets.Source.Enums;
using SQLite;
using SqlKata;
using SqlKata.Compilers;
using System.Collections.Generic;

namespace InertialOuija.Ghosts.Database.SQL;

internal static class SqlExtensions
{
	private static readonly Compiler Compiler = new SqliteCompiler();

	public static Query Where(this Query query, Track track, TrackDirection direction, Car? car)
	{
		query.Where(nameof(ExternalGhostFile.Track), track);
		query.Where(nameof(ExternalGhostFile.Direction), direction);
		if (car != null)
			query.Where(nameof(ExternalGhostFile.Car), car);
		return query;
	}

	public static Query WhereUser(this Query query, ulong steamUserId)
	{
		if (UserData.GetFallbackUsername(steamUserId) is string fallbackName)
		{
			return query.Where(query => query
				.Where(nameof(ExternalGhostInfo.SteamUserId), steamUserId)
				.Or()
				.WhereNull(nameof(ExternalGhostInfo.SteamUserId))
				.Where(nameof(ExternalGhostInfo.Username), fallbackName)
			);
		}
		else
		{
			return query.Where(nameof(ExternalGhostInfo.SteamUserId), steamUserId);
		}
	}


	public static Query BuildQuery(this SQLiteConnection connection)
		=> new SQLiteQuery(connection);

	public static Query BuildQuery<T>(this SQLiteConnection connection)
		=> new SQLiteQuery(connection).From(connection.GetMapping<T>().TableName);


	public static T ExecuteScalar<T>(this Query query)
	{
		var compiledQuery = Compiler.Compile(query);
		return GetConnection(query).ExecuteScalar<T>(compiledQuery.Sql, compiledQuery.Bindings.ToArray());
	}

	public static IEnumerable<T> Execute<T>(this Query query)
	{
		var compiledQuery = Compiler.Compile(query);
		return GetConnection(query).DeferredQuery<T>(compiledQuery.Sql, compiledQuery.Bindings.ToArray());
	}


	private static SQLiteConnection GetConnection(Query query) => ((SQLiteQuery)query).Connection;

	public sealed class SQLiteQuery(SQLiteConnection connection) : Query
	{
		public SQLiteConnection Connection { get; } = connection;

		public override Query NewQuery() => new SQLiteQuery(Connection);
	}
}
