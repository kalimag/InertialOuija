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

			Styles.ApplySkin();
			Styles.ScaleRelative(.5f, 1f);

			string message = RaceComplete ? "Wait until car stops to save last lap" : "Drive further past the finish line to save last lap";

			var rect = Styles.GetScreenRect(TextAnchor.LowerCenter, new(480, 64), new(0, 100));
			GUILayout.BeginArea(rect, "UNSAVED GHOST", Styles.Warning);
			GUILayout.Label(message, Styles.WarningContentLabel);
			GUILayout.EndArea();
		}
	}
}
