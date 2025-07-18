﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace InertialOuija.Patches;

internal static class PatchExtensions
{
	public static void Apply(this Harmony harmony, Type patchClass, bool condition = true)
	{
		if (condition)
		{
			List<MethodInfo> patchedMethods;
			try
			{
				patchedMethods = harmony.CreateClassProcessor(patchClass).Patch();
			}
			catch (Exception ex)
			{
				Log.Error($"Failed to apply {patchClass.Name}", ex);
#if DEBUG
				return;
#else
				try
				{
					MainController.TrySendMainThread(() => Process.Start(Path.GetFullPath(UnityEngine.Application.consoleLogPath)));
				}
				finally
				{
					Environment.Exit(0xBAD);
				}
#endif
			}

#if DEBUG
			// Calling ToString() on one specific MethodInfo suddenly hard crashes Mono, even on older revisions?
			// No clue why, so let's just not do that and hope for the best I guess
			if (patchedMethods != null)
				Log.Info($"Applied {patchClass.Name}:\n" + String.Join("\n", patchedMethods.Select(m => m.Name)));
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

	public static MethodInfo GetIteratorImplementation(this MethodInfo method)
	{
		if (method == null)
			throw new ArgumentNullException(nameof(method));

		var iteratorAttr = method.GetCustomAttribute<IteratorStateMachineAttribute>();
		if (iteratorAttr == null)
			throw new InvalidOperationException($"{method} is not a yield iterator method.");

		return iteratorAttr.StateMachineType.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
	}
}
