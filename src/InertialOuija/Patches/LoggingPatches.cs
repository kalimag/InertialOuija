using System.Diagnostics;
using HarmonyLib;

namespace InertialOuija.Patches;

[HarmonyPatch]
internal partial class LoggingPatches
{
	[Conditional("DEBUG")]
	public static void Apply(Harmony harmony)
	{
		harmony.Apply<LoggingPatches>();
		foreach (var nestedType in typeof(LoggingPatches).GetNestedTypes())
			harmony.Apply(nestedType);
	}
}
