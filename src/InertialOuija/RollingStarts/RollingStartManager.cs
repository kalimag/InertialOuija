extern alias GameScripts;

using GameScripts.Assets.Source.Enums;
using GameScripts.Assets.Source.GhostCars.GhostDatabases;
using GameScripts.Assets.Source.Tools;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace InertialOuija.RollingStarts;

public static class RollingStartManager
{
	private static readonly AccessTools.FieldRef<RollingStartDatabase, Dictionary<GhostKey, RollingStartRecord>> GetFastestLaps =
		AccessTools.FieldRefAccess<RollingStartDatabase, Dictionary<GhostKey, RollingStartRecord>>("_fastestLaps");
	private static readonly AccessTools.FieldRef<RollingStartDatabase, List<RollingStartRecord>> GetSaveRecords =
		AccessTools.FieldRefAccess<RollingStartDatabase, List<RollingStartRecord>>("_saveRecords");
	private static readonly AccessTools.FieldRef<RollingStartDatabase, bool> GetDirty =
		AccessTools.FieldRefAccess<RollingStartDatabase, bool>("_dirty");

	private static Dictionary<GhostKey, RollingStartRecord> FastestLaps => GetFastestLaps(CorePlugin.RollingStartDatabase);
	private static List<RollingStartRecord> SaveRecords => GetSaveRecords(CorePlugin.RollingStartDatabase);
	public static bool Dirty
	{
		get => GetDirty(CorePlugin.RollingStartDatabase);
		private set => GetDirty(CorePlugin.RollingStartDatabase) = value;
	}



	public static RollingStartRecord UndoRecord { get; internal set; }



	public static RollingStartRecord GetRollingStart(GhostKey ghostKey)
	{
		CorePlugin.RollingStartDatabase.Load();

		if (FastestLaps.TryGetValue(ghostKey, out var rollingStart))
			return rollingStart;
		else
			return null;
	}

	public static RollingStartRecord GetRollingStart(Track track, TrackDirection direction, Car car) => GetRollingStart(new(Character.None, track, car, direction));

	public static void SetRollingStart(RollingStartRecord record)
	{
		if (record == null)
			throw new ArgumentNullException(nameof(record));
		if (record.Recording == null)
			throw new ArgumentException(nameof(record));
		if (record.GhostKey.Character != Character.None)
			throw new ArgumentException($"RollingStartRecord GhostKey has character {record.GhostKey.Character}");

		var databaseRecord = GetRollingStart(record.GhostKey);
		if (databaseRecord != null)
		{
			UndoRecord = databaseRecord.Clone();
			databaseRecord.Speed = record.Speed;
			databaseRecord.Recording = record.Recording;
		}
		else
		{
			databaseRecord = record.Clone();
			FastestLaps.Add(databaseRecord.GhostKey, databaseRecord);
			SaveRecords.Add(databaseRecord);
		}
		Dirty = true;
	}

	internal static void Delete(GhostKey ghostKey)
	{
		var record = GetRollingStart(ghostKey);
		if (record != null)
		{
			UndoRecord = record;
			GetFastestLaps(CorePlugin.RollingStartDatabase).Remove(ghostKey);
			GetSaveRecords(CorePlugin.RollingStartDatabase).Remove(record);
			Dirty = true;
		}
	}

	internal static void RestoreBackup(GhostKey ghostKey)
	{
		if (UndoRecord?.GhostKey == ghostKey)
			SetRollingStart(UndoRecord);
	}
}
