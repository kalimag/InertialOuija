extern alias GameScripts;
using GameScripts.Assets.Source.Messaging;
using GameScripts.Assets.Source.Messaging.Messages;
using InertialOuija.Ghosts;
using UnityEngine;

namespace InertialOuija.UI
{
	internal class GhostConfigWindow : Window, IReceiveMessages<GameModeSetupCompleteMessage>
	{
		protected override string Title => "Ghosts";
		protected override Rect InitialPosition => new(100, 100, 200, 50);
		protected override Rect WindowPosition { get => _windowPosition; set => _windowPosition = value; }

		private static Rect _windowPosition;


		protected override void Awake()
		{
			base.Awake();
			MessagingCenter.RegisterReceiver(this);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			MessagingCenter.DeregisterReceiver(this);
		}

		protected override void DrawWindow()
		{
			GUILayout.BeginVertical();

			GhostController.UseVanillaGhosts = GUILayout.Toggle(GhostController.UseVanillaGhosts, "Use regular ghosts");
			GhostController.UseExternalGhosts = GUILayout.Toggle(GhostController.UseExternalGhosts, "Use external ghosts");
			GhostController.SameCarOnly = GUILayout.Toggle(GhostController.SameCarOnly, "Same car only");

			GUILayout.BeginHorizontal();
			GUILayout.Label("Ghost count:");
			GUILayout.Label(GhostController.GhostCount.ToString());
			GUILayout.EndHorizontal();
			GhostController.GhostCount = Mathf.RoundToInt(GUILayout.HorizontalSlider(GhostController.GhostCount, 1, 10));

			GUILayout.Space(10);

			if (GUILayout.Button("Export Ghost Database"))
				ExternalGhostManager.ExportPlayerDatabase();

			if (GUILayout.Button("Refresh External Ghosts"))
				ExternalGhostManager.ExportPlayerDatabase();

			GUILayout.Space(10);

			if (GUILayout.Button("Close"))
				Close();

			GUILayout.EndVertical();
		}

		void IReceiveMessages<GameModeSetupCompleteMessage>.HandleMessage(GameModeSetupCompleteMessage message)
		{
			Close();
		}
	}
}
