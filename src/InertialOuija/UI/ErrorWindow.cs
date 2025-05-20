extern alias GameScripts;

using GameScripts.Assets.Source.Tools;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using static InertialOuija.Configuration.ModConfig;

namespace InertialOuija.UI;

internal sealed class ErrorWindow : Window
{
	private static bool _hideErrors;

	private string _title;
	private string _message;

	private Vector2 _scrollPosition;

	protected override Rect WindowPosition { get; set; }
	protected override Rect InitialPosition => new(100, 100, 700, 300);
	protected override string Title => _title;



	protected override void OnGUI()
	{
		// Hide during active race, except when paused/in menu
		if (CorePlugin.CorePluginInstance() && (!CorePlugin.IsPaused && CorePlugin.EventManager?.EventInProgress == true && CorePlugin.MenuSelectionManager?.ActiveMenu() != true))
			return;

		base.OnGUI();
	}

	protected override void DrawWindow()
	{
		_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
		GUILayout.Label(_message, Styles.MultilineWrapLabel);
		GUILayout.EndScrollView();

		GUILayout.BeginHorizontal();

		if (GUILayout.Button("Copy", Styles.FixedButton))
			CopyErrorMessage();

		if (GUILayout.Button("Open Log", Styles.FixedButton))
			OpenLog();

		GUILayout.FlexibleSpace();

		_hideErrors = GUILayout.Toggle(_hideErrors, "Hide further errors");

		GUILayout.FlexibleSpace();

		if (GUILayout.Button("Close", Styles.FixedButton))
			Close();

		GUILayout.EndHorizontal();
	}

	private void CopyErrorMessage()
	{
		var sb = new StringBuilder(_message);

		sb.AppendLine();
		sb.AppendLine();

		var modVersion = typeof(Doorstop.Entrypoint).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
		var modBuild = typeof(Doorstop.Entrypoint).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration;
		sb.AppendLine($"Mod: {modVersion} {modBuild}");
		sb.AppendLine($"Game: {Application.version} {Application.buildGUID}");
		sb.AppendLine($"OS: {SystemInfo.operatingSystem} [{Environment.OSVersion}]");

		GUIUtility.systemCopyBuffer = sb.ToString();
	}

	private static void OpenLog()
	{
		var logPath = Path.GetFullPath(Application.consoleLogPath); // explorer doesn't like Unity's use of forward slashes
		if (!File.Exists(logPath))
			return;

		if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			Process.Start("explorer", $"/select,\"{logPath}\"")?.Dispose();
		else
			Process.Start(logPath)?.Dispose();
	}



	public static void ShowError(string message, string title = "ERROR")
	{
		if (!_hideErrors && Config.UI.ShowErrors)
			MainController.TryPostMainThread(CreateErrorWindow);

		void CreateErrorWindow()
		{
			var obj = new GameObject { hideFlags = HideFlags.HideAndDontSave };
			var window = obj.AddComponent<ErrorWindow>();
			window._message = message;
			window._title = title;
		}
	}
}
