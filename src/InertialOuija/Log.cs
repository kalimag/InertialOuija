using System;
using ConditionalAttribute = System.Diagnostics.ConditionalAttribute;
using System.Runtime.CompilerServices;

namespace InertialOuija;

internal static class Log
{
	public static void Info(string message)
	{
		UnityEngine.Debug.Log($"[InertialOuija] {message}");
	}
	public static void Info(string message, string source)
	{
		UnityEngine.Debug.Log($"[InertialOuija {source}] {message}");
	}

	public static void Warning(string message)
	{
		UnityEngine.Debug.LogWarning($"[InertialOuija] {message}");
	}

	public static void Warning(string message, string source)
	{
		UnityEngine.Debug.LogWarning($"[InertialOuija {source}] {message}");
	}

	public static void Error(string message, Exception ex)
	{
		UnityEngine.Debug.LogError($"[InertialOuija] {message}\n\n{ex}");
	}

	public static void Error(Exception ex)
	{
		UnityEngine.Debug.LogError($"[InertialOuija] {ex}");
	}

	[Conditional("DEBUG")]
	public static void Debug(string message, [CallerMemberName] string source = null)
	{
		UnityEngine.Debug.Log($"[InertialOuija {source}] {message}");
	}
}
