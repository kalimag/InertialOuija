﻿extern alias GameScripts;

using GameScripts.Assets.Source.Enums;
using GameScripts.Assets.Source.Tools;
using GameScripts.Assets.Source.UI.Menus;
using HarmonyLib;
using System;
using System.Threading;
using UnityEngine;

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


	public static Car? FirstPlayerCar
	{
		get
		{
			if (!CorePluginInitialized)
				return null;
			else if (CorePlugin.GameModeManager.PlayerInformation.Count > 0)
				return CorePlugin.GameModeManager.PlayerInformation[0].CarPrefab.Car;
			else if (MenuSelection.CurrentCharacter != Character.None)
				return CorePlugin.CharacterDatabase.GetCharacterData(MenuSelection.CurrentCharacter)?.CarPrefab.Car;
			else
				return null;
		}
	}


	public static string DataPath { get; private set; }
	public static string Version { get; private set; }
	public static string BuildGuid { get; private set; }



	internal static void Initialize()
	{
		if (SynchronizationContext.Current is null)
			throw new InvalidOperationException("Must be initialized from main thread");
		DataPath = Application.dataPath;
		Version = Application.unityVersion;
		BuildGuid = Application.buildGUID;
	}
}
