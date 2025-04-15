extern alias GameScripts;

using System;
using System.Collections.Generic;
using System.Linq;
using GameScripts.Assets.Source.GameData;
using GameScripts.Assets.Source.Gameplay;
using GameScripts.Assets.Source.Gameplay.GameModes;
using GameScripts.Assets.Source.Tools;
using GameScripts.Assets.Source.UI;
using HarmonyLib;
using UnityEngine;
using InertialOuija.Ghosts;
using static InertialOuija.Configuration.ModConfig;
using InertialOuija.Components;

namespace InertialOuija.Patches;

[HarmonyPatch]
internal class TimeAttackHudPatches
{
	private static bool _hideTargetTimes;

	[HarmonyPostfix, HarmonyPatch(typeof(TimeAttack), nameof(TimeAttack.InitialiseGameMode))]
	static void TimeAttack_InitialiseGameMode(TrackInfo trackInfo, EventDetails ___DefaultEvent, Dictionary<GameObject, HudPrefabInfo> ____hudInfos)
	{
		EventDetails eventDetails = CorePlugin.GameModeManager.CurrentEventDetails ?? ___DefaultEvent;
		if (CorePlugin.GameModeManager.ActivePlayers != 1 || !eventDetails.UseLeaderboardGhost)
		{
			_hideTargetTimes = false;
			return;
		}

		var personalBest = ExternalGhostManager.Ghosts.GetPersonalBestTime(CorePlugin.GameModeManager.CurrentTrack, CorePlugin.GameModeManager.TrackDirection,
			CorePlugin.GameModeManager.PlayerInformation[0].CarPrefab.Car);

		UpdateHideTargetTimes(eventDetails, ____hudInfos, personalBest);
		AddPersonalBestTime(____hudInfos, personalBest);
	}

	private static void AddPersonalBestTime(Dictionary<GameObject, HudPrefabInfo> hudInfos, GhostTime? personalBest)
	{
		if (!Config.UI.ShowPersonalBestTime && !Config.UI.ShowGhostTime)
			return;

		try
		{
			var hud = hudInfos.Values.Single();
			var topRightHudRoot = hud.RivalRoot.transform.parent;

			if (Config.UI.ShowPersonalBestTime && personalBest is not null)
			{
				var rivalTimeHud = GameAssets.CreateRivalTimeHud(topRightHudRoot, 1);
				rivalTimeHud.SetText("personal best", GameData.SteamUser.Name, personalBest.Value);
				rivalTimeHud.SetActive(true);
			}

			if (Config.UI.ShowGhostTime)
				RivalTimeHud.FastestGhostHud = GameAssets.CreateRivalTimeHud(topRightHudRoot, 1);
		}
		catch (Exception ex)
		{
			Log.Error(ex);
		}
	}

	private static void UpdateHideTargetTimes(EventDetails eventDetails, Dictionary<GameObject, HudPrefabInfo> hudInfos, GhostTime? personalBest)
	{
		try
		{
			float bronze = eventDetails.BronzeTargetSecondsValue;
			float silver = eventDetails.SilverTargetSecondsValue;
			float gold = eventDetails.GoldTargetSecondsValue;

			Log.Debug($"PB={personalBest} Gold={gold} Silver={silver} Bronze={bronze}");

			_hideTargetTimes = Config.UI.HideAchievedTargetTimes &&
				personalBest is GhostTime pb &&
				(gold == 0 || pb.TimeInSeconds < gold) &&
				(silver == 0 || pb.TimeInSeconds < silver) &&
				(bronze == 0 || pb.TimeInSeconds < bronze);

			if (_hideTargetTimes)
				hudInfos.Values.Single().ActiveTargetDisplays(false, false, false);
		}
		catch (Exception ex)
		{
			Log.Error(ex);
			_hideTargetTimes = false;
		}
	}

	[HarmonyPrefix, HarmonyPatch(typeof(TimeAttack), nameof(TimeAttack.Update))]
	static bool TimeAttack_Update()
	{
		// TimeAttack.Update() only updates the display of the target times
		return !_hideTargetTimes;
	}

}
