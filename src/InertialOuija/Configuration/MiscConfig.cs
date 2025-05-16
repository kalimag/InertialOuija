extern alias GameScripts;

using GameScripts.TinyJSON;

namespace InertialOuija.Configuration;

internal class MiscConfig
{
	[Include] public bool ScreenshotNewRecord { get; set; } = false;
	[Include] public string ScreenshotPath { get; set; } = "";
	[Include] public bool SkipIntro { get; set; } = false;

	[Include] public int RollingStartBackup { get; set; } = 0;
}
