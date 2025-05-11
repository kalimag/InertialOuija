extern alias GameScripts;

using GameScripts.Assets.Source.DebugTools;
using GameScripts.Assets.Source.Gameplay.GameModes;
using GameScripts.Assets.Source.Messaging;
using GameScripts.Assets.Source.Messaging.Messages;
using GameScripts.Assets.Source.Tools;
using DebugTools = GameScripts.Assets.Source.DebugTools;

namespace InertialOuija.Components;
internal class HudControl : BaseComponent, IReceiveMessages<StartEventMessage>, IReceiveMessages<RestartEventMessage>, IReceiveMessages<QuitEventMessage>
{
	public static bool HudVisible
	{
		get => DebugTools.ToggleHud.HudVisible;
		private set => DebugTools.ToggleHud.HudVisible = value;
	}

	public static void ToggleHud()
	{
		if (IsToggleHudAllowed() || !HudVisible)
		{
			HudVisible ^= true;
			MessagingCenter.BroadcastMessage<ToggleHudMessage>(default);
		}
	}

	public static void RestoreHud()
	{
		if (!HudVisible)
		{
			HudVisible = true;
			MessagingCenter.BroadcastMessage<ToggleHudMessage>(default);
		}
	}

	private static bool IsToggleHudAllowed()
		=> GameData.CorePluginInitialized && (CorePlugin.EventManager.InReplay || CorePlugin.EventManager.GameMode is FreeDriveMode);

	void IReceiveMessages<QuitEventMessage>.HandleMessage(QuitEventMessage message) => RestoreHud();

	// Just to be safe
	void IReceiveMessages<StartEventMessage>.HandleMessage(StartEventMessage message) => RestoreHud();

	void IReceiveMessages<RestartEventMessage>.HandleMessage(RestartEventMessage message) => RestoreHud();
}
