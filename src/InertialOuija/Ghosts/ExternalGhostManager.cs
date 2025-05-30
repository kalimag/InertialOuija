extern alias GameScripts;
using GameScripts.Assets.Source.CarModel;
using GameScripts.Assets.Source.CloudStorage;
using GameScripts.Assets.Source.Gameplay;
using GameScripts.Assets.Source.Gameplay.GameModes;
using GameScripts.Assets.Source.Gameplay.Scoring;
using GameScripts.Assets.Source.GhostCars.GhostDatabases;
using GameScripts.Assets.Source.GhostCars.GhostLaps;
using GameScripts::Assets.Source.SaveData;
using GameScripts.Assets.Source.Tools;
using InertialOuija.Components;
using InertialOuija.Ghosts.Database;
using InertialOuija.UI;
using InertialOuija.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static InertialOuija.Configuration.ModConfig;

namespace InertialOuija.Ghosts;

internal class ExternalGhostManager
{
	private static readonly string GhostsExtension = ".ghost";

	private static bool _unknownVersionShown;

	public static ExternalGhostDatabase Ghosts { get; private set; }

	public static bool RefreshInProgress { get; private set; }

	public static int Count => Ghosts.Count;

	public static string GhostsPath
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(Config.Ghosts.Directory))
				return Path.GetFullPath(Config.Ghosts.Directory);
			else
				return Path.Combine(FileUtility.GameDirectory, "Ghosts");
		}
	}



	internal static async void Initialize()
	{
		if (Ghosts != null)
			return;
		try
		{
			Ghosts = new ExternalGhostDatabase();
			await RefreshDatabaseAsync();
			if (!Config.Ghosts.PlayerGhostsExported)
				await ExportPlayerGhosts();
		}
		catch (Exception ex)
		{
			Log.Error(ex);
		}
	}

	public static void AddPlayerGhost(GhostLap lap, CarProperties carProperties)
	{
		Log.Info($"{nameof(AddPlayerGhost)}()", nameof(ExternalGhostManager));

		var info = new ExternalGhostInfo
		{
			Track = TrackInfo.CurrentTrack(),
			Direction = CorePlugin.GameModeManager.TrackDirection,
			Car = carProperties.CarVisualProperties.Car,
			TimeInSeconds = lap.GetTotalTime(),
			//Distance = carProperties.GetComponentInChildren<DistanceTracker>()?.DistanceTravelled,
			StyleScore = carProperties.GetComponentInChildren<DriftScoreTracker>()?.TotalScore,
			PrecisionScore = carProperties.GetComponent<StyleCounterRef>()?.StyleCounter.Points,
			Source = GhostSource.Player,
			EventType = CorePlugin.GameModeManager.CurrentEventDetails?.EventTitle.GetInvariantString(),
			GameMode = CorePlugin.GameModeManager.GameModePrefab?.GetComponent<IGameMode>()?.GetType().Name,
			LeaderboardType = (CorePlugin.GameModeManager.CurrentEventDetails?.LeaderboardType).ToEnum(),
			StoryMode = CorePlugin.GameModeManager.CurrentEventDetails?.StoryMission == true,
			RecordingId = lap.RecordingId,
			Username = CorePlugin.PlatformManager.PrimaryUserName(),
			SteamUserId = GameScripts.SteamManager.Initialized ? Steamworks.SteamUser.GetSteamID().m_SteamID : null,
			PlayerIndex = CorePlugin.GameModeManager.ActivePlayers > 1 || carProperties.CarVisualProperties.CarId > 0 ? carProperties.CarVisualProperties.CarId : null,
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

		if (!Ghosts.Contains(info))
		{
			var ghost = new ExternalGhost(info, request.Data);
			Task.Run(() => SaveGhost(ghost)).LogFailure();
		}
		else
		{
			Log.Debug($"Downloaded ghost already exists");
		}
	}

	public async static Task ExportPlayerGhosts()
	{
		if (RefreshInProgress)
			throw new InvalidOperationException("ExternalGhostManager is busy");
		RefreshInProgress = true;

		try
		{
			Log.Info("Export player database...");

			List<GhostRecord> savedGhosts;
			try
			{
				savedGhosts = await (Config.Patches.DisablePlayerGhosts ? GetSteamCloudGhosts() : GetSaveManagerGhosts());
			}
			catch (Exception ex)
			{
				Log.Error("Failed to load saved player ghosts", ex);
				return;
			}

			if (savedGhosts is null)
			{
				Log.Info("No PlayerGhosts found");
				return;
			}

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
					Username = GameData.SteamUser.Name,
					SteamUserId = GameData.SteamUser.Id,
					Date = DateTimeOffset.UtcNow
				};

				if (Ghosts.Contains(info))
				{
					Log.Debug($"Ghost {savedGhost.GhostKey} already exported");
					continue;
				}

				var ghost = new ExternalGhost(info, savedGhost.Recording);
				await Task.Run(() => SaveGhost(ghost));

				if (savedGhost.Time != savedGhost.Recording.LapTime)
					Log.Debug($"{savedGhost.GhostKey} {savedGhost.Time} - {savedGhost.Recording.LapTime} = {savedGhost.Time - savedGhost.Recording.LapTime}");
			}

			Config.Ghosts.PlayerGhostsExported = true;
			Config.Save();
		}
		finally
		{
			RefreshInProgress = false;
		}

		static async Task<List<GhostRecord>> GetSaveManagerGhosts()
		{
			while (!GameData.CorePluginInitialized || !CorePlugin.SaveManager.IsReady())
				await Task.Yield();

			return CorePlugin.SaveManager.Load<List<GhostRecord>>(SaveFileKeys.PlayerGhostKey);
		}

		static async Task<List<GhostRecord>> GetSteamCloudGhosts()
		{
			while (!GameScripts.SteamManager.Initialized)
				await Task.Yield();

			var buffer = await SteamUtility.ReadFileAsync(SaveFileKeys.PlayerGhostKey);
			if (buffer is null)
				return [];

			var records = await Task.Run(() => (List<GhostRecord>)SaveHelpers.Deserialise(buffer));
			Log.Debug($"Read {records.Count} ghosts from Steam storage");
			return records;
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

		if (Ghosts != null)
		{
			var ghostFile = ExternalGhostFile.FromInfo(new FileInfo(stream.Name), ghost.Info);

			if (Ghosts.Contains(ghost.Info))
				Log.Info($"Saved duplicate ghost: \"{ghostFile.Path}\"");
			Ghosts.Write(() => Ghosts.Add(ghostFile));
		}
	}

	public static async Task RefreshDatabaseAsync()
	{
		if (RefreshInProgress)
			throw new InvalidOperationException("ExternalGhostManager is busy");

		RefreshInProgress = true;
		var progress = MainController.CreatePersistentObject<RefreshProgressDisplay>("RefreshProgress");

		try
		{
			await Task.Run(() => RefreshDatabaseInternal(progress));
		}
		finally
		{
			UnityEngine.Object.Destroy(progress.gameObject);
			RefreshInProgress = false;
		}
	}

	private static void RefreshDatabaseInternal(IProgress<float> progress = null)
	{
		Log.Debug(nameof(RefreshDatabaseInternal), nameof(ExternalGhostManager));

		var unprocessedFiles = new HashSet<string>();
		var missingFiles = new List<string>();

		int cacheItemCount = 0;
		int processed = 0;

		// this should all happen as one transaction, to avoid a situation where a new ghost is added
		// after the files have been enumerated but before the database is enumerated
		Ghosts.Write(() =>
		{
			try
			{
				Log.Info($"Enumerate \"{GhostsPath}\"...");
				foreach (var file in Directory.GetFiles(GhostsPath, "*" + GhostsExtension, SearchOption.AllDirectories))
					unprocessedFiles.Add(FileUtility.PrefixLongPath(file));
			}
			catch (DirectoryNotFoundException)
			{
				Log.Info("Ghost directory doesn't exist");
			}
			catch (Exception ex)
			{
				Log.Error($"Failed to enumerate ghost directory", ex);
				return;
			}
			Log.Info($"Found {unprocessedFiles.Count} {GhostsExtension} files");

			cacheItemCount = Ghosts.GetCount(true); // for progress calculation

			foreach (var cachedFile in Ghosts.EnumerateFiles())
			{
				if (unprocessedFiles.Remove(cachedFile.Path))
				{
					try
					{
						var fileInfo = new FileInfo(cachedFile.Path);
						if (!fileInfo.Exists) // double check, FileInfo loads and caches all these properties at once
							missingFiles.Add(cachedFile.Path);
						else if (fileInfo.Length != cachedFile.Size || fileInfo.LastWriteTimeUtc != cachedFile.LastWrite)
							unprocessedFiles.Add(cachedFile.Path);
					}
					catch (Exception ex)
					{
						Log.Error($"Unable to get file info for \"{cachedFile.Path}\"", ex);
					}
				}
				else
				{
					missingFiles.Add(cachedFile.Path);
				}
				IncrementProgress();
			}

			Log.Info($"Removing {missingFiles.Count} missing files...");
			foreach (var file in missingFiles)
			{
				Ghosts.Remove(file);
				IncrementProgress();
			}
		});

		Log.Info($"Loading {unprocessedFiles.Count} new/stale files...");
		foreach (var file in unprocessedFiles)
		{
			FileInfo fileInfo = null;
			try
			{
				fileInfo = new FileInfo(file);
				var info = ExternalGhost.LoadInfo(file);
				Log.Debug($"New ghost: {info.Track} {info.Direction} {info.Car} {info.Time} {info.Username} {info.Source} (\"{file}\")");
				Ghosts.Write(() => Ghosts.Add(ExternalGhostFile.FromInfo(fileInfo, info)));
			}
			catch (UnknownGhostVersionException ex)
			{
				Log.Error(ex, true);
				if (!_unknownVersionShown)
				{
					ErrorWindow.ShowError($"Found a ghost with an unrecognized version ({ex.Version}), you probably need to update your mod.\n\nFile: {file}");
					_unknownVersionShown = true;
				}
			}
			catch (Exception ex)
			{
				if (fileInfo != null && !ex.IsFileSystemException()) // exception probably not related to the file contents and shouldn't be cached
					Ghosts.Write(() => Ghosts.AddError(fileInfo));
				Log.Error($"Failed to load ghost info from \"{file}\"", ex, true);
			}
			IncrementProgress();
		}

		Log.Info($"Database now contains {Ghosts.Count} valid ghost files");

		Log.Debug($"Checkpointing...");
		Ghosts.PassiveCheckpoint();

		progress?.Report(1);
		Log.Debug("Done");

		void IncrementProgress()
		{
			processed++;
			if (processed % 10 == 0)
			{
				float percentage = (float)processed / (cacheItemCount + unprocessedFiles.Count + missingFiles.Count);
				progress?.Report(float.IsNaN(percentage) ? 0 : percentage);
			}
		}
	}

	public static async Task ResetCache()
	{
		await Ghosts.WriteAsync(Ghosts.Clear);
		await RefreshDatabaseAsync();
	}

	private static (string Directory, string FileName) GetSavePath(ExternalGhostInfo info)
	{
		var directory = Path.Combine(
			GhostsPath,
			info.Type is GhostType.Precision or GhostType.Style ? info.Type.ToString() : "",
			FileUtility.Sanitize(info.Track.GetName(info.Direction, true)),
			FileUtility.Sanitize(info.Car.GetName())
		);

		string fileName = "";

		if (info.GameMode == nameof(StyleMode) && info.StyleScore != null)
			fileName = $"{(int)info.StyleScore} ";
		else if (info.GameMode == nameof(PrecisionStyleMode) && info.PrecisionScore != null)
			fileName = $"{info.PrecisionScore} ";

		fileName += $"{info.Time.ToString(true, ".")} {FileUtility.Sanitize(info.Username)}";

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
