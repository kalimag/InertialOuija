extern alias GameScripts;
using GameScripts.TinyJSON;
using InertialOuija.Ghosts;

namespace InertialOuija.Configuration;

internal class GhostConfig
{
	public ExternalGhostMode Mode { get; set; } = ExternalGhostMode.None;
	[Include] public string Directory { get; set; }
	[Include] public int Count { get; set; } = 1;
	[Include] public int MaxCount { get; set; } = 30;
	[Include] public bool SameCar { get; set; } = true;
	[Include] public bool UniqueCars { get; set; } = true;
	[Include] public bool TrackOrderPrefix { get; set; } = true;
}
