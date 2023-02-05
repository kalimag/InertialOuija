extern alias GameScripts;

using System;
using System.Collections.Generic;
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

	public static void SpawnExternalGhosts(GhostPlayer ghostPlayer)
	{
		Log.Debug(nameof(SpawnExternalGhosts), nameof(GhostController));

		if (Config.Ghosts.Mode == ExternalGhostMode.None || Config.Ghosts.Count < 1)
			return;

		Car? car = Config.Ghosts.SameCar ? CorePlugin.GameModeManager.PlayerInformation[0].CarPrefab.Car : null;

		var ghostFiles = ExternalGhostManager
			.GetGhosts(CorePlugin.GameModeManager.CurrentTrack, CorePlugin.GameModeManager.TrackDirection, car);

		if (Config.Ghosts.UniqueCars && !Config.Ghosts.SameCar)
		{
			ghostFiles = ghostFiles
				.GroupBy(ghost => ghost.Info.Car)
				.Select(group => group
					.OrderBy(ghost => ghost.Info.Time)
					.ThenByDescending(ghost => ghost.Info.Source != GhostSource.Leaderboard)
					.First()
				);
		}

		ghostFiles = ghostFiles.OrderBy(ghost => ghost.Info.Time)
			.ThenByDescending(ghost => ghost.Info.Source != GhostSource.Leaderboard)
			.Take(Config.Ghosts.Count);

		SpawnGhosts(ghostFiles, ghostPlayer);
	}

	private static async void SpawnGhosts(IEnumerable<ExternalGhostFile> ghostFiles, GhostPlayer ghostPlayer)
	{
		var nextPalettes = new Dictionary<Car, int>();

		foreach (var ghostFile in ghostFiles)
		{
			try
			{
				Log.Debug($"Load \"{ghostFile.Path}\"");
				var ghost = await ghostFile.LoadAsync();
				var carDetails = CorePlugin.CarDatabase.GetCarDetails(ghost.Info.Car);

				CarProperties carProperties;				
				if (Config.Ghosts.GhostVisual)
				{
					carProperties = GhostPlaybackPatches.SpawnGhost(ghostPlayer, carDetails, ghost.Recording, 2);
				}
				else
				{
					carProperties = GhostPlaybackPatches.SpawnCar(ghostPlayer, carDetails, ghost.Recording, 2);
					if (nextPalettes.TryGetValue(carProperties.CarVisualProperties.Car, out int nextPalette))
					{
						carProperties.CarVisualProperties.MaterialManager.ApplyPalette(nextPalette);
						nextPalettes[carProperties.CarVisualProperties.Car] = nextPalette + 1;
					}
					else
					{
						nextPalettes[carProperties.CarVisualProperties.Car] = 1;
					}
				}

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
