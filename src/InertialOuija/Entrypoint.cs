extern alias GameScripts;

using System.IO;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using InertialOuija;
using InertialOuija.Configuration;
using InertialOuija.Patches;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Doorstop;

internal class Entrypoint
{
	public static void Start()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;

		static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
			try
			{
				Initialize();
			}
			catch (System.Exception ex)
			{
				Log.Error("InertialOuija initialization failed.", ex);
			}
		}
	}

	private static void Initialize()
	{
		var modVersion = typeof(Entrypoint).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
		var modBuild = typeof(Entrypoint).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration;
		var gameVersion = $"{Application.version} {Application.buildGUID}";

		Log.Info($"Mod version {modVersion} {modBuild}");
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

		ModConfig.Config.Patches = patchConfig;

		MainController.Initialize();

		new Thread(() => ApplyPatches(patchConfig)) { IsBackground = true, Name = "Patch Thread" }.Start();
	}

	private static void ApplyPatches(PatchConfig patchConfig)
	{
		var harmony = new Harmony("inertialouija");

		var sw = System.Diagnostics.Stopwatch.StartNew();

		harmony.Apply<SaveGhostPatches>(patchConfig.SaveGhosts);
		harmony.Apply<SaveDownloadedGhostPatches>(patchConfig.SaveGhosts);
		harmony.Apply<GhostPlaybackPatches>(patchConfig.GhostPlayback);
		harmony.Apply<PointToPointGhostStartPatches>(patchConfig.PointToPointGhostStart);
		harmony.Apply<SerializationWhitelistPatches>(patchConfig.SerializationWhitelist);
		harmony.Apply<DownloadDlcGhostPatches>(patchConfig.DownloadDlcGhosts);
		harmony.Apply<UnsavedGhostWarningPatches>(patchConfig.UnsavedGhostWarning);
		harmony.Apply<PrecisionModeTimerPatches>(patchConfig.PrecisionModeTimer);
		harmony.Apply<ReplaceLeaderboardTitlePatches>(patchConfig.ReplaceLeaderboardTitle);
		harmony.Apply<RememberSelectedPalettePatches>(patchConfig.RememberSelectedPalette);
		harmony.Apply<AllowKeyboardReverseBindingPatches>(patchConfig.AllowKeyboardReverseBinding);
		harmony.Apply<FixSplitscreenCarSelectionPatches>(patchConfig.FixSplitscreenCarSelection);
		harmony.Apply<FixSplitscreenStyleCounterPatches>(patchConfig.FixSplitscreenStyleCounter);
		harmony.Apply<IgnoreUnfinishedLapTimesPatches>(patchConfig.IgnoreUnfinishedLapTimes);
		harmony.Apply<TimeAttackHudPatches>(patchConfig.TimeAttackHud);
		harmony.Apply<DisableReplayRecordingPatches>(patchConfig.DisableReplayRecording && !patchConfig.EnableReplay);
		harmony.Apply<EnableReplayPatches>(patchConfig.EnableReplay);

#if DEBUG
		LoggingPatches.Apply(harmony);
#endif

		sw.Stop();
		Log.Info($"Patching took {sw.Elapsed}");
	}
}
