using System;
using System.Collections.Generic;

namespace InertialOuija.Ghosts;

public abstract class GhostComparer : IEqualityComparer<ExternalGhostInfo>, IEqualityComparer<ExternalGhostFile>
{
	public bool Equals(ExternalGhostInfo x, ExternalGhostInfo y)
		=> ReferenceEquals(x, y) || (x != null && y != null && EqualsInternal(x, y));
	public int GetHashCode(ExternalGhostInfo ghostInfo)
		=> ghostInfo is not null ? GetHashCodeInternal(ghostInfo) : 0;

	public bool Equals(ExternalGhostFile x, ExternalGhostFile y)
		=> Equals((ExternalGhostInfo)x, (ExternalGhostInfo)y);
	public int GetHashCode(ExternalGhostFile ghostFile)
		=> GetHashCode((ExternalGhostInfo)ghostFile);

	protected abstract bool EqualsInternal(ExternalGhostInfo x, ExternalGhostInfo y);
	protected abstract int GetHashCodeInternal(ExternalGhostInfo ghostInfo);

	protected static bool EqualsOrNull<T>(T? x, T? y) where T : struct, IEquatable<T>
		=> x == null || y == null || x.Equals(y);
	protected static bool EqualsAndNotNull<T>(T? x, T? y) where T : struct, IEquatable<T>
		=> x != null && x.Equals(y);
}
