extern alias GameScripts;

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using GameScripts.Assets.Source.CarModel;
using GameScripts.Assets.Source.GameData;
using GameScripts.Assets.Source.Gameplay;
using GameScripts.Assets.Source.Gameplay.GameModes;
using GameScripts.Assets.Source.GhostCars;
using GameScripts.Assets.Source.GhostCars.GhostPlayback;
using GameScripts.Assets.Source.Tools;
using HarmonyLib;
using InertialOuija.Ghosts;
using static InertialOuija.Configuration.ModConfig;

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
		Log.Debug($"Prevent ghost spawn: {Config.Ghosts.Mode != ExternalGhostMode.None}");
		return Config.Ghosts.Mode == ExternalGhostMode.None;
	}

	[HarmonyPrefix, HarmonyPatch(typeof(SpawnHelpers), nameof(SpawnHelpers.PlayerGhost))]
	static bool SpawnHelpers_PlayerGhost()
	{
		Log.Debug($"Prevent ghost spawn: {Config.Ghosts.Mode != ExternalGhostMode.None}");
		return Config.Ghosts.Mode == ExternalGhostMode.None;
	}

	[HarmonyReversePatch, HarmonyPatch(typeof(SpawnHelpers), nameof(SpawnHelpers.CloudGhost))]
	public static CarProperties SpawnGhost(GhostPlayer basePrefab, CarSetupDetails carDetails, IGhostRecording ghostRecording, int carId)
	{
		throw new NotImplementedException();
	}

	[HarmonyReversePatch, HarmonyPatch(typeof(SpawnHelpers), nameof(SpawnHelpers.CloudGhost))]
	public static CarProperties SpawnCar(GhostPlayer basePrefab, CarSetupDetails carDetails, IGhostRecording ghostRecording, int carId)
	{
		// Patched version of SpawnHelpers.CloudGhost that uses the regular car visual
		IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var ghostCarField = typeof(CarVisualProperties).GetField(nameof(CarVisualProperties.GhostCar));

			foreach (var instruction in instructions)
			{
				if (instruction.LoadsField(ghostCarField))
				{
					// don't load ghost prefab so it falls back to regular visual
					yield return new(OpCodes.Pop);
					yield return new(OpCodes.Ldnull);
				}
				else
				{
					yield return instruction;
				}
			}
		}

		_ = Transpiler(null);
		throw new NotImplementedException();
	}
}
