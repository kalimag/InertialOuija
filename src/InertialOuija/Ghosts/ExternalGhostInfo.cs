extern alias GameScripts;

using System;
using GameScripts.Assets.Source.Enums;
using GameScripts.Assets.Source.Gameplay.GameModes;
using UnityEngine;

namespace InertialOuija.Ghosts;

[Serializable]
public class ExternalGhostInfo : IEquatable<ExternalGhostInfo>
{
	public static int CurrentVersion => 1;

	[NonSerialized]
	public readonly int ExternalGhostVersion;

	public Track Track;
	public TrackDirection Direction;
	public Car Car;

	public float TimeInSeconds;

	public GhostSource Source;
	public string EventType;
	public string GameMode;
	public Guid? RecordingId;
	public string Username;
	public ulong? SteamUserId;
	/// <remarks>Only set for downloaded ghosts</remarks>
	public int? LeaderboardId;
	public ulong? SteamFileId;
	/// <remarks>Only set for uploaded ghosts</remarks>
	public GhostLeaderboardType LeaderboardType;
	public bool StoryMode;
	public int? PlayerIndex;

	public DateTimeOffset? Date;

	public string GameVersion;
	public string BuildGuid;


	public TimeSpan Time => TimeSpan.FromSeconds(TimeInSeconds);

	public GhostType? Type => GameMode switch
	{
		nameof(TimeAttack) or nameof(RaceMode) or nameof(FreeDriveMode) or nameof(GhostBattleMode) or nameof(DuelMode) => GhostType.Timed,
		nameof(EnduranceMode) => GhostType.Distance,
		nameof(StyleMode) => GhostType.Style,
		nameof(PrecisionStyleMode) => GhostType.Precision,
		_ => null,
	};


	internal ExternalGhostInfo(int externalGhostVersion)
	{
		ExternalGhostVersion = externalGhostVersion;
		GameVersion = Application.version;
		BuildGuid = Application.buildGUID;
	}
	internal ExternalGhostInfo() : this(CurrentVersion)
	{ }


	public bool Equals(ExternalGhostInfo other)
	{
		return other != null &&
			Track == other.Track &&
			Direction == other.Direction &&
			Car == other.Car &&
			TimeInSeconds == other.TimeInSeconds &&
			RecordingId == other.RecordingId &&
			SteamFileId == other.SteamFileId &&
			LeaderboardId == other.LeaderboardId;
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
		hashCode = hashCode * -1521134295 + LeaderboardId.GetHashCode();
		return hashCode;
	}
}
