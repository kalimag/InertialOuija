extern alias GameScripts;

using InertialOuija.Utilities;
using System;
using UnityEngine;

namespace InertialOuija.UI;

internal sealed class RefreshProgressDisplay : MonoBehaviour, IProgress<float>
{
	private readonly StringCache<double> _text = new(static value => $"Loading ghosts: {value:0}%");

	private volatile float _progress;

	void Awake()
	{
		useGUILayout = false;
	}

	void OnGUI()
	{
		Styles.ScaleRelative(1f, 1f);
		var text = _text.GetString(Mathf.Round(_progress * 100));
		GUI.Label(new Rect(Screen.width - 200, Screen.height - 50, 190, 40), text, Styles.RefreshProgress.LabelStyle);
	}

	public void Report(float value) => _progress = value;
}

partial class Styles
{
	public static class RefreshProgress
	{
		public static readonly GUIStyle LabelStyle = new(GUI.skin.box)
		{
			fontSize = 15,
			alignment = TextAnchor.MiddleCenter,
			wordWrap = false,
		};
	}
}
