extern alias GameScripts;
using System;
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

			GUILayout.Space(10);

			if (GUILayout.Button("Export Ghost Database"))
				ExternalGhostManager.ExportPlayerDatabase();

			if (GUILayout.Button("Refresh External Ghosts"))
				ExternalGhostManager.ExportPlayerDatabase();

			if (GUILayout.Button("Open Ghost Folder"))
				OpenGhostFolder();

			GUILayout.Space(10);

			if (GUILayout.Button("Close"))
				Close();

			GUILayout.EndVertical();
		}

		private void OpenGhostFolder()
		{
			throw new NotImplementedException();
		}
	}
}
