extern alias GameScripts;
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
		private readonly StringCache<int> _countString = new("Ghosts: 0");


		protected override string Title => "Ghost Options";
		protected override Rect InitialPosition => new(710, 100, 200, 50);
		protected override Rect WindowPosition { get => _windowPosition; set => _windowPosition = value; }

		private static Rect _windowPosition;


		protected override void OnDestroy()
		{
			base.OnDestroy();
			Config.Save();
		}

		protected override void DrawWindow()
		{
			Config.Ghosts.GhostVisual = !GUILayout.Toggle(!Config.Ghosts.GhostVisual, "Show as regular cars");

			Config.Ghosts.DisableHeadlights = GUILayout.Toggle(Config.Ghosts.DisableHeadlights, "Disable headlight effect");

			Styles.Space();

			Config.UI.ShowGhostTime = GUILayout.Toggle(Config.UI.ShowGhostTime, "Show ghost time in hud");
			Config.UI.ShowPersonalBestTime = GUILayout.Toggle(Config.UI.ShowPersonalBestTime, TempContent("Show personal best in hud", "Based on saved ghosts"));
			Config.UI.HideAchievedTargetTimes = GUILayout.Toggle(Config.UI.HideAchievedTargetTimes, "Hide medals time in hud");
			Config.UI.ShowChosenGhosts = GUILayout.Toggle(Config.UI.ShowChosenGhosts, "Show ghost list before race");

			Styles.Space();

			if (GUILayout.Button("Open Ghost Folder"))
				OpenGhostFolder();

			if (GUILayout.Button(TempContent("Refresh External Ghosts", "Use if you added or removed files while the game is running")))
				ExternalGhostManager.RefreshDatabaseAsync().LogFailure();

			if (GUILayout.Button(TempContent("Reset Ghost Cache", "This shouldn't be necessary unless something goes wrong")))
				ExternalGhostManager.ResetCache().LogFailure();

			Styles.Space();

			if (GUILayout.Button(TempContent("Export Old Ghosts", "This only needs to be done once")))
				ExternalGhostManager.ExportPlayerDatabase();

			Styles.Space();

			GUILayout.Label(_countString.GetString(ExternalGhostManager.Count));

			Styles.Space();

			if (GUILayout.Button("Close"))
				Close();
		}

		private void OpenGhostFolder()
		{
			if (!Directory.Exists(ExternalGhostManager.GhostsPath))
				Directory.CreateDirectory(ExternalGhostManager.GhostsPath);

			Process.Start(ExternalGhostManager.GhostsPath);
		}
	}
}
