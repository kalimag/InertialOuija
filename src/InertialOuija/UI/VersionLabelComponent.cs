extern alias GameScripts;
using System.Reflection;
using Doorstop;
using GameScripts.Assets.Source.UI.Menus;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InertialOuija.UI;

internal class VersionLabelComponent : MonoBehaviour
{
	private string _label;

	void Awake()
	{
		var version = typeof(Entrypoint).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
		_label = $"InertialOuija {version}";

		SceneManager.activeSceneChanged += OnAciveSceneChanged;
	}

	private void OnAciveSceneChanged(Scene prev, Scene next)
	{
		enabled = next.name == "MainMenu";
	}

	void OnGUI()
	{
		if (!MenuNavigator.ActiveMenu || MenuNavigator.ActiveMenu.name != "MainMenuCanvas")
			return;

		Styles.ScaleRelative(0f, 1f);

		GUI.Label(Screen.safeArea, _label, Styles.Version.LabelStyle);
	}
}

partial class Styles
{
	public static class Version
	{
		public static readonly GUIStyle LabelStyle = new(GUI.skin.label)
		{
			fontSize = 15,
			padding = new RectOffset(10, 10, 10, 10),
			alignment = TextAnchor.LowerLeft
		};
	}
}