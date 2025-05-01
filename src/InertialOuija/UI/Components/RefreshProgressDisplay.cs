extern alias GameScripts;

using InertialOuija.Utilities;
using System;
using UnityEngine;

namespace InertialOuija.UI;

internal sealed class RefreshProgressDisplay : MonoBehaviour, IProgress<float>
{
	private static readonly Vector2 Size = new(270, 50);
	private static readonly Vector2 Margin = new(10, 10);


	private readonly StringCache<double> _text = new(static value => $"{value:0}%");

	private volatile float _progress;

	void OnGUI()
	{
		Styles.ApplySkin();
		Styles.ScaleRelative(0.5f, 1f);

		var text = _text.GetString(Mathf.Round(_progress * 100));

		GUILayout.BeginArea(Styles.GetScreenRect(TextAnchor.LowerCenter, Size, Margin), Styles.RefreshProgress);
		GUILayout.BeginHorizontal();
		GUILayout.Label("Loading ghosts:", Styles.RefreshProgressLabel);
		GUILayout.FlexibleSpace();
		GUILayout.Label(text, Styles.RefreshProgressLabel);
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}

	public void Report(float value) => _progress = value;
}
