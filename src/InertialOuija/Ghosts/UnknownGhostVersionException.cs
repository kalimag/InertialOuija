using System;

namespace InertialOuija.Ghosts;
[Serializable]
internal class UnknownGhostVersionException : Exception
{
	public uint Version { get; }

	public UnknownGhostVersionException(uint version)
		: base($"Ghost file version {version} is unknown.")
	{
		Version = version;
	}
}