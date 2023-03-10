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

namespace InertialOuija.Patches;

[HarmonyPatch]
internal class SerializationWhitelistPatches
{
	static MethodBase TargetMethod()
	{
		var iteratorAttr = typeof(SteamCloudStorage)
			.GetMethod(nameof(SteamCloudStorage.DownloadGhostPack))
			.GetCustomAttribute<IteratorStateMachineAttribute>();

		return iteratorAttr.StateMachineType.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
	}

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

		var serializer = new BinaryFormatter();
		serializer.Binder = new GhostSerializationBinder();
		SaveHelpers.AddUnitySerialisationSurrogates(serializer);

		using var mem = new MemoryStream(buffer);
		try
		{
			return serializer.Deserialize(mem);
		}
		catch (Exception ex)
		{
			Log.Error(ex);
			return null;
		}
	}
}