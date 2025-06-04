extern alias GameScripts;

using GameScripts.Assets.Source.Gameplay;
using GameScripts.Assets.Source.GhostCars.GhostDatabases;
using GameScripts.Assets.Source.Tools;
using HarmonyLib;
using System.Collections.Generic;

namespace InertialOuija.Patches;

[HarmonyPatch]
internal class FixCanRollingStartPatches
{
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
