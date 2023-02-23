extern alias GameScripts;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using GameScripts.Assets.Source.CloudStorage;
using GameScripts.Assets.Source.Enums;
using HarmonyLib;

namespace InertialOuija.Patches;

[HarmonyPatch]
internal class DownloadDlcGhostPatches
{
	// GhostDownloadSequence tries to obtain the "cloud packing id" for the car
	// There are no dictionary entries for DLC cars so it fails to download the ghost
	// However, this id isn't actually used by SteamCloudStorage.DownloadGhostPack,
	// so we can NOP it to fix downloads. Upload already works fine.

	static MethodBase TargetMethod() =>
		typeof(CloudManager)
		.GetMethod(nameof(CloudManager.GhostDownloadSequence))
		.GetIteratorImplementation();

	[HarmonyTranspiler]
	static IEnumerable<CodeInstruction> CloudManager_GhostDownloadSequence(IEnumerable<CodeInstruction> instructions)
	{
		var getCloudPackingIdMethod = ((Func<Car, int>)GameScripts.CloudPackMapping.GetCloudPackingId).Method;
		var getCloudPackingIdMethodStub = ((Func<Car, int>)StubGetCloudPackingIdMethod).Method;

		foreach (var instruction in instructions)
		{
			if (instruction.Calls(getCloudPackingIdMethod))				
				yield return new(instruction) { operand = getCloudPackingIdMethodStub }; // preserve label!
			else
				yield return instruction;
		}
	}

	static int StubGetCloudPackingIdMethod(Car car) => 0;
}
