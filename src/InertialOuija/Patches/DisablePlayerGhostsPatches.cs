extern alias GameScripts;
using GameScripts.Assets.Source.Gameplay;
using GameScripts.Assets.Source.GhostCars.GhostDatabases;
using GameScripts.Assets.Source.GhostCars.GhostLaps;
using GameScripts.Assets.Source.Messaging.Messages;
using GameScripts.Assets.Source.SaveData;
using GameScripts.Assets.Source.Tools;
using HarmonyLib;
using InertialOuija.Ghosts;
using Steamworks;
using System;
using System.Collections.Generic;

namespace InertialOuija.Patches;

[HarmonyPatch]
internal class DisablePlayerGhostsPatches
{
	[HarmonyTranspiler, HarmonyPatch(typeof(SteamSaveManager), "PreloadSequence", MethodType.Enumerator)]
	static IEnumerable<CodeInstruction> SteamSaveManager_Load(IEnumerable<CodeInstruction> instructions)
	{
		var fileExistsMethod = ((Delegate)SteamRemoteStorage.FileExists).Method;
		var fileExistsReplacementMethod = ((Delegate)FileExists).Method;

		foreach (var instruction in instructions)
		{
			if (instruction.Calls(fileExistsMethod))
				yield return new CodeInstruction(instruction) { operand = fileExistsReplacementMethod };
			else
				yield return instruction;
		}

		static bool FileExists(string pchFile)
		{
			if (pchFile == SaveFileKeys.PlayerGhostKey)
				return false;
			else
				return SteamRemoteStorage.FileExists(pchFile);
		}
	}

	[HarmonyPrefix, HarmonyPatch(typeof(SteamSaveManager), nameof(SteamSaveManager.Save))]
	static bool SteamSaveManager_Save(string fileKey)
	{
		if (fileKey == SaveFileKeys.PlayerGhostKey)
		{
			Log.Debug($"Skipping Save({fileKey})");
			return false;
		}
		return true;
	}

	[HarmonyPrefix, HarmonyPatch(typeof(PlayerGhostDatabase), nameof(PlayerGhostDatabase.GetGhostTime))]
	static bool PlayerGhostDatabase_GetGhostTime(GhostKey ghostKey, ref float __result)
	{
		// Called by the lap time popup
		// Called every frame by the score screen
		try
		{
			if (ExternalGhostManager.Ghosts.GetPersonalBestTime(ghostKey.Track, ghostKey.Direction, ghostKey.Car) is GhostTime time)
				__result = time.TimeInSeconds;
			else
				__result = float.MaxValue;
		}
		catch (Exception ex)
		{
			Log.Error(ex);
			__result = float.MaxValue;
		}
		return PrefixPatch.SkipOriginal;
	}

	/// <remarks>
	/// Emulate raising of <see cref="NewFastestGhostMessage"/>.
	/// Vanilla game only uses this for player ghost playback and NPC dialogue.
	/// </remarks>
	[HarmonyPrefix, HarmonyPatch(typeof(PlayerGhostDatabase), nameof(PlayerGhostDatabase.AddGhostRecording))]
	static bool PlayerGhostDatabase_AddGhostRecording(GhostKey ghostKey, GhostLap lap)
	{
		// This should raise a NewFastestGhostMessage, but that's only used by FastestLapGhost (useless with this patch)
		// and a dialogue trigger, so it doesn't seem worth reimplementing.
		return PrefixPatch.SkipOriginal;
	}

	/// <remarks>
	/// Default implementation queries PlayerGhostDatabase rather than RollingStartDatabase.
	/// This leads to strange camera behavior during race start when rolling start is available.
	/// </remarks>
	/// 
	[HarmonyTranspiler, HarmonyPatch(typeof(GameModeManager), nameof(GameModeManager.CanRollingStart))]
	static IEnumerable<CodeInstruction> GameModeManager_CanRollingStart(IEnumerable<CodeInstruction> instructions)
	{
		var playerGhostsGetter = AccessTools.PropertyGetter(typeof(CorePlugin), nameof(CorePlugin.PlayerGhosts));
		var rollingStartsGetter = AccessTools.PropertyGetter(typeof(CorePlugin), nameof(CorePlugin.RollingStartDatabase));

		var playerGhostsHasGhost = AccessTools.Method(typeof(PlayerGhostDatabase), nameof(PlayerGhostDatabase.HasGhost));
		var rollingStartsHasGhost = AccessTools.Method(typeof(RollingStartDatabase), nameof(RollingStartDatabase.HasGhost));

		foreach (var instruction in instructions)
		{
			if (instruction.Calls(playerGhostsGetter))
				yield return new(instruction) { operand = rollingStartsGetter };
			else if (instruction.Calls(playerGhostsHasGhost))
				yield return new(instruction) { operand = rollingStartsHasGhost };
			else
				yield return instruction;
		}
	}
}
