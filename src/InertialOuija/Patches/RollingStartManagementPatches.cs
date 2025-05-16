extern alias GameScripts;

using GameScripts.Assets.Source.GhostCars.GhostDatabases;
using HarmonyLib;
using InertialOuija.RollingStarts;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using static InertialOuija.Configuration.ModConfig;

namespace InertialOuija.Patches;

[HarmonyPatch]
internal class RollingStartManagementPatches
{
	[HarmonyPrefix, HarmonyPatch(typeof(RollingStartDatabase), nameof(RollingStartDatabase.AddGhostRecording))]
	static bool RollingStartDatabase_AddGhostRecordingPrefix(RollingStartDatabase __instance, GhostKey ghostKey)
	{
		try
		{
			var pref = Config.Game.GetRollingStartPreference(ghostKey.Track, ghostKey.Direction, ghostKey.Car);
			Log.Debug($"Rolling start preference: {pref}");

			if (pref == RollingStartPreference.Locked)
				return PrefixPatch.SkipOriginal;

			__instance.OverwriteRecords = pref == RollingStartPreference.Newest;
			return PrefixPatch.RunOriginal;
		}
		catch (Exception ex)
		{
			Log.Error(ex);
			__instance.OverwriteRecords = false;
			return PrefixPatch.RunOriginal;
		}
	}


	[HarmonyTranspiler, HarmonyPatch(typeof(RollingStartDatabase), nameof(RollingStartDatabase.AddGhostRecording))]
	static IEnumerable<CodeInstruction> RollingStartDatabase_AddGhostRecording(IEnumerable<CodeInstruction> instructions)
	{
		var getItemMethod = SymbolExtensions.GetMethodInfo((Dictionary<GhostKey, RollingStartRecord> dict) => dict[default]);

		return new CodeMatcher(instructions)
			.MatchEndForward(
				new(OpCodes.Ldarg_1),
				new(OpCodes.Callvirt, getItemMethod),
				new(OpCodes.Stloc_2)
			)
			.ThrowIfInvalid("rollingStartRecord = this._fastestLaps[ghostKey] not found")
			.InsertAfterAndAdvance(
				new CodeInstruction(OpCodes.Ldloc_2), // RollingStartRecord local
				new CodeInstruction(OpCodes.Call, ((Delegate)BackupRecord).Method)
			)
			.Instructions();

		static void BackupRecord(RollingStartRecord record) => RollingStartManager.UndoRecord = record.Clone();
	}
}
