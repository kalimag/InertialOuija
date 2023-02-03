extern alias GameScripts;

using System;
using System.Linq;
using GameScripts.Assets.Source.CarModel;
using GameScripts.Assets.Source.Enums;
using GameScripts.Assets.Source.GhostCars.GhostPlayback;
using GameScripts.Assets.Source.Tools;
using InertialOuija.Patches;
using UnityEngine;
using static InertialOuija.Configuration.ModConfig;

namespace InertialOuija.Ghosts;

internal static class GhostController
{

	public static async void SpawnExternalGhosts(GhostPlayer ghostPlayer)
	{
		Log.Debug(nameof(SpawnExternalGhosts), nameof(GhostController));

		if (Config.Ghosts.Mode == ExternalGhostMode.None || Config.Ghosts.Count < 1)
			return;

		Car? car = Config.Ghosts.SameCar ? CorePlugin.GameModeManager.PlayerInformation[0].CarPrefab.Car : null;

		var ghostFiles = ExternalGhostManager
			.GetGhosts(CorePlugin.GameModeManager.CurrentTrack, CorePlugin.GameModeManager.TrackDirection, car)
			.OrderBy(ghost => ghost.Info.Time)
			.Take(Config.Ghosts.Count);

		foreach (var ghostFile in ghostFiles)
		{
			try
			{
				Log.Debug($"Load \"{ghostFile.Path}\"");
				var ghost = await ghostFile.LoadAsync();
				var carDetails = CorePlugin.CarDatabase.GetCarDetails(ghost.Info.Car);
				CarProperties carProperties = GhostPlaybackPatches.SpawnGhost(ghostPlayer, carDetails, ghost.Recording, 2);

				if (ghost.Recording.IsEventStartLap())
					Log.Info($"IsEventStartLap={ghost.Recording.IsEventStartLap()} Source={ghost.Info.Source} EventType={ghost.Info.EventType} GameMode={ghost.Info.GameMode}");

				float eventStartTime = CorePlugin.EventManager.EventStartTime;
				if (eventStartTime >= 0f)
				{
					Log.Info($"eventStartTime is {eventStartTime} ({Time.time - eventStartTime})");
					carProperties.GetComponent<GhostPlayer>().SetPlaybackTime(Time.time - eventStartTime);
				}
			}
			catch (Exception ex)
			{
				Log.Error($"Could not load ghost from \"{ghostFile.Path}\"", ex);
			}
		}
	}
}
