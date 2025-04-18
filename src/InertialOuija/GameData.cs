extern alias GameScripts;

using GameScripts.Assets.Source.Tools;
using HarmonyLib;
using System;
using System.Threading;

namespace InertialOuija;

public static class GameData
{
	private static AccessTools.FieldRef<bool> _corePluginInitialized = AccessTools.StaticFieldRefAccess<bool>(AccessTools.Field(typeof(CorePlugin), "_initialised"));
	public static bool CorePluginInitialized => CorePlugin.CorePluginInstance() && _corePluginInitialized();


	private static readonly Lazy<(string, ulong)> _steamUser = new(() =>
	{
		if (!GameScripts.SteamManager.Initialized)
			throw new InvalidOperationException("SteamManager not initialized yet.");
		return (Steamworks.SteamFriends.GetPersonaName(), Steamworks.SteamUser.GetSteamID().m_SteamID);
	}, LazyThreadSafetyMode.PublicationOnly);

	public static (string Name, ulong Id) SteamUser => _steamUser.Value;
	public static (string Name, ulong Id)? TryGetSteamUser() => GameScripts.SteamManager.Initialized ? SteamUser : null;

}
