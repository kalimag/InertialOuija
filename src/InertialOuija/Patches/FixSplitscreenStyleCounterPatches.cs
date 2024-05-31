extern alias GameScripts;

using System;
using GameScripts.Assets.Source.CarModel;
using GameScripts.Assets.Source.Gameplay.Scoring;
using GameScripts.Assets.Source.Messaging.Messages;
using GameScripts.Assets.Source.Tools;
using HarmonyLib;

namespace InertialOuija.Patches;

[HarmonyPatch]
internal class FixSplitscreenStyleCounterPatches
{
	[HarmonyPrefix, HarmonyPatch(typeof(DriftScoreTracker), nameof(DriftScoreTracker.HandleMessage), typeof(CompletedRaceMessage))]
	static bool DriftScoreTracker_HandleCompletedRaceMessage(CompletedRaceMessage message, CarProperties ___Setup)
	{
		try
		{
			if (CorePlugin.GameModeManager.ActivePlayers > 1 && message.Racer != ___Setup.gameObject)
			{
				Log.Debug($"Suppressing other racer's CompletedRaceMessage");
				return false;
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex);
		}
		return true;
	}
}
