using UnityEngine;

namespace InertialOuija.UI;

internal partial class Styles
{
	private const float BaseDPI = 96f;

	public static void Scale(float x, float y)
	{
		float scale = Screen.dpi / BaseDPI;
		GUI.matrix = Matrix4x4.identity;
		GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), new Vector2(x, y));
	}

	public static void Scale()
		=> Scale(0f, 0f);

	public static void ScaleRelative(float x, float y)
		=> Scale(Screen.width * x, Screen.height * y);

	public static void NoScaling()
	{
		GUI.matrix = Matrix4x4.identity;
	}


	public static readonly GUILayoutOption ExpandHeight = GUILayout.ExpandHeight(true);
	public static readonly GUILayoutOption ExpandWidth = GUILayout.ExpandWidth(true);
	public static readonly GUILayoutOption[] ExpandBoth = new[] { GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true) };

	public static readonly GUILayoutOption DontExpandHeight = GUILayout.ExpandHeight(false);
	public static readonly GUILayoutOption DontExpandWidth = GUILayout.ExpandWidth(false);
	public static readonly GUILayoutOption[] DontExpandBoth = new[] { GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false) };

	public static readonly GUILayoutOption Width100 = GUILayout.Width(100);
}
