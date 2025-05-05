extern alias GameScripts;

using GameScripts.TinyJSON;

namespace InertialOuija.Configuration;

internal class UIConfig
{
	[Include] public bool ShowErrors { get; set; } = true;
	[Include] public float ModScale { get; set; } = 1f;

	[Include] public bool HideAchievedTargetTimes { get; set; } = true;
	[Include] public bool ShowGhostTime { get; set; } = true;
	[Include] public bool ShowPersonalBestTime { get; set; } = true;
	[Include] public bool ShowChosenGhosts { get; set; } = false;
}
