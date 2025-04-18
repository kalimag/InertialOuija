extern alias GameScripts;
using System.Collections.Generic;

namespace InertialOuija.Ghosts;

public class GhostCarComparer : IEqualityComparer<ExternalGhostInfo>, IEqualityComparer<ExternalGhostFile>
{
	public static GhostCarComparer Instance { get; } = new GhostCarComparer();

	public bool Equals(ExternalGhostInfo x, ExternalGhostInfo y)
		=> ReferenceEquals(x, y) || x.Car == y.Car;

	public bool Equals(ExternalGhostFile x, ExternalGhostFile y)
		=> Equals((ExternalGhostInfo)x, (ExternalGhostInfo)y);

	public int GetHashCode(ExternalGhostInfo ghostInfo)
		=> ghostInfo != null ? ghostInfo.Car.GetHashCode() : 0;

	public int GetHashCode(ExternalGhostFile ghostFile)
		=> GetHashCode((ExternalGhostInfo)ghostFile);
}
