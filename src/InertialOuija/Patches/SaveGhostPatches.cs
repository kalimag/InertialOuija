extern alias GameScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using GameScripts.Assets.Source.CloudStorage;
using GameScripts.Assets.Source.GhostCars.GhostDatabases;
using GameScripts.Assets.Source.GhostCars.GhostLaps;
using HarmonyLib;
using InertialOuija.Ghosts;

namespace InertialOuija.Patches;

[HarmonyPatch]
internal class SaveGhostPatches
{

	[HarmonyPrefix, HarmonyPatch(typeof(PlayerGhostDatabase), nameof(PlayerGhostDatabase.AddGhostRecording))]
	static void PlayerGhostDatabase_AddGhostRecording(GhostKey ghostKey, GhostLap lap, Dictionary<GhostKey, GhostRecord> ____fastestLaps)
	{
		try
		{
			bool isValid = lap.Sectors != null && lap.Sectors.Any() && lap.LapTime >= 10f && lap.Sectors[0].HasNodes(5);
			bool isFastest = ____fastestLaps?.TryGetValue(ghostKey, out var fastestLap) != true || lap.LapTime <= fastestLap.Time;
			if (isValid)
				ExternalGhostManager.AddPlayerGhost(ghostKey, lap, isFastest);
			else
				Log.Info($"AddGhostRecording called for invalid ghost ({ghostKey}, AnySectors={lap.Sectors?.Any()}, LapTime{lap.LapTime}, Sector0Nodes={lap.Sectors?.FirstOrDefault()?.MainZone?.Count})");
		}
		catch (Exception ex)
		{
			Log.Error("Failed to save player ghost", ex);
		}
	}

	[HarmonyFinalizer, HarmonyPatch(typeof(PlayerGhostDatabase), nameof(PlayerGhostDatabase.AddGhostRecording))]
	static void PlayerGhostDatabase_AddGhostRecordingFinalizer(GhostKey ghostKey, Exception __exception)
	{
		if (__exception != null)
			Log.Error($"Error while saving player ghost {ghostKey}", __exception);
	}
}

[HarmonyPatch]
internal class SaveDownloadedGhostPatches
{
	// This patch is a bit of a mess because we need three things from cloud ghosts:
	// - the GhostDownloadRequest to id the ghost
	// - the unpacked GhostLap
	// - the CompressionFriendlyGhostRecording because the unpacked ghost is missing the lap time
	// This is the only place to get all three in a single method

	static MethodBase TargetMethod() =>
		typeof(CloudManager)
		.GetMethod(nameof(CloudManager.GhostDownloadSequence))
		.GetIteratorImplementation();

	[HarmonyTranspiler]
	static IEnumerable<CodeInstruction> CloudManager_GhostDownloadSequence(IEnumerable<CodeInstruction> instructions, ILGenerator il, MethodBase method)
	{
		var saveGhostMethod = ((Action<GhostDownloadRequest, CompressionFriendlyGhostRecording>)SaveGhostDownloadSequenceGhost).Method;
		var ghostCacheGetGhostMethod = typeof(IGhostCache).GetMethod(nameof(IGhostCache.GetGhost));
		var closureRequestField = method.DeclaringType
			.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			.First(field => field.FieldType == typeof(GhostDownloadRequest));
		var requestDataField = typeof(GhostDownloadRequest).GetField(nameof(GhostDownloadRequest.Data));

		var packedGhostLocal = il.DeclareLocal(typeof(CompressionFriendlyGhostRecording));

		foreach (var instruction in instructions)
		{
			yield return instruction;

			if (instruction.Calls(ghostCacheGetGhostMethod))
			{
				// store CompressionFriendlyGhostRecording in predictable local
				yield return new(OpCodes.Dup);
				yield return new(OpCodes.Stloc, packedGhostLocal);
			}
			else if (instruction.StoresField(requestDataField))
			{
				yield return new(OpCodes.Ldarg_0); // load iterator closure instance
				yield return new(OpCodes.Ldfld, closureRequestField); // load request parameter
				yield return new(OpCodes.Ldloc, packedGhostLocal);
				yield return new(OpCodes.Call, saveGhostMethod);
			}
		}
	}

	private static void SaveGhostDownloadSequenceGhost(GhostDownloadRequest request, CompressionFriendlyGhostRecording packedGhost)
	{
		try
		{
			ExternalGhostManager.AddLeaderboardGhost(request, packedGhost);
		}
		catch (Exception ex)
		{
			Log.Error("Failed to save leaderboard ghost", ex);
		}
	}
}