extern alias GameScripts;
using GameScripts.TinyJSON;
using InertialOuija.Ghosts;

namespace InertialOuija.Configuration;

internal class GhostConfig
{
	[Include] public ExternalGhostMode Mode { get; set; } = ExternalGhostMode.Default;
	[Include] public string Directory { get; set; } = "";
	[Include] public int Count { get; set; } = 1;
	[Include] public int MaxCount { get; set; } = 30;
	[Include] public CarFilter CarFilter { get; set; } = CarFilter.SameCar;
	[Include] public bool UniqueCars { get; set; } = false;
	[Include] public bool UniqueUsers { get; set; } = false;
	[Include] public bool MyGhosts { get; set; } = false;
	[Include] public bool GhostVisual { get; set; } = true;
	[Include] public bool DisableHeadlights { get; set; } = false;
	[Include] public bool PlayerGhostsExported { get; set; } = false;
}
