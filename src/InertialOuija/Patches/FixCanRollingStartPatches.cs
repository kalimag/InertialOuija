extern alias GameScripts;

using GameScripts.Assets.Source.Gameplay;
using GameScripts.Assets.Source.GhostCars.GhostDatabases;
using GameScripts.Assets.Source.Tools;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace InertialOuija.Patches;

[HarmonyPatch]
internal class FixCanRollingStartPatches
{
	/// <remarks>
	/// Default implementation only queries PlayerGhostDatabase but not RollingStartDatabase.
	/// This leads to strange camera behavior during race start when rolling start is available,
	/// but PlayerGhost is not.
	/// </remarks>
	[HarmonyTranspiler, HarmonyPatch(typeof(GameModeManager), nameof(GameModeManager.CanRollingStart))]
	static IEnumerable<CodeInstruction> GameModeManager_CanRollingStart(IEnumerable<CodeInstruction> instructions)
	{
		var playerGhostsGetter = AccessTools.PropertyGetter(typeof(CorePlugin), nameof(CorePlugin.PlayerGhosts));
		var playerGhostsHasGhost = AccessTools.Method(typeof(PlayerGhostDatabase), nameof(PlayerGhostDatabase.HasGhost));

		// instance CorePlugin.PlayerGhosts.HasGhost(...)
		// to
		// static HasRollingStart(...)
		foreach (var instruction in instructions)
		{
			if (instruction.Calls(playerGhostsGetter))
				continue;
			else if (instruction.Calls(playerGhostsHasGhost))
				yield return new(OpCodes.Call, ((Delegate)HasRollingStart).Method);
			else
				yield return instruction;
		}

		// PlayerGhosts can be used as a fallback for missing rolling starts, so both must be checked
		static bool HasRollingStart(GhostKey ghostKey) => CorePlugin.RollingStartDatabase.HasGhost(ghostKey) || CorePlugin.PlayerGhosts.HasGhost(ghostKey);
	}
}
