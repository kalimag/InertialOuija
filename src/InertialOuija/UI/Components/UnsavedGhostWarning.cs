extern alias GameScripts;
using GameScripts.Assets.Source.Tools;
using UnityEngine;

namespace InertialOuija.UI.Components
{
	internal class UnsavedGhostWarning : MonoBehaviour
	{
		public bool RaceComplete { get; set; }

		void OnGUI()
		{
			if (!RaceComplete && !CorePlugin.IsPaused)
				return;

			Styles.ScaleRelative(.5f, 1f);

			string message = RaceComplete ? "Wait until car stops to save last lap" : "Drive further past the finish line to save last lap";

			GUILayout.BeginArea(new Rect(Screen.width / 2 - 200, Screen.height - 100, 400, 80));
			GUILayout.BeginVertical(GUI.skin.box);
			GUILayout.Label("Unsaved ghost", Styles.UnsavedGhostWarning.CaptionStyle);
			GUILayout.Label(message, Styles.UnsavedGhostWarning.MessageStyle);
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}
}

namespace InertialOuija.UI
{
	partial class Styles
	{
		public static class UnsavedGhostWarning
		{
			public static readonly GUIStyle CaptionStyle = new(GUI.skin.label)
			{
				fontSize = 15,
				normal =
				{
					textColor = new Color(0.83f, 0.13f, 0.38f),
				},
				fontStyle = FontStyle.Bold,
				alignment = TextAnchor.MiddleCenter,
				wordWrap = false,
			};
			public static readonly GUIStyle MessageStyle = new(GUI.skin.label)
			{
				alignment = TextAnchor.MiddleCenter,
				wordWrap = false,
			};
		}
	}
}
