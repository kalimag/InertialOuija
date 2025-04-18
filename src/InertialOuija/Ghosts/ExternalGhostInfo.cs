extern alias GameScripts;

using GameScripts.Assets.Source.Enums;
using GameScripts.Assets.Source.Gameplay.GameModes;
using SQLite;
using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace InertialOuija.Ghosts;

[Serializable, SourceGenerators.ImplementISerializable]
public partial class ExternalGhostInfo : IEquatable<ExternalGhostInfo>, ISerializable
{
	[Indexed(Name = "IdxTrackCarTime", Order = 0)]
	public required Track Track { get; init; }
	[Indexed(Name = "IdxTrackCarTime", Order = 1)]
	public required TrackDirection Direction { get; init; }
	[Indexed(Name = "IdxTrackCarTime", Order = 2)]
	public required Car Car { get; init; }

	[Indexed(Name = "IdxTrackCarTime", Order = 3)]
	public required float TimeInSeconds { get; init; }
	//public float? Distance { get; }
	public float? StyleScore { get; init; }
	public int? PrecisionScore { get; init; }

	public required GhostSource Source { get; init; }
	public string EventType { get; init; }
	public string GameMode { get; init; }
	public Guid? RecordingId { get; init; }
	public required string Username { get; init; }
	public required ulong? SteamUserId { get; init; }
	/// <remarks>Only set for downloaded ghosts</remarks>
	public int? LeaderboardId { get; init; }
	public ulong? SteamFileId { get; init; }
	/// <remarks>Only set for uploaded ghosts</remarks>
	public GhostLeaderboardType LeaderboardType { get; init; }
	public bool StoryMode { get; init; }
	public int? PlayerIndex { get; init; }

	public required DateTimeOffset? Date { get; init; }

	public string GameVersion { get; init; }
	public string BuildGuid { get; init; }

	public TimeSpan Time => TimeSpan.FromSeconds(TimeInSeconds);

	// Keep in sync with GameExtensions.GetGhostType
	public GhostType? Type => GameMode switch
	{
		nameof(TimeAttack) or nameof(RaceMode) or nameof(FreeDriveMode) or nameof(GhostBattleMode) or nameof(DuelMode) => GhostType.Timed,
		nameof(EnduranceMode) => GhostType.Distance,
		nameof(StyleMode) => GhostType.Style,
		nameof(PrecisionStyleMode) => GhostType.Precision,
		_ => null,
	};


	internal ExternalGhostInfo()
	{
		GameVersion = Application.version;
		BuildGuid = Application.buildGUID;
	}


	public bool Equals(ExternalGhostInfo other)
	{
		return other != null &&
			Track == other.Track &&
			Direction == other.Direction &&
			Car == other.Car &&
			TimeInSeconds == other.TimeInSeconds &&
			RecordingId == other.RecordingId &&
			SteamFileId == other.SteamFileId;
	}

	public override bool Equals(object obj) => Equals(obj as ExternalGhostInfo);

	public override int GetHashCode()
	{
		int hashCode = 686532055;
		hashCode = hashCode * -1521134295 + Track.GetHashCode();
		hashCode = hashCode * -1521134295 + Direction.GetHashCode();
		hashCode = hashCode * -1521134295 + Car.GetHashCode();
		hashCode = hashCode * -1521134295 + TimeInSeconds.GetHashCode();
		hashCode = hashCode * -1521134295 + RecordingId.GetHashCode();
		hashCode = hashCode * -1521134295 + SteamFileId.GetHashCode();
		return hashCode;
	}
}
