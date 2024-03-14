extern alias GameScripts;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GameScripts.Assets.Source.CloudStorage;
using GameScripts.Assets.Source.Enums;
using GameScripts.Assets.Source.Gameplay.GameModes;
using GameScripts.Assets.Source.GhostCars.GhostDatabases;
using GameScripts.Assets.Source.GhostCars.GhostLaps;
using GameScripts.Assets.Source.Tools;
using static InertialOuija.Configuration.ModConfig;

namespace InertialOuija.Ghosts;

internal class ExternalGhostManager
{
	private static readonly string GhostsExtension = ".ghost";
	private static readonly string GhostTimeFormat = @"mm\.ss\.fff";

	private static readonly ConcurrentDictionary<string, ExternalGhostFile> GhostFiles = new(1, 0);
	private static readonly ConcurrentDictionary<ExternalGhostInfo, ExternalGhostFile> UniqueGhosts = new(1, 0);



	public static int Count => UniqueGhosts.Count;

	public static string GhostsPath
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(Config.Ghosts.Directory))
				return Config.Ghosts.Directory;
			else
				return Path.Combine(Path.GetDirectoryName(UnityEngine.Application.dataPath), "Ghosts");
		}
	}



	public static void AddPlayerGhost(Track track, TrackDirection direction, Car car, GhostLap lap, int? playerIndex)
	{
		Log.Info($"{nameof(AddPlayerGhost)}({track}, {direction}, {car}, {lap.GetTotalTime()})", nameof(ExternalGhostManager));

		var info = new ExternalGhostInfo
		{
			Track = track,
			Direction = direction,
			Car = car,
			TimeInSeconds = lap.GetTotalTime(),
			Source = GhostSource.Player,
			EventType = CorePlugin.GameModeManager.CurrentEventDetails?.EventTitle.GetInvariantString(),
			GameMode = CorePlugin.GameModeManager.GameModePrefab?.GetComponent<IGameMode>()?.GetType().Name,
			LeaderboardType = (CorePlugin.GameModeManager.CurrentEventDetails?.LeaderboardType).ToEnum(),
			StoryMode = CorePlugin.GameModeManager.CurrentEventDetails?.StoryMission == true,
			RecordingId = lap.RecordingId,
			Username = CorePlugin.PlatformManager.PrimaryUserName(),
			SteamUserId = GameScripts.SteamManager.Initialized ? Steamworks.SteamUser.GetSteamID().m_SteamID : null,
			PlayerIndex = CorePlugin.GameModeManager.ActivePlayers > 1 || playerIndex > 0 ? playerIndex : null,
			Date = DateTimeOffset.UtcNow,
		};

		Log.Debug($"CurrentEventDetails is {CorePlugin.GameModeManager.CurrentEventDetails?.GetType()}");
		Log.Debug($"StoryMode= {info.StoryMode}");

		var ghost = new ExternalGhost(info, lap);
		Task.Run(() => SaveGhost(ghost)).LogFailure();
	}

	public static void AddLeaderboardGhost(GhostDownloadRequest request, CompressionFriendlyGhostRecording packedGhost)
	{
		Log.Info($"AddLeaderboardGhost(B{request.BoardId} {request.UserDetails.Username}, {packedGhost.FriendlyString()}", nameof(ExternalGhostManager));

		var leaderboard = GameScripts.LeaderboardIdMapping.GetLeaderboardDetails(request.BoardId);

		var info = new ExternalGhostInfo
		{
			Track = packedGhost.Track,
			Direction = packedGhost.Direction,
			Car = packedGhost.Car,
			TimeInSeconds = packedGhost.TotalTime,
			Source = GhostSource.Leaderboard,
			EventType = CorePlugin.GameModeManager.CurrentEventDetails?.EventTitle.GetInvariantString(),
			GameMode = CorePlugin.GameModeManager.GameModePrefab?.GetComponent<IGameMode>()?.GetType().Name,
			Username = request.UserDetails.Username,
			SteamUserId = request.UserDetails.SteamUserId.m_SteamID,
			SteamFileId = request.UserDetails.DataHandle.m_UGCHandle,
			LeaderboardId = request.BoardId,
			Date = DateTimeOffset.UtcNow,
		};

		if (!UniqueGhosts.TryGetValue(info, out var existingGhost))
		{
			var ghost = new ExternalGhost(info, request.Data);
			Task.Run(() => SaveGhost(ghost)).LogFailure();
		}
		else
		{
			Log.Debug($"Downloaded ghost already exists at \"{existingGhost.Path}\"");
		}
	}

	public static void ExportPlayerDatabase()
	{
		RefreshDatabase();

		Log.Info("Export player database...");

		List<GhostRecord> savedGhosts;
		try
		{
			savedGhosts = CorePlugin.SaveManager.Load<List<GhostRecord>>("PlayerGhosts");
		}
		catch (Exception ex)
		{
			Log.Error("Failed to load saved player ghosts", ex);
			return;
		}

		ulong? steamId = GameScripts.SteamManager.Initialized ? Steamworks.SteamUser.GetSteamID().m_SteamID : null;

		foreach (var savedGhost in savedGhosts)
		{
			var info = new ExternalGhostInfo
			{
				Track = savedGhost.GhostKey.Track,
				Direction = savedGhost.GhostKey.Direction,
				Car = savedGhost.GhostKey.Car,
				TimeInSeconds = savedGhost.Recording.GetTotalTime(),
				Source = GhostSource.PlayerDatabaseExport,
				RecordingId = savedGhost.Recording.RecordingId,
				Username = CorePlugin.PlatformManager.PrimaryUserName(),
				SteamUserId = steamId,
				Date = DateTimeOffset.UtcNow
			};

			if (UniqueGhosts.TryGetValue(info, out var existingGhostFile))
			{
				Log.Debug($"Ghost {info} already exported at \"{existingGhostFile.Path}\"");
				continue;
			}

			var ghost = new ExternalGhost(info, savedGhost.Recording);
			SaveGhost(ghost);

			if (savedGhost.Time != savedGhost.Recording.LapTime)
				Log.Debug($"{savedGhost.GhostKey} {savedGhost.Time} - {savedGhost.Recording.LapTime} = {savedGhost.Time - savedGhost.Recording.LapTime}");
		}
	}

	private static void SaveGhost(ExternalGhost ghost)
	{
		(string directory, string fileName) = GetSavePath(ghost.Info);
		using var stream = FileUtility.CreateUniqueFile(directory, fileName, GhostsExtension);
		Log.Info($"Save ghost \"{stream.Name}\"");

		try
		{
			ghost.Save(stream);
		}
		catch (Exception)
		{
			try
			{
				stream.Close();
				if (File.Exists(stream.Name))
					File.Move(stream.Name, stream.Name + ".error");
			}
			catch (Exception ex)
			{
				Log.Error("Couldn't rename errored ghost", ex);
			}
			throw;
		}

		var path = Path.GetFullPath(stream.Name);
		var ghostFile = new ExternalGhostFile(path, ghost.Info);

		GhostFiles[path] = ghostFile;
		if (!UniqueGhosts.TryAdd(ghost.Info, ghostFile))
			Log.Debug($"Duplicate ghosts: \"{path}\"");
	}

	public static void RefreshDatabase()
	{
		Log.Debug(nameof(RefreshDatabase), nameof(ExternalGhostManager));

		GhostFiles.Clear();
		UniqueGhosts.Clear();

		string[] files;
		try
		{
			files = Directory.GetFiles(GhostsPath, "*" + GhostsExtension, SearchOption.AllDirectories);
		}
		catch (Exception ex)
		{
			Log.Error($"Failed to enumerate ghost directory", ex);
			return;
		}

		foreach (var file in files)
		{
			ExternalGhostInfo info;
			try
			{
				info = ExternalGhost.LoadInfo(file);
				//Log.Debug($"Ghost {info.Track} {info.Direction} {info.Car} {info.Time} {info.Username} {info.Source}");

				var path = Path.GetFullPath(file);
				var ghostFile = new ExternalGhostFile(path, info);

				GhostFiles[path] = new ExternalGhostFile(path, info);
				if (!UniqueGhosts.TryAdd(info, ghostFile) && UniqueGhosts.TryGetValue(info, out var existingGhost))
					Log.Debug($"Duplicate ghosts: \"{path}\" and \"{existingGhost.Path}\"");
			}
			catch (Exception ex)
			{
				Log.Error($"Failed to load ghost info from \"{file}\"", ex);
			}
		}
		Log.Info($"Found {GhostFiles.Count} valid ghost files ({files.Length} {GhostsExtension} files)");
	}

	public static IEnumerable<ExternalGhostFile> GetGhosts(Track track, TrackDirection direction, Car? car = null)
	{
		return UniqueGhosts.Values.Where(ghost => ghost.Info.Track == track && ghost.Info.Direction == direction && (car == null || ghost.Info.Car == car));
	}

	public static ExternalGhostInfo GetPersonalBestTime(Track track, TrackDirection direction, Car? car = null)
	{
		if (!GameScripts.SteamManager.Initialized)
			return null;

		string name = CorePlugin.PlatformManager.PrimaryUserName();
		ulong id = Steamworks.SteamUser.GetSteamID().m_SteamID;

		var ghosts = UniqueGhosts.Values
			.Where(ghost => (ghost.Info.SteamUserId == id || ghost.Info.Username == name) &&
			ghost.Info.Track == track && ghost.Info.Direction == direction && (car == null || ghost.Info.Car == car));

		ExternalGhostInfo best = null;
		foreach (var ghost in ghosts)
			if (best == null || ghost.Info.Time < best.Time)
				best = ghost.Info;

		return best;
	}

	private static (string Directory, string FileName) GetSavePath(ExternalGhostInfo info)
	{
		var directory = Path.Combine(
			GhostsPath,
			info.Type is not null and not GhostType.Timed ? info.Type.ToString() : "",
			FileUtility.Sanitize(info.Track.GetName(info.Direction, true)),
			FileUtility.Sanitize(info.Car.GetName())
		);

		var fileName = $"{info.Time.ToString(GhostTimeFormat, CultureInfo.InvariantCulture)} {FileUtility.Sanitize(info.Username)}";

		if (info.PlayerIndex != null && info.GameMode != nameof(OnlineRaceMode))
			fileName += $" [{info.PlayerIndex + 1}]";

		var additional = new List<string>(0);
		if (!string.IsNullOrEmpty(info.EventType))
			additional.Add(info.EventType);
		if (info.Date != null && info.Source == GhostSource.Player)
			additional.Add(info.Date.Value.ToLocalTime().ToString("yyyy-MM-dd HH-mm"));
		if (info.Source == GhostSource.Leaderboard)
			additional.Add("Leaderboard");
		if (info.Source == GhostSource.PlayerDatabaseExport)
			additional.Add("Export");
		if (additional.Count > 0)
			fileName = fileName + " (" + String.Join(" ", additional) + ")";

		return (directory, fileName);
	}
}
