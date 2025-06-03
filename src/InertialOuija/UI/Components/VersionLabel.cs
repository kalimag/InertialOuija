extern alias GameScripts;
using System.Reflection;
using Doorstop;
using GameScripts.Assets.Source.Messaging;
using GameScripts.Assets.Source.Messaging.Messages;
using GameScripts.Assets.Source.Tools;
using GameScripts.Assets.Source.UI.Menus;
using UnityEngine;

namespace InertialOuija.UI;

internal class VersionLabel : BaseComponent, IReceiveMessages<ShowMainMenuMessage>, IReceiveMessages<HideMainMenuMessage>
{
	private string _label;

	public override void Awake()
	{
		base.Awake();

		useGUILayout = false;

		var version = typeof(Entrypoint).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
		_label = $"InertialOuija {version}";

		var build = typeof(Entrypoint).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration;
		if (build != "Release")
			_label += $" {build}";
	}

	void IReceiveMessages<ShowMainMenuMessage>.HandleMessage(ShowMainMenuMessage message) => enabled = true;
	void IReceiveMessages<HideMainMenuMessage>.HandleMessage(HideMainMenuMessage message) => enabled = false;

	void OnGUI()
	{
		if (!GameData.CorePluginInitialized || CorePlugin.LoadingScreen.IsLoading || !MenuNavigator.ActiveMenu ||
			MenuNavigator.ActiveMenu.name is not ("MainMenuCanvas" or "TrackSelectCanvas" or "EventSelectFreeDriveCanvas" or "CarSelectCanvas1"))
			return;

		Styles.ApplySkin();
		Styles.ScaleRelative(0f, 1f);

		GUI.Label(Screen.safeArea, _label, Styles.Version);
	}
}
