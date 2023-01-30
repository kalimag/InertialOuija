extern alias GameScripts;
using System;
using GameScripts.Assets.Source.Gameplay;
using GameScripts.Assets.Source.GhostCars;
using GameScripts.Assets.Source.GhostCars.GhostLaps;
using GameScripts.Assets.Source.GhostCars.GhostPlayback;
using GameScripts.Assets.Source.Messaging.Messages;
using GameScripts.Assets.Source.Tools;
using HarmonyLib;

namespace InertialOuija.Patches;

[HarmonyPatch]
internal class PointToPointGhostStartPatches
{

	[HarmonyPrefix, HarmonyPatch(typeof(FastestLapGhost), nameof(FastestLapGhost.HandleMessage), typeof(RaceStartMessage))]
	static void FastestLapGhost_RaceStartMessage(FastestLapGhost __instance, IGhostRecording ____recording)
	{
		try
		{
			Log.Debug($"IsEventStartLap={____recording?.IsEventStartLap()} ShouldRollingStart={CorePlugin.GameModeManager.ShouldRollingStart()} Circuit={TrackInfo.CurrentTrackInfo().IsCircuit}");

			if (StartFixNeeded(____recording))
			{
				Log.Debug($"Forcing ghost playback");
				__instance.StartPlayback();
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex);
		}
	}

	[HarmonyPrefix, HarmonyPatch(typeof(FastestLapGhost), nameof(FastestLapGhost.HandleMessage), typeof(CrossedStartLineMessage))]
	static bool FastestLapGhost_CrossedStartLineMessage(IGhostRecording ____recording)
	{
		try
		{
			if (StartFixNeeded(____recording))
			{
				Log.Debug($"Preventing ghost playback");
				return false;
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex);
		}
		return true;
	}

	private static bool StartFixNeeded(IGhostRecording recording)
	{
		return recording is GhostLap && !recording.IsEventStartLap() && !TrackInfo.CurrentTrackInfo().IsCircuit;
	}
}