extern alias GameScripts;
using System;
using System.Diagnostics;
using System.IO;
using InertialOuija.Ghosts;
using UnityEngine;
using static InertialOuija.Configuration.ModConfig;

namespace InertialOuija.UI
{
	internal class GhostOptionsWindow : Window
	{
		protected override string Title => "Ghost Options";
		protected override Rect InitialPosition => new(350, 100, 200, 50);
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

			if (GUILayout.Button("Export Ghost Database"))
				ExternalGhostManager.ExportPlayerDatabase();

			if (GUILayout.Button("Refresh External Ghosts"))
				ExternalGhostManager.ExportPlayerDatabase();

			if (GUILayout.Button("Open Ghost Folder"))
				OpenGhostFolder();

			GUILayout.Space(10);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Ghosts:");
			GUILayout.Label(ExternalGhostManager.Count.ToString());
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
