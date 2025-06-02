extern alias GameScripts;

using GameScripts.Assets.Source.Gameplay.GameModes;
using GameScripts.Assets.Source.Gameplay.Timing;
using GameScripts.Assets.Source.GhostCars.GhostDatabases;
using GameScripts.Assets.Source.Messaging.Messages;
using GameScripts.Assets.Source.Tools;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace InertialOuija.Patches;

[HarmonyPatch]
internal class DelaySavingAfterRacePatches
{
	[HarmonyTranspiler, HarmonyPatch(typeof(EventCompleteScreen), nameof(EventCompleteScreen.HandleMessage), typeof(CompletedRaceMessage))]
	static IEnumerable<CodeInstruction> EventCompleteScreen_HandleMessage(IEnumerable<CodeInstruction> instructions)
	{
		var playerGhostSaveMethod = SymbolExtensions.GetMethodInfo((PlayerGhostDatabase db) => db.Save());
		var rollingStartSaveMethod = SymbolExtensions.GetMethodInfo((RollingStartDatabase db) => db.Save());

		foreach (var instruction in instructions)
		{
			if (instruction.Calls(playerGhostSaveMethod) || instruction.Calls(rollingStartSaveMethod))
				yield return new(OpCodes.Pop);
			else
				yield return instruction;
		}
	}

	[HarmonyPostfix, HarmonyPatch(typeof(ModeStarter), nameof(ModeStarter.HandleMessage), typeof(QuitEventMessage))]
	static void ModeStarter_HandleQuitEventMessage()
	{
		try
		{
			CorePlugin.PlayerGhosts.Save();
			CorePlugin.RollingStartDatabase.Save();
		}
		catch (Exception ex)
		{
			Log.Error(ex);
		}
	}
}
