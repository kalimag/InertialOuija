extern alias GameScripts;

using System.IO;
using System.Threading;
using HarmonyLib;
using InertialOuija;
using InertialOuija.Patches;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Doorstop;

internal class Entrypoint
{
	public static void Start()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;

#if DEBUG
		var build = "Debug";
#else
		var build = "Release";
#endif
		var gameVersion = $"{Application.version} {Application.buildGUID}";

		Log.Info($"Mod version {typeof(Entrypoint).Assembly.GetName().Version.ToString(2)} {build}");
		Log.Info($"Game version {gameVersion}");

		var patchConfig = PatchConfig.Load();
		if (gameVersion != PatchConfig.SupportedVersion && gameVersion != patchConfig.AllowVersion)
		{
			Log.Info($"Unsupported game version. Mod will not be enabled.");
			return;
		}

		UnityEngine.CrashReportHandler.CrashReportHandler.enableCaptureExceptions = false;

#if DEBUG
		var harmonyLogPath = Path.GetFullPath("harmony.log");
		FileLog.LogWriter = new StreamWriter(harmonyLogPath, false) { AutoFlush = true };
		Harmony.DEBUG = true;
#endif

		MainController.Initialize();

		new Thread(() => ApplyPatches(patchConfig)) { IsBackground = true }.Start();
	}

	private static void ApplyPatches(PatchConfig patchConfig)
	{
		var harmony = new Harmony("inertialouija");

		var sw = System.Diagnostics.Stopwatch.StartNew();

		if (patchConfig.SaveGhosts)
		{
			ApplyPatches<SaveGhostPatches>();
			ApplyPatches<SaveDownloadedGhostPatches>();
		}
		if (patchConfig.GhostPlayback)
		{
			ApplyPatches<GhostPlaybackPatches>();
		}
		if (patchConfig.PointToPointGhostStart)
		{
			ApplyPatches<PointToPointGhostStartPatches>();
		}
		if (patchConfig.SerializationWhitelist)
		{
			ApplyPatches<SerializationWhitelistPatches>();
		}

#if DEBUG
		LoggingPatches.Apply(harmony);
#endif

		sw.Stop();
		Log.Info($"Patching took {sw.Elapsed}");

		void ApplyPatches<T>() => harmony.CreateClassProcessor(typeof(T)).Patch();
	}
}
