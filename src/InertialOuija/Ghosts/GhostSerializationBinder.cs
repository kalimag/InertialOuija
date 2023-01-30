extern alias GameScripts;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace InertialOuija.Ghosts;

public sealed class GhostSerializationBinder : SerializationBinder
{
	private static readonly HashSet<Type> AllowedTypes = new()
	{
		typeof(Nullable<>),
		typeof(DateTimeOffset),
		typeof(Guid),
		typeof(List<>),
		typeof(UnityEngine.Vector2),
		typeof(UnityEngine.Vector3),
		typeof(UnityEngine.Quaternion),
		typeof(GameScripts.Assets.Source.GhostCars.GhostLaps.GhostLap),
		typeof(GameScripts.Assets.Source.GhostCars.GhostLaps.GhostNode),
		typeof(GameScripts.Assets.Source.GhostCars.GhostLaps.GhostSector),
		typeof(GameScripts.Assets.Source.CloudStorage.CompressionFriendlyGhostRecording),
		typeof(GameScripts.Assets.Source.CloudStorage.ByteCompressionArray),
		typeof(GameScripts.Assets.Source.CloudStorage.ShortCompressionArray),
		typeof(ExternalGhostInfo),
	};
	private static readonly HashSet<string> AllowedAssemblies =
		new(AllowedTypes.Select(type => type.Assembly).Distinct().Select(asm => asm.GetName().Name));

	private static readonly ConcurrentDictionary<(string Assembly, string Type), Type> ReadTypeCache = new(1, 0);
#if DEBUG
	private static readonly ConcurrentDictionary<Type, bool> WriteWhitelistCache = new(1, 0);
#endif

	public override Type BindToType(string assemblyName, string typeName)
	{
		return ReadTypeCache.GetOrAdd((assemblyName, typeName), static (key) => FindType(key.Assembly, key.Type));
	}

#if DEBUG
	public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
	{
		WriteWhitelistCache.GetOrAdd(serializedType, static type =>
		{
			CheckWhitelist(type);
			return true;
		});
		assemblyName = null;
		typeName = null;
	}
#endif

	private static Type FindType(string assemblyName, string typeName)
	{
		//Log.Debug($"{nameof(FindType)}({assemblyName}, {typeName})", nameof(GhostSerializationBinder));

		CheckWhitelist(assemblyName, typeName);

		var assembly = Assembly.Load(assemblyName);
		var type = assembly.GetType(typeName, true);

		CheckWhitelist(type);

		return type;
	}

	private static void CheckWhitelist(string assemblyName, string typeName)
	{
		//Log.Debug($"{nameof(CheckWhitelist)}({assemblyName})", nameof(GhostSerializationBinder));

		if (AllowedAssemblies.Contains(assemblyName))
			return;

		var parsedAssemblyName = new AssemblyName(assemblyName);
		if (AllowedAssemblies.Contains(parsedAssemblyName.Name))
			return;

		throw new SerializationException($"Tried to serialize type \"{typeName}\" from non-whitelisted \"{assemblyName}\"");
	}

	private static void CheckWhitelist(Type type)
	{
		//Log.Debug($"{nameof(CheckWhitelist)}({type.FullName})", nameof(GhostSerializationBinder));

		if (type.IsPrimitive || type.IsEnum)
			return;

		if (type.IsConstructedGenericType)
		{
			foreach (var typeParameter in type.GetGenericArguments())
				CheckWhitelist(typeParameter);

			if (AllowedTypes.Contains(type.GetGenericTypeDefinition()))
				return;
		}

		if (!AllowedTypes.Contains(type))
			throw new SerializationException($"Tried to serialize non-whitelisted type \"{type.AssemblyQualifiedName}\"");
	}
}