extern alias GameScripts;

using System;
using GameScripts.Assets.Source.CarModel;
using GameScripts.Assets.Source.GameData;
using GameScripts.Assets.Source.Gameplay;
using GameScripts.Assets.Source.Gameplay.GameModes;
using GameScripts.Assets.Source.GhostCars;
using GameScripts.Assets.Source.GhostCars.GhostPlayback;
using GameScripts.Assets.Source.Tools;
using HarmonyLib;
using InertialOuija.Ghosts;

namespace InertialOuija.Patches;

[HarmonyPatch]
internal class GhostPlaybackPatches
{
	[HarmonyPrefix, HarmonyPatch(typeof(TimeAttack), nameof(TimeAttack.InitialiseGameMode))]
	static void TimeAttack_InitialiseGameMode(GhostPlayer ___AiOpponent, EventDetails ___DefaultEvent)
	{
		try
		{
			var eventDetails = CorePlugin.GameModeManager.CurrentEventDetails ?? ___DefaultEvent;
			Log.Debug($"TimeAttack.InitialiseGameMode: UseLeaderboardGhost={eventDetails.UseLeaderboardGhost} GhostMode={CorePlugin.GameModeManager.GhostMode} " +
				$"AiOpponent={___AiOpponent} StoryMission={eventDetails.StoryMission}", nameof(GhostPlaybackPatches));
			if (eventDetails.UseLeaderboardGhost && CorePlugin.GameModeManager.GhostMode != 0 && ___AiOpponent != null)
			{
				GhostController.SpawnExternalGhosts(___AiOpponent);
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex);
		}
	}

	[HarmonyPostfix, HarmonyPatch(typeof(FreeDriveMode), nameof(FreeDriveMode.InitialiseGameMode))]
	static void FreeDrive_InitialiseGameMode(GhostPlayer ___GhostCar)
	{
		try
		{
			Log.Debug(nameof(FreeDrive_InitialiseGameMode), nameof(GhostPlaybackPatches));
			if (___GhostCar != null)
				GhostController.SpawnExternalGhosts(___GhostCar);
		}
		catch (Exception ex)
		{
			Log.Error(ex);
		}
	}

	[HarmonyPrefix, HarmonyPatch(typeof(SpawnHelpers), nameof(SpawnHelpers.CloudGhost))]
	static bool SpawnHelpers_CloudGhost()
	{
		Log.Debug(nameof(SpawnHelpers_CloudGhost), nameof(GhostPlaybackPatches));
		return GhostController.UseVanillaGhosts;
	}

	[HarmonyPrefix, HarmonyPatch(typeof(SpawnHelpers), nameof(SpawnHelpers.PlayerGhost))]
	static bool SpawnHelpers_PlayerGhost()
	{
		Log.Debug(nameof(SpawnHelpers_PlayerGhost), nameof(GhostPlaybackPatches));
		return GhostController.UseVanillaGhosts;
	}

	[HarmonyReversePatch, HarmonyPatch(typeof(SpawnHelpers), nameof(SpawnHelpers.CloudGhost))]
	public static CarProperties SpawnGhost(GhostPlayer basePrefab, CarSetupDetails carDetails, IGhostRecording ghostRecording, int carId)
	{
		throw new NotImplementedException();
	}
}
