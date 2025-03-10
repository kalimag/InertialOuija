extern alias GameScripts;

using GameScripts;
using GameScripts.Assets.Source.Tools;
using HarmonyLib;
using System;
using TMPro;

namespace InertialOuija.Patches;

[HarmonyPatch]
internal class ReplaceLeaderboardTitlePatches
{
	[HarmonyPostfix, HarmonyPatch(typeof(LeaderboardDisplay), nameof(LeaderboardDisplay.RefreshBoard))]
	static void LeaderboardDisplay_RefreshBoard(LeaderboardDisplay __instance)
	{
		try
		{
			var title = __instance.transform.parent.Find("Title Bar/TitleText");
			if (title && title.GetComponent<TMP_Text>() is { } textComponent)
			{
				// see LeaderboardDisplay.PopulateRequest
				var track = CorePlugin.GameModeManager.GetRealCurrentTrack();
				var direction = CorePlugin.GameModeManager.TrackDirection;
				textComponent.text = $"LEADERBOARDS<space=0.75em> <nobr><size=80%>{track.GetName(direction)}";
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex);
		}
	}
}
