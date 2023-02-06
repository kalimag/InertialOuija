extern alias GameScripts;
using System;
using System.Diagnostics.Tracing;
using GameScripts.Assets.Source.Messaging;
using GameScripts.Assets.Source.Messaging.Messages;
using InertialOuija.Ghosts;
using UnityEngine;
using static InertialOuija.Configuration.ModConfig;

namespace InertialOuija.UI
{
	internal class GhostSelectionWindow : Window, IReceiveMessages<GameModeSetupCompleteMessage>
	{
		protected override string Title => "Ghosts";
		protected override Rect InitialPosition => new(100, 100, 200, 50);
		protected override Rect WindowPosition { get => _windowPosition; set => _windowPosition = value; }

		private static Rect _windowPosition;


		private static readonly string[] ModeLabels =
		{
			"None",
			"Fastest"
		};


		protected override void Awake()
		{
			base.Awake();
			MessagingCenter.RegisterReceiver(this);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			MessagingCenter.DeregisterReceiver(this);
			Config.Save();
		}

		protected override void DrawWindow()
		{
			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal();
			GUILayout.Label("External Ghosts:", Styles.Width100);
			Config.Ghosts.Mode = (ExternalGhostMode)GUILayout.Toolbar((int)Config.Ghosts.Mode, ModeLabels);
			GUILayout.EndHorizontal();

			GUI.enabled = Config.Ghosts.Mode != ExternalGhostMode.None;

			GUILayout.BeginHorizontal();
			GUILayout.Label("Ghost count:", Styles.Width100);
			GUILayout.Label(Config.Ghosts.Count.ToString());
			GUILayout.EndHorizontal();
			Config.Ghosts.Count = Mathf.RoundToInt(GUILayout.HorizontalSlider(Config.Ghosts.Count, 1, Config.Ghosts.MaxCount));

			Config.Ghosts.MyGhosts = GUILayout.Toggle(Config.Ghosts.MyGhosts, "My ghosts only");

			Config.Ghosts.SameCar = GUILayout.Toggle(Config.Ghosts.SameCar, "Same car only");

			GUI.enabled &= !Config.Ghosts.SameCar;
			Config.Ghosts.UniqueCars = GUILayout.Toggle(Config.Ghosts.UniqueCars, "Unique cars");

			GUI.enabled = true;

			GUILayout.Space(10);

			if (GUILayout.Button("Options"))
				UIController.ToggleGhostOptionsWindow();

			GUILayout.Space(10);

			if (GUILayout.Button("Close"))
				Close();

			GUILayout.EndVertical();
		}

		private void OpenGhostFolder()
		{
			throw new NotImplementedException();
		}

		void IReceiveMessages<GameModeSetupCompleteMessage>.HandleMessage(GameModeSetupCompleteMessage message)
		{
			Close();
		}
	}
}
