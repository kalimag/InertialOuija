extern alias GameScripts;

using System.Collections.Generic;
using System.Reflection.Emit;
using GameScripts.Assets.Source.GhostCars;
using GameScripts.Assets.Source.GhostCars.GhostLaps;
using HarmonyLib;

namespace InertialOuija.Patches;

[HarmonyPatch]
internal class DisableReplayRecordingPatches
{
	[HarmonyTranspiler, HarmonyPatch(typeof(GhostRecorder), "RecordCurrentDetails")]
	static IEnumerable<CodeInstruction> GhostRecorder_RecordCurrentDetails(IEnumerable<CodeInstruction> instructions)
	{
		var allNodesField = AccessTools.Field(typeof(GhostRecorder), "_allNodes");
		var listAddMethod = SymbolExtensions.GetMethodInfo((List<GhostNode> list) => list.Add(default));

		return new CodeMatcher(instructions)
		.MatchStartForward(
			new CodeMatch(OpCodes.Ldarg_0),
			new CodeMatch(OpCodes.Ldfld, allNodesField),
			new CodeMatch(OpCodes.Ldloc_1),
			new CodeMatch(OpCodes.Callvirt, listAddMethod)
		)
		.ThrowIfInvalid("_allNodes.Add() not found")
		.RemoveInstructions(4)
		.Instructions();
	}
}
