namespace InertialOuija.Ghosts;

public sealed class GhostUserComparer : GhostComparer
{
	public static GhostUserComparer Instance { get; } = new GhostUserComparer();

	protected override bool EqualsInternal(ExternalGhostInfo x, ExternalGhostInfo y) => GetUserId(x) == GetUserId(y);

	protected override int GetHashCodeInternal(ExternalGhostInfo ghostInfo) => GetUserId(ghostInfo)?.GetHashCode() ?? 0;

	private static ulong? GetUserId(ExternalGhostInfo ghost) => ghost.SteamUserId ?? UserData.GetFallbackUserId(ghost.Username);
}
