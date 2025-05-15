extern alias GameScripts;

using GameScripts.Assets.Source.Gameplay.Timing;
using GameScripts.Assets.Source.GhostCars;
using GameScripts.Assets.Source.Messaging.Messages;
using HarmonyLib;
using System;

namespace InertialOuija.Patches;

[HarmonyPatch]
internal class IgnoreUnfinishedLapTimesPatches
{
	[HarmonyPrefix, HarmonyPatch(typeof(LapTimer), nameof(LapTimer.HandleMessage), typeof(StartedNewLapMessage))]
	public static bool LapTimer_StartedNewLapMessage(LapTimer __instance, StartedNewLapMessage message, bool ____running)
	{
		try
		{
			if (message.Racer == __instance.TargetRacer && message.Lap > 1 && !____running)
			{
#if DEBUG
				Log.Warning(
#else
				Log.Info(
#endif
				$"""
				Ignoring StartedNewLapMessage on stopped LapTimer

				Lap={message.Lap}
				GetCurrentLapTime={__instance.GetCurrentLapTime()}
				BestLapTime={__instance.GetBestLapTime()}
				GhostRecorder.FastestLap={__instance.TargetRacer.GetComponent<GhostRecorder>()?.FastestLap?.GetTotalTime()}
				""");
				return PrefixPatch.SkipOriginal;
			}
			else
			{
				return PrefixPatch.RunOriginal;
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex);
			return PrefixPatch.RunOriginal;
		}
	}
}
