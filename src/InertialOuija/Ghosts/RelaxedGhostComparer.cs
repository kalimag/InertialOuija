namespace InertialOuija.Ghosts;

public sealed class RelaxedGhostComparer : GhostComparer
{
	public static RelaxedGhostComparer Instance { get; } = new RelaxedGhostComparer();

	protected override bool EqualsInternal(ExternalGhostInfo x, ExternalGhostInfo y)
	{
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
	}

	protected override int GetHashCodeInternal(ExternalGhostInfo ghost)
	{
		int hashCode = 686532055;
		hashCode = hashCode * -1521134295 + ghost.Track.GetHashCode();
		hashCode = hashCode * -1521134295 + ghost.Direction.GetHashCode();
		hashCode = hashCode * -1521134295 + ghost.Car.GetHashCode();
		hashCode = hashCode * -1521134295 + ghost.TimeInSeconds.GetHashCode();
		return hashCode;
	}
}
