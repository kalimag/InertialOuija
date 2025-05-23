extern alias GameScripts;

using GameScripts.Assets.Source.CarModel;
using GameScripts.Assets.Source.Enums;
using GameScripts.Assets.Source.Gameplay;
using GameScripts.Assets.Source.Gameplay.Timing;
using GameScripts.Assets.Source.Localisation;
using GameScripts.Assets.Source.Messaging.Messages;
using GameScripts.Assets.Source.Tools;
using HarmonyLib;
using InertialOuija.Ghosts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using static InertialOuija.Configuration.ModConfig;

namespace InertialOuija.Patches;

[HarmonyPatch]
internal class AutoScreenshotPatches
{
	[HarmonyTranspiler, HarmonyPatch(typeof(LapCompletePopup), nameof(LapCompletePopup.HandleMessage), typeof(LapTimeMessage))]
	static IEnumerable<CodeInstruction> LapCompletePopup_HandleLapTimeMessage(IEnumerable<CodeInstruction> instructions)
	{
		var newRecordTriggerField = AccessTools.Field(typeof(LapCompletePopup), nameof(LapCompletePopup.NewRecordTrigger));
		var animatorSetTriggerMethod = SymbolExtensions.GetMethodInfo((Animator animator) => animator.SetTrigger(""));
		var createScreenshotsMethod = ((Delegate)CaptureLapTimePopup).Method;

		return new CodeMatcher(instructions)
			.MatchEndForward(
				CodeMatch.LoadsField(newRecordTriggerField),
				CodeMatch.Calls(animatorSetTriggerMethod)
			)
			.ThrowIfInvalid("SetTrigger(NewRecordTrigger) not found")
			.InsertAfterAndAdvance(
				new CodeInstruction(OpCodes.Ldarg_0), // this
				new CodeInstruction(OpCodes.Ldarg_1), // LapTimeMessage
				new CodeInstruction(OpCodes.Call, createScreenshotsMethod)
			)
			.Instructions();
	}

	private static void CaptureLapTimePopup(LapCompletePopup lapTimePopup, LapTimeMessage message)
	{
		try
		{
			if (!Config.Misc.ScreenshotNewRecord)
				return;

			Log.Debug($"Create screenshot of LapCompletePopup (#{message.LapIndex} {message.LapTime})");

			var lapTimeText = lapTimePopup.transform.Find("Rounded Box/Time")?.GetComponent<TMP_Text>();
			var newRecordText = lapTimePopup.transform.Find("New Record/Title")?.GetComponent<TextLocaliser>();
			if (!lapTimeText || !newRecordText)
				return;

			var car = message.Racer.GetComponentInChildren<CarVisualProperties>().Car;
			var name = GetName(TrackInfo.CurrentTrack(), CorePlugin.GameModeManager.TrackDirection, car, message.LapTime);

			MainController.StartCoroutine(WaitAndCapture(name, lapTimeText, newRecordText));
		}
		catch (Exception ex)
		{
			Log.Error(ex);
		}

		static IEnumerator WaitAndCapture(string name, TMP_Text lapTimeText, TextLocaliser newRecordText)
		{
			const float MaxWaitTime = 3f;
			const int MinVisibleCharacters = 8; // "00:00:00"

			// wait until lap time popup is fully displayed
			var elapsed = 0f;
			while (true)
			{
				if (!lapTimeText || elapsed > MaxWaitTime)
					yield break;
				if (lapTimeText.isActiveAndEnabled && lapTimeText.maxVisibleCharacters >= MinVisibleCharacters && newRecordText.isActiveAndEnabled && newRecordText.Alpha == 1)
					break;
				elapsed += Time.deltaTime;
				yield return null;
			}

			CaptureScreenshot(name);
		}
	}


	[HarmonyPostfix, HarmonyPatch(typeof(EventCompleteScreen), nameof(EventCompleteScreen.HandleMessage), typeof(CompletedRaceMessage))]
	static void EventCompleteScreen_HandleMessage(EventCompleteScreen __instance, CompletedRaceMessage message)
	{
		try
		{
			if (!Config.Misc.ScreenshotNewRecord)
				return;
			if (__instance.WaitingForResults.activeSelf || message.Racer != __instance.TargetRacer || !__instance.LapTimeRoot.activeSelf || !__instance.TargetHudInfo.Timer)
				return;

			var root = __instance.Winner.activeSelf ? __instance.Winner : __instance.Loser; // I think these are always the same object? Just in case
			var animator = root.GetComponent<Animator>();

			var track = TrackInfo.CurrentTrack();
			var direction = CorePlugin.GameModeManager.TrackDirection;
			var car = __instance.TargetRacer.GetComponentInChildren<CarVisualProperties>().Car;

			var personalBestTime = CorePlugin.PlayerGhosts.GetGhostTime(new(Character.None, track, car, direction));
			var bestLapTime = __instance.TargetHudInfo.Timer.LapTimes.Min(); // not using GetBestLapTime() because that carries over between restarts

			Log.Debug($"EventCompleteScreen: PB={personalBestTime} Lap={bestLapTime}");
			if (bestLapTime <= personalBestTime)
			{
				var name = GetName(track, direction, car, bestLapTime);
				MainController.StartCoroutine(WaitAndCapture(name, animator));
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex);
			return;
		}

		static IEnumerator WaitAndCapture(string name, Animator animator)
		{
			const float TargetProgress = 0.25f / 0.32f; // end of slide-in animation

			while (true)
			{
				if (!animator)
					yield break;
				if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= TargetProgress)
					break;
				yield return null;
			}

			CaptureScreenshot(name);
		}
	}


	private static string GetName(Track track, TrackDirection direction, Car car, float timeInSeconds)
	{
		var time = new GhostTime(timeInSeconds);
		return $"{track.GetName(direction, true)} - {car.GetName()} - {time.ToString(true, ".")} ({DateTime.Now:yyyy-MM-dd HH-mm})";
	}

	private static void CaptureScreenshot(string name)
	{
		try
		{
			var directory = FileUtility.ScreenshotDirectory;
			Directory.CreateDirectory(directory);
			ScreenCapture.CaptureScreenshot(Path.Combine(directory, $"{name}.png"));
		}
		catch (Exception ex)
		{
			Log.Error(ex);
		}
	}
}
