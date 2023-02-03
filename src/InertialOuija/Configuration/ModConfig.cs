extern alias GameScripts;
using System;
using System.IO;
using GameScripts.TinyJSON;

namespace InertialOuija.Configuration;

internal class ModConfig
{
    private const string Filename = "config.json";


    private static ModConfig _config;
    public static ModConfig Config => _config ??= Load();



    [Include] public GhostConfig Ghosts { get; private set; } = new GhostConfig();



    private static ModConfig Load()
    {
        var configPath = Path.Combine(FileUtility.ModDirectory, Filename);

        ModConfig config;
        try
        {
            var json = File.ReadAllText(configPath);
            JSON.MakeInto(JSON.Load(json), out config);
        }
        catch (Exception ex)
        {
            Log.Error("Failed to load patch configuration", ex);
            config = new ModConfig();
        }
        return config;
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
