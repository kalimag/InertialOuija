extern alias GameScripts;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using GameScripts.Assets.Source.Gameplay.GameModes;
using GameScripts.Assets.Source.Gameplay.Timing;
using GameScripts.Assets.Source.UI;
using HarmonyLib;
using UnityEngine;

namespace InertialOuija.Patches;

/// <summary>
/// Adds total time display to hud and score screen in precision mode.
/// </summary>
[HarmonyPatch]
internal class PrecisionModeTimerPatches
{
	[HarmonyTranspiler, HarmonyPatch(typeof(PrecisionStyleMode), nameof(PrecisionStyleMode.InitialiseGameMode))]
	static IEnumerable<CodeInstruction> PrecisionStyleMode_InitialiseGameMode(IEnumerable<CodeInstruction> instructions)
	{
		var getHudPrefabInfoMethod = SymbolExtensions.GetMethodInfo((GameObject obj) => obj.GetComponent<HudPrefabInfo>());
		var completionScreenField = AccessTools.Field(typeof(HudPrefabInfo), nameof(HudPrefabInfo.CompletionScreen));
		var setActiveDisplaysMethod = AccessTools.Method(typeof(EventCompleteScreen), nameof(EventCompleteScreen.SetActiveDisplays));

		foreach (var instruction in instructions)
		{
			if (instruction.Calls(getHudPrefabInfoMethod))
			{
				// Instantiate timer before it gets setup inside HudPrefabInfo.SetTargetRacer
				yield return instruction;
				yield return new(OpCodes.Dup); // HudPrefabInfo
				yield return new(OpCodes.Call, ((Delegate)CreateLapTimerAndHud).Method);
			}
			else if (instruction.LoadsField(completionScreenField))
			{
				// Only do this after the completion screen has been instantiated
				yield return new(OpCodes.Dup); // HudPrefabInfo
				yield return new(OpCodes.Call, ((Delegate)SetupCompletionScreen).Method);
				yield return instruction;
			}
			else if (instruction.Calls(setActiveDisplaysMethod))
			{
				// replace display total argument with true
				yield return new(OpCodes.Pop);
				yield return new(OpCodes.Pop);
				yield return new(OpCodes.Pop);
				yield return new(OpCodes.Ldc_I4_1);
				yield return new(OpCodes.Ldc_I4_0);
				yield return new(OpCodes.Ldc_I4_0);
				yield return instruction;
			}
			else
			{
				yield return instruction;
			}
		}

		static void CreateLapTimerAndHud(HudPrefabInfo hud)
		{
			try
			{
				Log.Debug(nameof(CreateLapTimerAndHud), nameof(PrecisionStyleMode_InitialiseGameMode));

				if (!hud.Timer)
				{
					hud.Timer = hud.gameObject.GetOrAddComponent<LapTimer>();
					hud.Timer.LapTimes = new(); // assumed to be initialized by prefab
				}

				var totalTimeHudPrefab = GameAssets.GetHudPrefabInfo("DuelHud")
					.HudRoot.transform.Find("Total Time").gameObject;
				var topRightHudRoot = hud.TargetRoot.transform.parent;

				var totalTimeHud = UnityEngine.Object.Instantiate(totalTimeHudPrefab, topRightHudRoot);
				totalTimeHud.GetComponentInChildren<DisplayTotalTime>().LapTimer = hud.Timer;
				totalTimeHud.GetComponent<RectTransform>().IntegrateInLayout(15);
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
		}

		static void SetupCompletionScreen(HudPrefabInfo hud)
		{
			try
			{
				Log.Debug(nameof(SetupCompletionScreen), nameof(PrecisionStyleMode_InitialiseGameMode));

				if (!hud.CompletionScreen.TotalTimeField.LapTimer)
					hud.CompletionScreen.TotalTimeField.LapTimer = hud.Timer;
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
		}
	}
}
