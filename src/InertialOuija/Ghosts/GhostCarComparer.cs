namespace InertialOuija.Ghosts;

public sealed class GhostCarComparer : GhostComparer
{
	public static GhostCarComparer Instance { get; } = new GhostCarComparer();

	protected override bool EqualsInternal(ExternalGhostInfo x, ExternalGhostInfo y) => x.Car == y.Car;

	protected override int GetHashCodeInternal(ExternalGhostInfo ghostInfo) => ghostInfo.Car.GetHashCode();
}
