using System;
using HarmonyLib;

namespace InertialOuija.Patches;

internal static class PatchExtensions
{
	public static void Apply(this Harmony harmony, Type patchClass, bool condition = true)
	{
		if (condition)
		{
			var patchedMethods = harmony.CreateClassProcessor(patchClass).Patch();

#if DEBUG
			if (patchedMethods != null)
				Log.Info($"Applied {patchClass.Name}:\n" + String.Join("\n", patchedMethods));
			else
				Log.Info($"No patches found in {patchClass.Name}");
#else
			Log.Info($"Applied {patchClass.Name}");
#endif
		}
		else
		{
			Log.Info($"{patchClass.Name} not applied");
		}
	}

	public static void Apply<T>(this Harmony harmony, bool condition = true)
		=> harmony.Apply(typeof(T), condition);
}
