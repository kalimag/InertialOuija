extern alias GameScripts;

using System;
using System.Linq;
using GameScripts.Assets.Source.CameraScripts;
using GameScripts.Assets.Source.Gameplay;
using GameScripts.Assets.Source.Gameplay.Timing;
using GameScripts.Assets.Source.Messaging.Messages;
using GameScripts.Assets.Source.Tools;
using HarmonyLib;

namespace InertialOuija.Patches;

[HarmonyPatch]
internal class EnableReplayPatches
{
	[HarmonyPostfix, HarmonyPatch(typeof(EventCompleteScreen), nameof(EventCompleteScreen.OnEnable))]
	static void EventCompleteScreen_OnEnable(EventCompleteScreen __instance)
	{
		try
		{
			var startReplayButton = __instance.ButtonsRoot.GetComponentInChildren<StartReplayButton>(true);
			if (startReplayButton)
			{
				// Menu items are logically sorted by their initial Y position at first, but visually arranged by their sibling order later
				// All the other buttons have the same initial position, make the replay button match to fix the sorting
				startReplayButton.transform.SetPositionAndRotation(startReplayButton.transform.parent.GetChild(0).transform.position, startReplayButton.transform.rotation);
				startReplayButton.gameObject.SetActive(true);
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex);
		}
	}

	[HarmonyPostfix, HarmonyPatch(typeof(ReplayCameraSetup), nameof(ReplayCameraSetup.HandleMessage), typeof(StartReplayMessage))]
	static void ReplayCameraSetup_HandleStartReplayMessage(ReplayCameraController ___Controller)
	{
		try
		{
			var racer1 = CorePlugin.EventManager.AllRacers.FirstOrDefault();
			var racer2 = CorePlugin.EventManager.AllRacers.Skip(1).FirstOrDefault();

			// allow switching replay cam to ghosts
			foreach (var carProperties in CorePlugin.EventManager.AllCars)
			{
				if (carProperties == racer1 || carProperties == racer2 || !carProperties.ReplayRig)
					continue;

				if (racer1)
					carProperties.ReplayRig.AddOtherTarget(racer1);
				___Controller.ReplayCameras.Add(carProperties.ReplayRig);
				carProperties.ReplayRig.gameObject.SetActive(true);
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex);
		}
	}
}
