extern alias GameScripts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using GameScripts.Assets.Source.CloudStorage;
using GameScripts.Assets.Source.SaveData;
using HarmonyLib;
using InertialOuija.Ghosts;
using InertialOuija.Utilities;

namespace InertialOuija.Patches;

[HarmonyPatch]
internal class SerializationWhitelistPatches
{
	static MethodBase TargetMethod() => 
		typeof(SteamCloudStorage)
		.GetMethod(nameof(SteamCloudStorage.DownloadGhostPack))
		.GetIteratorImplementation();

	[HarmonyTranspiler]
	static IEnumerable<CodeInstruction> SteamCloudStorage_DownloadGhostPack(IEnumerable<CodeInstruction> instructions)
	{
		var origDeserializeMethod = ((Func<byte[], object>)SaveHelpers.Deserialise).Method;
		var newDeserializeMethod = ((Func<byte[], object>)DeserializeWhitelisted).Method;

		foreach (var instruction in instructions)
		{
			if (instruction.Calls(origDeserializeMethod))
				yield return new CodeInstruction(OpCodes.Call, newDeserializeMethod);
			else
				yield return instruction;
		}
	}

	static object DeserializeWhitelisted(byte[] buffer)
	{
		Log.Debug(nameof(DeserializeWhitelisted), nameof(SerializationWhitelistPatches));

		using var mem = new MemoryStream(buffer);
		using var serializer = ObjectPool.WhitelistedSerializers.Lease();
		try
		{
			return serializer.Value.Deserialize(mem);
		}
		catch (Exception ex)
		{
			Log.Error(ex);
			return null;
		}
	}
}