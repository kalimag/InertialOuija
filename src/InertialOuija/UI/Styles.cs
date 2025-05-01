using System;
using System.ComponentModel;
using System.IO;
using UnityEngine;
using static InertialOuija.Configuration.ModConfig;

namespace InertialOuija.UI;

internal partial class Styles
{
	private const float BaseDPI = 96f;

	private static readonly Lazy<AssetBundle> GuiAssets = new(() => AssetBundle.LoadFromFile(Path.Combine(FileUtility.ModDirectory, "gui.bundle")));
	private static readonly Lazy<GUISkin> GuiSkin = new(() => GuiAssets.Value.LoadAsset<GUISkin>("IDSkin"));

	public static void ApplySkin()
		=> GUI.skin = GuiSkin.Value;


	public static void Scale(float x, float y)
	{
		float scale = Screen.dpi / BaseDPI * Config.UI.ModScale;
		GUI.matrix = Matrix4x4.identity;
		GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), new Vector2(x, y));
	}

	public static void Scale()
		=> Scale(0f, 0f);

	public static void ScaleRelative(float x, float y)
		=> Scale(Screen.width * x, Screen.height * y);

	public static void NoScaling()
		=> GUI.matrix = Matrix4x4.identity;


	public static Rect GetScreenRect(TextAnchor anchor, Vector2 size, Vector2 margin = default)
	{
		float x = Screen.safeArea.x + anchor switch
		{
			TextAnchor.UpperLeft or TextAnchor.MiddleLeft or TextAnchor.LowerLeft => margin.x,
			TextAnchor.UpperCenter or TextAnchor.MiddleCenter or TextAnchor.LowerCenter => (Screen.safeArea.width - size.x) / 2 + margin.x,
			TextAnchor.UpperRight or TextAnchor.MiddleRight or TextAnchor.LowerRight => Screen.safeArea.width - size.x - margin.x,
			_ => throw new InvalidEnumArgumentException(nameof(anchor)),
		};
		float y = Screen.safeArea.y + anchor switch
		{
			TextAnchor.UpperLeft or TextAnchor.UpperCenter or TextAnchor.UpperRight => margin.y,
			TextAnchor.MiddleLeft or TextAnchor.MiddleCenter or TextAnchor.MiddleRight => (Screen.safeArea.height - size.y) / 2 + margin.y,
			TextAnchor.LowerLeft or TextAnchor.LowerCenter or TextAnchor.LowerRight => Screen.safeArea.height - size.y - margin.y,
			_ => throw new InvalidEnumArgumentException(nameof(anchor)),
		};
		return new(x, y, size.x, size.y);
	}



	public static GUIStyle CenteredLabel => "CenteredLabel";
	public static GUIStyle FixedButton => "FixedButton";
	public static GUIStyle MultilineLabel => "MultilineLabel";
	public static GUIStyle MultilineWrapLabel => "MultilineWrapLabel";
	public static GUIStyle RefreshProgress => "RefreshProgress";
	public static GUIStyle RefreshProgressLabel => "RefreshProgressLabel";
	public static GUIStyle RowLabel => "RowLabel";
	public static GUIStyle SeparatorStyle => "Separator";
	public static GUIStyle Toolbar => "Toolbar";
	public static GUIStyle Tooltip => "Tooltip";
	public static GUIStyle Version => "Version";
	public static GUIStyle Warning => "Warning";
	public static GUIStyle WarningContentLabel => "WarningContentLabel";



	private static GUIStyle _spaceStyle;
	public static void Space(int size = 16)
	{
		_spaceStyle ??= new() { stretchWidth = false };
		GUILayoutUtility.GetRect(size, size, _spaceStyle);
	}

	public static void Separator()
		=> GUILayout.Box(GUIContent.none, SeparatorStyle);

	public static HorizontalScope Row(string label = "")
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(label, RowLabel);
		return new();
	}

	public static HorizontalScope Horizontal()
	{
		GUILayout.BeginHorizontal();
		return new();
	}

	public static HorizontalScope Vertical()
	{
		GUILayout.BeginVertical();
		return new();
	}

	public static EnabledScope Enable(bool enabled, bool force = false)
	{
		var previousValue = GUI.enabled;
		GUI.enabled = enabled && (previousValue || force);
		return new(previousValue);
	}

	public readonly struct HorizontalScope : IDisposable
	{
		public void Dispose() => GUILayout.EndHorizontal();
	}

	public readonly struct VerticalScope : IDisposable
	{
		public void Dispose() => GUILayout.EndVertical();
	}

	public readonly struct EnabledScope(bool previousValue) : IDisposable
	{
		private readonly bool _previousValue = previousValue;
		public void Dispose() => GUI.enabled = _previousValue;
	}
}
