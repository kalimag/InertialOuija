extern alias GameScripts;
using System;
using System.IO;
using GameScripts.TinyJSON;

namespace InertialOuija.Configuration;

internal class ModConfig
{
	private const string Filename = "config.json";


	private static ModConfig _config;
	public static ModConfig Config => _config ?? Load();



	[Include] public GhostConfig Ghosts { get; private set; } = new();
	[Include] public UIConfig UI { get; private set; } = new();
	[Include] public CustomizationConfig Customization { get; private set; } = new();

	public PatchConfig Patches { get; internal set; }



	private static ModConfig Load()
	{
		var configPath = Path.Combine(FileUtility.ModDirectory, Filename);

		try
		{
			var json = File.ReadAllText(configPath);
			JSON.MakeInto(JSON.Load(json), out _config);
			if (_config == null)
				throw new InvalidDataException("Unknown JSON error");
		}
		catch (Exception ex)
		{
			_config = new ModConfig();
			_config.Save();
			if (ex is not FileNotFoundException)
				Log.Error("Failed to load mod configuration", ex);
		}
		return _config;
	}

	public void Save()
	{
		var configPath = Path.Combine(FileUtility.ModDirectory, Filename);

		var json = JSON.Dump(this, EncodeOptions.PrettyPrint | EncodeOptions.NoTypeHints);

		try
		{
			File.WriteAllText(configPath, json);
		}
		catch (Exception ex)
		{
			Log.Error("Failed to save configuration", ex);
		}
	}
}
