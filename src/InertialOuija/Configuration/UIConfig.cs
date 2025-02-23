extern alias GameScripts;

using GameScripts.TinyJSON;
using System;

namespace InertialOuija.Configuration;

internal class UIConfig
{
	[Include] public bool ShowErrors { get; set; } = true;

	[Include] public bool HideAchievedTargetTimes { get; set; } = true;
	[Include] public bool ShowGhostTime { get; set; } = true;
	[Include] public bool ShowPersonalBestTime { get; set; } = true;
}
