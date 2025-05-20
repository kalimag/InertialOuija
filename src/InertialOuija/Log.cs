using System;
using ConditionalAttribute = System.Diagnostics.ConditionalAttribute;
using System.Runtime.CompilerServices;
using UnityEngine;

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
		UI.ErrorWindow.ShowError(message, "WARNING");
	}

	public static void Warning(string message, string source)
	{
		UnityEngine.Debug.LogWarning($"[InertialOuija {source}] {message}");
		UI.ErrorWindow.ShowError($"{source}: {message}", "WARNING");
	}

	public static void Error(string message, Exception ex, bool hidden = false)
	{
		UnityEngine.Debug.LogError($"[InertialOuija] {message}\n\n{ex}");
		if (!hidden)
			UI.ErrorWindow.ShowError($"{message}\n\n{ex}");
	}

	public static void Error(Exception ex, bool hidden = false)
	{
		UnityEngine.Debug.LogError($"[InertialOuija] {ex}");
		if (!hidden)
			UI.ErrorWindow.ShowError(ex.ToString());
	}

	[Conditional("DEBUG")]
	public static void Debug(string message, [CallerMemberName] string source = null)
	{
		UnityEngine.Debug.Log($"[InertialOuija {source}] {message}");
	}

#if DEBUG
	static Log()
	{
		Application.logMessageReceivedThreaded += (message, stackTrace, type) =>
		{
			if (type == LogType.Exception)
				UI.ErrorWindow.ShowError($"{message}\n\n{stackTrace}");
		};
	}
#endif
}
