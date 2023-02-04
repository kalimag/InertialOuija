extern alias GameScripts;

using System;
using System.IO;
using GameScripts.TinyJSON;

namespace InertialOuija;

internal class PatchConfig
{
	private const string Filename = "patch.json";
	public const string SupportedVersion = "1.1.4 60253fa5d0c82584480f77793739bde2";

	[Include] public string AllowVersion { get; set; }
	[Include] public bool PointToPointGhostStart { get; set; } = true;
	[Include] public bool SerializationWhitelist { get; set; } = true;
	[Include] public bool SaveGhosts { get; set; } = true;
	[Include] public bool GhostPlayback { get; set; } = true;
	[Include] public bool DownloadDlcGhosts { get; set; } = true;
	[Include] public bool UnsavedGhostWarning { get; set; } = true;



	public static PatchConfig Load()
	{
		var configPath = Path.Combine(FileUtility.ModDirectory, Filename);

		PatchConfig config;
		try
		{
			var json = File.ReadAllText(configPath);
			JSON.MakeInto(JSON.Load(json), out config);
		}
		catch (Exception ex)
		{
			Log.Error("Failed to load patch configuration", ex);
			config = new PatchConfig();
		}
		return config;
	}
}
