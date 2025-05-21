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


		protected override string Title => "OPTIONS";
		protected override Rect InitialPosition => new(710, 100, 400, 50);
		protected override Rect WindowPosition { get => _windowPosition; set => _windowPosition = value; }

		private static Rect _windowPosition;


		protected override void OnDestroy()
		{
			base.OnDestroy();
			Config.Save();
		}

		protected override void DrawWindow()
		{

			GUILayout.Label("Ghosts", Styles.Heading);
			Config.Ghosts.GhostVisual = !GUILayout.Toggle(!Config.Ghosts.GhostVisual, "Show as regular cars");
			Config.Ghosts.DisableHeadlights = GUILayout.Toggle(Config.Ghosts.DisableHeadlights, "Disable headlight effect");

			GUILayout.Label("UI", Styles.Heading);
			Config.UI.ShowGhostTime = GUILayout.Toggle(Config.UI.ShowGhostTime, "Show ghost time in HUD");
			Config.UI.ShowPersonalBestTime = GUILayout.Toggle(Config.UI.ShowPersonalBestTime, TempContent("Show personal best in HUD", "Based on saved ghosts"));
			Config.UI.HideAchievedTargetTimes = GUILayout.Toggle(Config.UI.HideAchievedTargetTimes, TempContent("Hide medal target times in HUD", "Hides the gold/silver/bronze display if your PB is better"));
			Config.UI.ShowChosenGhosts = GUILayout.Toggle(Config.UI.ShowChosenGhosts, "Show ghost list before race");

			GUILayout.Label("Ghost Files", Styles.Heading);
			GUILayout.Label(_countString.GetString(ExternalGhostManager.Count));

			using (Styles.Horizontal())
			{
				using (Styles.Enable(!ExternalGhostManager.RefreshInProgress))
					if (GUILayout.Button(TempContent("Refresh Ghosts", "Use if you added or removed files while the game is running"), CustomStyles.GhostButtonLayout))
						ExternalGhostManager.RefreshDatabaseAsync().LogFailure();

				if (GUILayout.Button("Open Ghost Folder", CustomStyles.GhostButtonLayout))
					OpenGhostFolder();
			}

			Styles.Space();

			using (Styles.Enable(!ExternalGhostManager.RefreshInProgress))
			using (Styles.Horizontal())
			{
				using (Styles.Enable(GameData.CorePluginInitialized))
					if (GUILayout.Button(TempContent("Export Old Ghosts", "This only needs to be done once"), CustomStyles.GhostButtonLayout))
						ExternalGhostManager.ExportPlayerDatabase();

				if (GUILayout.Button(TempContent("Reset Ghost Cache", "This shouldn't be necessary unless something goes wrong"), CustomStyles.GhostButtonLayout))
					ExternalGhostManager.ResetCache().LogFailure();
			}

			Styles.Space();

			using (Styles.Horizontal())
			{
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Close", Styles.FixedButton))
					Close();
			}
		}

		private void OpenGhostFolder()
		{
			if (!Directory.Exists(ExternalGhostManager.GhostsPath))
				Directory.CreateDirectory(ExternalGhostManager.GhostsPath);

			Process.Start(ExternalGhostManager.GhostsPath);
		}

		private static class CustomStyles
		{
			public static GUILayoutOption[] GhostButtonLayout = [GUILayout.Width(200)];
		}
	}
}
