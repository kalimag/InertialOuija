extern alias GameScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GameScripts.Assets.Source.CarModel;
using GameScripts.Assets.Source.Enums;
using GameScripts.Assets.Source.Gameplay.GameModes;
using GameScripts.Assets.Source.GhostCars.GhostDatabases;
using GameScripts.Assets.Source.Localisation;
using GameScripts.Assets.Source.Tools;
using GameScripts.Assets.Source.UI.Menus;
using InertialOuija.Ghosts;
using UnityEngine;

namespace InertialOuija;

internal static class GameExtensions
{
	public static string GetInvariantString(this LocalisedString value)
	=> value?.GetLocalisedString(Language.English) ?? "";


	private readonly static Dictionary<Track, int> TrackOrder =
		((Track[])Enum.GetValues(typeof(Track)))
		.OrderBy(track => track.ToString())
		.Select((track, pos) => (track, pos))
		.ToDictionary(pair => pair.track, pair => pair.pos);
	public static string GetName(this Track track, TrackDirection direction = TrackDirection.Forward, bool prefixOrder = false)
	{
		string name = null;
		try
		{
			name = CorePlugin.TrackDatabase.GetTrackName(track, direction)?.GetInvariantString();
		}
		catch (NullReferenceException)
		{
			// CorePlugin not initialized
		}
		name ??= $"{track} {direction}";

		if (prefixOrder && TrackOrder.TryGetValue(track, out var order))
			name = $"{order:00} {name}";

		return name;
	}

	public static GhostLeaderboardType ToEnum(this GameScripts.LeaderboardType leaderboardType)
	{
		return leaderboardType switch
		{
			null => GhostLeaderboardType.None,
			{ Timed: true } => GhostLeaderboardType.Timed,
			{ Distance: true } => GhostLeaderboardType.Distance,
			{ Style: true } => GhostLeaderboardType.Style,
			_ => GhostLeaderboardType.Other,
		};
	}

	// Keep in sync with ExternalGhostInfo.Type
	public static GhostType? GetGhostType(this IGameMode mode) => mode switch
	{
		TimeAttack or RaceMode or FreeDriveMode or GhostBattleMode or DuelMode => GhostType.Timed,
		EnduranceMode => GhostType.Distance,
		StyleMode => GhostType.Style,
		PrecisionStyleMode => GhostType.Precision,
		_ => null,
	};


	private static readonly Lazy<Dictionary<Car, (string Name, PerformanceClassification PerfClass)>> CarDetails = new(() =>
	{
		return ((Car[])Enum.GetValues(typeof(Car)))
			.Select(CorePlugin.CarDatabase.GetCarDetails)
			.Where(details => details != null)
			.ToDictionary(
				details => details.CarType,
				details => (details.DisplayedName.GetInvariantString(), details.Class)
			);
	}, LazyThreadSafetyMode.PublicationOnly);

	public static string GetName(this Car car)
	{
		try
		{
			if (CarDetails.Value.TryGetValue(car, out var details))
				return details.Name;
		}
		catch (NullReferenceException)
		{
			// CorePlugin not initialized
		}
		return car.ToString();
	}

	public static PerformanceClassification GetClass(this Car car)
		=> CarDetails.Value[car].PerfClass;

	public static IEnumerable<Car> GetCars(this PerformanceClassification perfClass)
		=> CarDetails.Value.Where(car => car.Value.PerfClass == perfClass).Select(car => car.Key);


	public static RollingStartRecord Clone(this RollingStartRecord record) =>
		new()
		{
			GhostKey = record.GhostKey,
			Recording = record.Recording.ToList(),
			Speed = record.Speed
		};


	public static void IntegrateInLayout(this RectTransform item, int? index = null, int offset = 0)
	{
		var layout = item.GetComponentInParent<SimpleVerticalLayout>();
		if (!layout)
			return;

		int insertIndex = index ?? layout.Children.Length;

		Array.Resize(ref layout.Children, layout.Children.Length + 1);
		if (layout.LayoutInfo.Length < layout.Children.Length)
			Array.Resize(ref layout.LayoutInfo, layout.Children.Length);

		if (index.HasValue)
		{
			Array.Copy(layout.Children, insertIndex, layout.Children, insertIndex + 1, layout.Children.Length - insertIndex - 1);
			Array.Copy(layout.LayoutInfo, insertIndex, layout.LayoutInfo, insertIndex + 1, layout.LayoutInfo.Length - insertIndex - 1);
		}

		layout.Children[insertIndex] = item;
		layout.LayoutInfo[insertIndex] ??= new();
		layout.LayoutInfo[insertIndex].Offset = offset;
	}
}
