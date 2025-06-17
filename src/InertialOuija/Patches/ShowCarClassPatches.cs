extern alias GameScripts;

using GameScripts.Assets.Source.CarModel;
using GameScripts.Assets.Source.Enums;
using GameScripts.Assets.Source.Tools;
using GameScripts.Assets.Source.UI.Menus;
using HarmonyLib;
using System;

namespace InertialOuija.Patches;

[HarmonyPatch]
internal class ShowCarClassPatches
{
	[HarmonyPostfix, HarmonyPatch(typeof(CharacterSelectButton), nameof(CharacterSelectButton.UpdateButtonDisplays))]
	static void CharacterSelectButton_UpdateButtonDisplays(CharacterSelectButton __instance)
	{
		try
		{
			if (__instance.Subtitle != null && __instance.Character != Character.None)
			{
				__instance.Subtitle.text = CorePlugin.CharacterDatabase.GetCharacterData(__instance.Character).CarPrefab.Details.Class switch
				{
					PerformanceClassification.Sport => "Class C - Sport",
					PerformanceClassification.Performance => "Class B - Performance",
					PerformanceClassification.Race => "Class A - Race",
					_ => ""
				};
				__instance.Subtitle.gameObject.SetActive(true);
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex);
		}
	}
}
