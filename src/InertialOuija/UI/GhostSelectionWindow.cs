extern alias GameScripts;
using System;
using System.Diagnostics.Tracing;
using GameScripts.Assets.Source.Messaging;
using GameScripts.Assets.Source.Messaging.Messages;
using InertialOuija.Ghosts;
using InertialOuija.Utilities;
using UnityEngine;
using static InertialOuija.Configuration.ModConfig;

namespace InertialOuija.UI
{
	internal class GhostSelectionWindow : Window, IReceiveMessages<GameModeSetupCompleteMessage>
	{
		private readonly StringCache<int> _countString = new();


		protected override string Title => "Ghosts";
		protected override Rect InitialPosition => new(100, 100, 400, 50);
		protected override Rect WindowPosition { get => _windowPosition; set => _windowPosition = value; }

		private static Rect _windowPosition;


		private static readonly string[] ModeLabels =
		{
			"None",
			"Fastest"
		};
		
		private static readonly string[] CarFilterLabels =
		{
			"Same Car",
			"Same Class",
			"Any"
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
			GUILayout.Label(_countString.GetString(Config.Ghosts.Count));
			Config.Ghosts.Count = Mathf.RoundToInt(GUILayout.HorizontalSlider(Config.Ghosts.Count, 1, Config.Ghosts.MaxCount));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Car:", Styles.Width100);
			Config.Ghosts.CarFilter = (CarFilter)GUILayout.Toolbar((int)Config.Ghosts.CarFilter, CarFilterLabels);
			GUILayout.EndHorizontal();

			GUI.enabled &= Config.Ghosts.CarFilter != CarFilter.SameCar;
			Config.Ghosts.UniqueCars = GUILayout.Toggle(Config.Ghosts.UniqueCars, "Unique cars");
			
			GUI.enabled = Config.Ghosts.Mode != ExternalGhostMode.None;
			Config.Ghosts.MyGhosts = GUILayout.Toggle(Config.Ghosts.MyGhosts, "My ghosts only");


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
