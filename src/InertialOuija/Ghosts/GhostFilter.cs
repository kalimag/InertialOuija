extern alias GameScripts;

using GameScripts.Assets.Source.CarModel;
using GameScripts.Assets.Source.Enums;
using static InertialOuija.Configuration.ModConfig;

namespace InertialOuija.Ghosts;

public record struct GhostFilter(
	GhostType Type,
	Track Track,
	TrackDirection Direction,
	Car? Car = null,
	PerformanceClassification? PerformanceClass = null,
	bool UniqueCars = false,
	ulong? User = null
)
{
	public static GhostFilter FromConfig(GhostType type, Track track, TrackDirection direction, Car car) =>
		new(
			type,
			track,
			direction,
			Config.Ghosts.CarFilter == CarFilter.SameCar ? car : null,
			Config.Ghosts.CarFilter == CarFilter.SameClass ? car.GetClass() : null,
			Config.Ghosts.UniqueCars && Config.Ghosts.CarFilter != CarFilter.SameCar && Config.Ghosts.Mode != ExternalGhostMode.NextBest,
			Config.Ghosts.MyGhosts ? GameData.SteamUser.Id : null
		);
}
