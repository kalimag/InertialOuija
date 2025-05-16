extern alias GameScripts;

using GameScripts.Assets.Source.Enums;
using GameScripts.Assets.Source.GhostCars.GhostDatabases;
using GameScripts.TinyJSON;
using InertialOuija.RollingStarts;
using System.Collections.Generic;

namespace InertialOuija.Configuration;

internal class GameConfig
{
	[Include] public Dictionary<string, Dictionary<Car, RollingStartPreference>> RollingStartPreferences { get; set; } = [];

	public RollingStartPreference GetRollingStartPreference(Track track, TrackDirection direction, Car car)
	{
		if (RollingStartPreferences.TryGetValue(track.GetName(direction), out var trackPrefs))
			if (trackPrefs.TryGetValue(car, out var pref))
				return pref;
		return RollingStartPreference.Fastest;
	}

	public RollingStartPreference GetRollingStartPreference(GhostKey ghostKey) =>
		GetRollingStartPreference(ghostKey.Track, ghostKey.Direction, ghostKey.Car);

	public void SetRollingStartPreference(Track track, TrackDirection direction, Car car, RollingStartPreference preference)
	{
		if (!RollingStartPreferences.TryGetValue(track.GetName(direction), out var trackPrefs))
		{
			trackPrefs = [];
			RollingStartPreferences[track.GetName(direction)] = trackPrefs;
		}

		if (preference == RollingStartPreference.Fastest)
			trackPrefs.Remove(car);
		else
			trackPrefs[car] = preference;
	}

	public void SetRollingStartPreference(GhostKey ghostKey, RollingStartPreference preference) =>
		SetRollingStartPreference(ghostKey.Track, ghostKey.Direction, ghostKey.Car, preference);
}
