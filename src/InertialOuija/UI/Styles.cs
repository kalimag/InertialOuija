using UnityEngine;

namespace InertialOuija.UI;

internal partial class Styles
{
	private const float BaseDPI = 96f;

	public static void Initialize()
	{
		float scale = Screen.dpi * 2 / BaseDPI;
		GUIUtility.ScaleAroundPivot(new(scale, scale), Vector2.zero);
	}

	public static readonly GUILayoutOption ExpandHeight = GUILayout.ExpandHeight(true);
	public static readonly GUILayoutOption ExpandWidth = GUILayout.ExpandWidth(true);
	public static readonly GUILayoutOption[] ExpandBoth = new[] { GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true) };

	public static readonly GUILayoutOption DontExpandHeight = GUILayout.ExpandHeight(false);
	public static readonly GUILayoutOption DontExpandWidth = GUILayout.ExpandWidth(false);
	public static readonly GUILayoutOption[] DontExpandBoth = new[] { GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false) };

	public static readonly GUILayoutOption Width100 = GUILayout.Width(100);
}
