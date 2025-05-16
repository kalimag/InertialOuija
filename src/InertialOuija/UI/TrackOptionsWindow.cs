extern alias GameScripts;

using GameScripts.Assets.Source.Enums;
using GameScripts.Assets.Source.GhostCars.GhostDatabases;
using GameScripts::Assets.Source.PlatformManagement;
using GameScripts.Assets.Source.SaveData;
using GameScripts.Assets.Source.Tools;
using InertialOuija.RollingStarts;
using InertialOuija.Utilities;
using System;
using UnityEngine;
using static InertialOuija.Configuration.ModConfig;

namespace InertialOuija.UI
{
	internal class TrackOptionsWindow : Window
	{
		private static readonly GUIContent[] PreferenceLabels =
		[
			new("Fastest", "Replace rolling start only if it's faster (Unmodded behavior)"),
			new("Newest", "Always replace rolling start"),
			new("Locked", "Never replace rolling start"),
		];



		protected override string Title => "TRACK OPTIONS";
		protected override Rect InitialPosition => new(750, 200, 200, 50);
		private static Rect _windowPosition;
		protected override Rect WindowPosition { get => _windowPosition; set => _windowPosition = value; }



		private readonly StringCache<float> _currentSpeedString = new(FormatSpeed);
		private readonly StringCache<float> _previousSpeedString = new(FormatSpeed);



		private static GhostKey? SelectedGhostKey
		{
			get
			{
				if (!GameData.CorePluginInitialized)
					return null;

				var track = CorePlugin.GameModeManager.GetRealCurrentTrack();
				var direction = CorePlugin.GameModeManager.TrackDirection;
				var car = GameData.FirstPlayerCar ?? Car.None;

				if (track == Track.None || car == Car.None)
					return null;

				return new(Character.None, track, car, direction);
			}
		}



		protected override void Awake()
		{
			base.Awake();

			if (SelectedGhostKey == null || !CreateBackup())
			{
				DestroyImmediate(gameObject);
				return;
			}

			Application.quitting += SaveDatabase;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			SaveDatabase();
			Application.quitting -= SaveDatabase;
		}

		protected override void DrawWindow()
		{
			if (SelectedGhostKey is not { } ghostKey)
			{
				Destroy(gameObject);
				return;
			}

			var currentRollingStart = RollingStartManager.GetRollingStart(ghostKey);
			var backupRollingStart = RollingStartManager.UndoRecord?.GhostKey == ghostKey ? RollingStartManager.UndoRecord : null;

			using (Styles.Row("Track:"))
				GUILayout.Label(ghostKey.Track.GetName(ghostKey.Direction));
			using (Styles.Row("Car:"))
				GUILayout.Label(ghostKey.Car.GetName());

			GUILayout.Label("Rolling Start", Styles.Heading);
			using (Styles.Enable(CorePlugin.GameModeManager.GetRealTrackInfo().IsCircuit))
			{
				using (Styles.Row("Mode:"))
					Config.Game.SetRollingStartPreference(ghostKey, (RollingStartPreference)GUILayout.Toolbar(
						(int)Config.Game.GetRollingStartPreference(ghostKey), PreferenceLabels, Styles.Toolbar));

				using (Styles.Row("Current:"))
				{
					if (currentRollingStart != null)
						GUILayout.Label(_currentSpeedString.GetString(currentRollingStart.Speed));
					else if (CorePlugin.PlayerGhosts.HasGhost(ghostKey))
						GUILayout.Label("Ghost fallback");
					else
						GUILayout.Label("None");
				}

				using (Styles.Row("Previous:"))
					GUILayout.Label(backupRollingStart != null ? _previousSpeedString.GetString(backupRollingStart.Speed) : "None");

				using (Styles.Row())
				{
					using (Styles.Enable(backupRollingStart != null))
					{
						if (GUILayout.Button("Undo", Styles.FixedButton))
							RollingStartManager.RestoreBackup(ghostKey);
					}

					using (Styles.Enable(currentRollingStart != null))
					{
						// PlayerGhosts are used as a fallback for missing rolling starts, so this is pointless unless they're disabled
						if (Config.Patches.DisablePlayerGhosts && GUILayout.Button("Delete", Styles.FixedButton))
							RollingStartManager.Delete(ghostKey);
					}
				}
			}

			Styles.Space();

			using (Styles.Horizontal())
			{
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Close", Styles.FixedButton))
					Close();
			}
		}

		private void SaveDatabase()
		{
			Config.Save();
			Log.Debug($"Dirty={RollingStartManager.Dirty} PerformingIO={SteamPlatformManager.Manager.PerformingIO} Qutting={SteamPlatformManager.Manager.Quitting} SteamInitialized={GameScripts.SteamManager.Initialized}");
			if (GameData.CorePluginInitialized && GameScripts.SteamManager.Initialized)
				CorePlugin.RollingStartDatabase.Save();
		}

		private static bool CreateBackup()
		{
			if (Config.Misc.RollingStartBackup < 1)
			{
				try
				{
					BackupUtility.CreateSteamBackup(SaveFileKeys.RollingStartKey);
				}
				catch (Exception ex)
				{
					Log.Error("Failed to create rolling start backup", ex);
					return false;
				}
				Config.Misc.RollingStartBackup = 1;
				Config.Save();
			}
			return true;
		}

		private static string FormatSpeed(float value) =>
			CorePlugin.SettingsManager.Units.IsDefault()
			? $"{ConversionTools.PerSecondToPerHour(ConversionTools.UnitsToMiles(value)):0.0} MPH"
			: $"{ConversionTools.PerSecondToPerHour(ConversionTools.UnitsToKm(value)):0.0} KPH";
	}
}
