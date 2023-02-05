extern alias GameScripts;

using System;
using System.Collections.Generic;

namespace InertialOuija.Ghosts;

public class RelaxedGhostComparer : IEqualityComparer<ExternalGhostInfo>, IEqualityComparer<ExternalGhostFile>
{
	public static RelaxedGhostComparer Instance { get; } = new RelaxedGhostComparer();

	public bool Equals(ExternalGhostInfo x, ExternalGhostInfo y)
	{
		if (ReferenceEquals(x, y))
			return true;

		// Try to match ghosts from different sources, mainly leaderboard and local ghosts.
		// Only require matching ids if both ghosts have that data available, but require
		// at least one match including username
		return
			x.Track == y.Track &&
			x.Direction == y.Direction &&
			x.Car == y.Car &&
			x.TimeInSeconds == y.TimeInSeconds &&
			EqualsOrNull(x.RecordingId, y.RecordingId) &&
			EqualsOrNull(x.SteamUserId, y.SteamUserId) &&
			EqualsOrNull(x.SteamFileId, y.SteamFileId) &&
			EqualsOrNull(x.LeaderboardId, y.LeaderboardId) &&
			(
				(x.Username == y.Username) ||
				EqualsAndNotNull(x.RecordingId, y.RecordingId) ||
				EqualsAndNotNull(x.SteamUserId, y.SteamUserId) ||
				EqualsAndNotNull(x.SteamFileId, y.SteamFileId)
			);

		static bool EqualsOrNull<T>(T? x, T? y) where T : struct, IEquatable<T>
			=> x == null || y == null || x.Equals(y);
		static bool EqualsAndNotNull<T>(T? x, T? y) where T : struct, IEquatable<T>
			=> x != null && x.Equals(y);
	}
	public bool Equals(ExternalGhostFile x, ExternalGhostFile y)
		=> Equals(x?.Info, y?.Info);

	public int GetHashCode(ExternalGhostInfo ghost)
	{
		if (ghost == null)
			return 0;

		int hashCode = 686532055;
		hashCode = hashCode * -1521134295 + ghost.Track.GetHashCode();
		hashCode = hashCode * -1521134295 + ghost.Direction.GetHashCode();
		hashCode = hashCode * -1521134295 + ghost.Car.GetHashCode();
		hashCode = hashCode * -1521134295 + ghost.TimeInSeconds.GetHashCode();
		return hashCode;
	}

	public int GetHashCode(ExternalGhostFile ghostFile)
		=> GetHashCode(ghostFile?.Info);
}
