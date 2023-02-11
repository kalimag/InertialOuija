extern alias GameScripts;
using System;
using GameScripts.Assets.Source.GhostCars;
using GameScripts.Assets.Source.GhostCars.GhostLaps;
using GameScripts.Assets.Source.Messaging.Messages;
using HarmonyLib;
using InertialOuija.UI.Components;
using UnityEngine;

namespace InertialOuija.Patches
{
	[HarmonyPatch]
	internal class UnsavedGhostWarningPatches
	{
		[HarmonyPostfix, HarmonyPatch(typeof(GhostRecorder), nameof(GhostRecorder.HandleMessage), typeof(StartedNewLapMessage))]
		static void GhostRecorder_StartedNewLapMessage(GhostRecorder __instance, StartedNewLapMessage message,
			bool ____raceComplete, bool ____inTransitionZone, GhostLap ____currentLap, GhostLap ____previousLap)
		{
			try
			{
				if (message.Lap < 2 || message.Racer != __instance.gameObject)
					return;

				Log.Debug($"raceComplete={____raceComplete} inTransitionZone={____inTransitionZone} Lap={message.Lap} " +
					$"currentLap.RecordingComplete={____currentLap?.RecordingComplete} previousLap.RecordingComplete={____previousLap?.RecordingComplete} ");

				var showWarning = ____raceComplete ? ____currentLap?.RecordingComplete == false :
					____inTransitionZone && ____previousLap.RecordingComplete == false;

				if (showWarning)
				{
					Debug.Log($"Enabling unsaved ghost warning");
					var warningComponent = __instance.GetOrAddComponent<UnsavedGhostWarning>();
					warningComponent.RaceComplete = ____raceComplete;
					warningComponent.enabled = true;
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
		}

		[HarmonyPostfix, HarmonyPatch(typeof(GhostRecorder), "SaveLap")]
		static void GhostRecorder_SaveLap(GhostRecorder __instance)
			=> DisableWarning(__instance);

		[HarmonyPostfix, HarmonyPatch(typeof(GhostRecorder), nameof(GhostRecorder.HandleMessage), typeof(RestartEventMessage))]
		static void GhostRecorder_RestartEventMessage(GhostRecorder __instance)
			=> DisableWarning(__instance);

		private static void DisableWarning(GhostRecorder instance)
		{
			try
			{
				var warningComponent = instance.GetComponent<UnsavedGhostWarning>();
				if (warningComponent && warningComponent.enabled)
				{
					Debug.Log($"Disabling unsaved ghost warning");
					warningComponent.enabled = false;
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
		}
	}
}