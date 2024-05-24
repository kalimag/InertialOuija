extern alias GameScripts;
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


		private static readonly GUIContent[] ModeLabels =
		{
			new("Default", "Unmodded behavior"),
			new("Best"),
			new("Rival", "Closest ghosts to personal best"),
			new("None"),
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
			GUILayout.Label("Ghosts:", Styles.Width100);
			Config.Ghosts.Mode = (ExternalGhostMode)GUILayout.Toolbar((int)Config.Ghosts.Mode, ModeLabels);
			GUILayout.EndHorizontal();


			GUI.enabled = Config.Ghosts.Mode is not ExternalGhostMode.Default and not ExternalGhostMode.None;

			GUILayout.BeginHorizontal();
			GUILayout.Label("Ghost count:", Styles.Width100);
			GUILayout.Label(_countString.GetString(Config.Ghosts.Count), Styles.GhostSelection.GhostCountLabelStyle, Styles.GhostSelection.GhostCountLabelLayout);
			Config.Ghosts.Count = Mathf.RoundToInt(GUILayout.HorizontalSlider(Config.Ghosts.Count, 1, Config.Ghosts.MaxCount,
				Styles.GhostSelection.GhostCountSlider, GUI.skin.horizontalSliderThumb));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Car:", Styles.Width100);
			Config.Ghosts.CarFilter = (CarFilter)GUILayout.Toolbar((int)Config.Ghosts.CarFilter, CarFilterLabels);
			GUILayout.EndHorizontal();

			GUI.enabled &= Config.Ghosts.CarFilter != CarFilter.SameCar && Config.Ghosts.Mode != ExternalGhostMode.NextBest;
			Config.Ghosts.UniqueCars = GUILayout.Toggle(Config.Ghosts.UniqueCars, "Unique cars");

			GUI.enabled = Config.Ghosts.Mode is not ExternalGhostMode.Default and not ExternalGhostMode.None and not ExternalGhostMode.NextBest;
			Config.Ghosts.MyGhosts = GUILayout.Toggle(Config.Ghosts.MyGhosts, "My ghosts only");


			GUI.enabled = true;

			GUILayout.Space(10);

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Options"))
				UIController.ToggleGhostOptionsWindow();

			GUILayout.Space(10);

			if (GUILayout.Button("Close"))
				Close();
			GUILayout.EndHorizontal();


			GUILayout.EndVertical();
		}

		void IReceiveMessages<GameModeSetupCompleteMessage>.HandleMessage(GameModeSetupCompleteMessage message)
		{
			Close();
		}
	}

	partial class Styles
	{
		public static class GhostSelection
		{
			public static GUILayoutOption[] GhostCountLabelLayout = new[] { GUILayout.Width(30) };
			public static readonly GUIStyle GhostCountLabelStyle = new(GUI.skin.label)
			{
				alignment = TextAnchor.MiddleCenter,
				wordWrap = false,
			};
			public static readonly GUIStyle GhostCountSlider = new(GUI.skin.horizontalSlider)
			{
				margin = new(0, 0, 10, 0) 
			};
		}
	}
}