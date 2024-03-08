extern alias GameScripts;
using System;
using System.Diagnostics;
using System.IO;
using InertialOuija.Ghosts;
using InertialOuija.Utilities;
using UnityEngine;
using static InertialOuija.Configuration.ModConfig;

namespace InertialOuija.UI
{
	internal class GhostOptionsWindow : Window
	{
		private readonly StringCache<int> _countString = new();


		protected override string Title => "Ghost Options";
		protected override Rect InitialPosition => new(510, 100, 200, 50);
		protected override Rect WindowPosition { get => _windowPosition; set => _windowPosition = value; }

		private static Rect _windowPosition;


		protected override void OnDestroy()
		{
			base.OnDestroy();
			Config.Save();
		}

		protected override void DrawWindow()
		{
			GUILayout.BeginVertical();

			Config.Ghosts.GhostVisual = !GUILayout.Toggle(!Config.Ghosts.GhostVisual, "Show as regular cars");

			Config.Ghosts.DisableHeadlights = GUILayout.Toggle(Config.Ghosts.DisableHeadlights, "Disable headlight effect");

			GUILayout.Space(10);

			Config.UI.ShowGhostTime = GUILayout.Toggle(Config.UI.ShowGhostTime, "Show ghost time in hud");
			Config.UI.ShowPersonalBestTime = GUILayout.Toggle(Config.UI.ShowPersonalBestTime, TempContent("Show personal best in hud", "Based on saved ghosts"));
			Config.UI.HideAchievedTargetTimes = GUILayout.Toggle(Config.UI.HideAchievedTargetTimes, "Hide medals time in hud");

			GUILayout.Space(10);

			if (GUILayout.Button("Open Ghost Folder"))
				OpenGhostFolder();

			if (GUILayout.Button(TempContent("Refresh External Ghosts","Use if you added or removed files while the game is running")))
				ExternalGhostManager.RefreshDatabase();

			GUILayout.Space(10);

			if (GUILayout.Button(TempContent("Export Old Ghosts", "This only needs to be done once")))
				ExternalGhostManager.ExportPlayerDatabase();

			GUILayout.Space(10);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Ghosts:", Styles.NoWrapLabel, Styles.DontExpandWidth);
			GUILayout.Label(_countString.GetString(ExternalGhostManager.Count), Styles.NoWrapLabel, Styles.DontExpandWidth);
			GUILayout.EndHorizontal();

			GUILayout.Space(10);

			if (GUILayout.Button("Close"))
				Close();

			GUILayout.EndVertical();
		}

		private void OpenGhostFolder()
		{
			if (!Directory.Exists(ExternalGhostManager.GhostsPath))
				Directory.CreateDirectory(ExternalGhostManager.GhostsPath);

			Process.Start(ExternalGhostManager.GhostsPath);
		}
	}
}
