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


		protected override string Title => "GHOSTS";
		protected override Rect InitialPosition => new(100, 100, 600, 50);
		protected override Rect WindowPosition { get => _windowPosition; set => _windowPosition = value; }

		private static Rect _windowPosition;


		private static readonly GUIContent[] ModeLabels =
		{
			new("Default", "Unmodded behavior"),
			new("Best"),
			new("Rival", "Closest ghosts to personal best"),
			new("None"),
		};

		private static readonly GUIContent[] CarFilterLabels =
		{
			new("Same Car"),
			new("Same Class"),
			new("Any"),
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
			using (Styles.Row("Ghosts:"))
				Config.Ghosts.Mode = (ExternalGhostMode)GUILayout.Toolbar((int)Config.Ghosts.Mode, ModeLabels, Styles.Toolbar);

			using (Styles.Enable(Config.Ghosts.Mode is not (ExternalGhostMode.Default or ExternalGhostMode.None)))
			{
				using (Styles.Row("Ghost count:"))
				{
					GUILayout.Label(_countString.GetString(Config.Ghosts.Count), Styles.CenteredLabel, GhostCountLabelLayout);
					Config.Ghosts.Count = Mathf.RoundToInt(GUILayout.HorizontalSlider(Config.Ghosts.Count, 1, Config.Ghosts.MaxCount));
				}

				using (Styles.Row("Car:"))
					Config.Ghosts.CarFilter = (CarFilter)GUILayout.Toolbar((int)Config.Ghosts.CarFilter, CarFilterLabels, Styles.Toolbar);

				using (Styles.Enable(Config.Ghosts.Mode != ExternalGhostMode.NextBest))
				{
					using (Styles.Enable(Config.Ghosts.CarFilter != CarFilter.SameCar))
					using (Styles.Row())
						Config.Ghosts.UniqueCars = GUILayout.Toggle(Config.Ghosts.UniqueCars, "Unique cars");

					using (Styles.Enable(!Config.Ghosts.MyGhosts))
					using (Styles.Row())
						Config.Ghosts.UniqueUsers = GUILayout.Toggle(Config.Ghosts.UniqueUsers, "Unique players");

					using (Styles.Row())
						Config.Ghosts.MyGhosts = GUILayout.Toggle(Config.Ghosts.MyGhosts, "My ghosts only");
				}
			}

			Styles.Space();

			using (Styles.Horizontal())
			{
				if (GUILayout.Button("Options", Styles.FixedButton))
					UIController.ToggleGhostOptionsWindow();

				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Close", Styles.FixedButton))
					Close();
			}
		}

		void IReceiveMessages<GameModeSetupCompleteMessage>.HandleMessage(GameModeSetupCompleteMessage message)
		{
			Close();
		}

		private static GUILayoutOption[] GhostCountLabelLayout = [GUILayout.Width(40)];
	}
}