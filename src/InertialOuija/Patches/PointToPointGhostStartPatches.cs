extern alias GameScripts;
using System;
using GameScripts.Assets.Source.Gameplay;
using GameScripts.Assets.Source.GhostCars;
using GameScripts.Assets.Source.GhostCars.GhostLaps;
using GameScripts.Assets.Source.GhostCars.GhostPlayback;
using GameScripts.Assets.Source.Messaging.Messages;
using HarmonyLib;

namespace InertialOuija.Patches;

/// <remarks>
/// The <see cref="IGhostRecording.IsEventStartLap"/> (<see cref="GhostSector.EventStartSector"/>) flag is lost during leaderboard compression.
/// This results in ghost playback starting when the player crosses the starting line, even though a ghost recording may contain the acceleration from
/// standstill, leading to a permanent delay in playback, obvious in point-to-point races.
/// This technically affects circuit race ghosts without rolling starts as well, but their status is ambiguous and those ghosts are not competitive anyway.
/// </remarks>
[HarmonyPatch]
internal class PointToPointGhostStartPatches
{

	[HarmonyPrefix, HarmonyPatch(typeof(FastestLapGhost), nameof(FastestLapGhost.HandleMessage), typeof(RaceStartMessage))]
	static void FastestLapGhost_RaceStartMessage(FastestLapGhost __instance, IGhostRecording ____recording)
	{
		try
		{
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