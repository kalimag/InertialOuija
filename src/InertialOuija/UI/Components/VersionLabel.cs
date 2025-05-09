﻿extern alias GameScripts;
using System.Reflection;
using Doorstop;
using GameScripts.Assets.Source.UI.Menus;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InertialOuija.UI;

internal class VersionLabel : MonoBehaviour
{
	private string _label;

	void Awake()
	{
		useGUILayout = false;

		var version = typeof(Entrypoint).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
		_label = $"InertialOuija {version}";

		var build = typeof(Entrypoint).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration;
		if (build != "Release")
			_label += $" {build}";

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

		Styles.ApplySkin();
		Styles.ScaleRelative(0f, 1f);

		GUI.Label(Screen.safeArea, _label, Styles.Version);
	}
}
