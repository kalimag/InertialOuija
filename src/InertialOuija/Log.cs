using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace InertialOuija;

internal class Log
{
	public static void Info(string message)
	{
		UnityEngine.Debug.Log($"[InertialOuija] {message}");
	}

	public static void Error(string message, Exception ex)
	{
		UnityEngine.Debug.Log($"[InertialOuija] {message}\n\n{ex}");
	}

	public static void Error(Exception ex)
	{
		UnityEngine.Debug.Log($"[InertialOuija] {ex}");
	}

	[Conditional("DEBUG")]
	public static void Debug(string message, [CallerMemberName] string source = null)
	{
		UnityEngine.Debug.Log($"[InertialOuija {source}] {message}");
	}
}
