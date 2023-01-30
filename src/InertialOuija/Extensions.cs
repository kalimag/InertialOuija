extern alias GameScripts;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GameScripts.Assets.Source.CloudStorage;
using GameScripts.Assets.Source.Enums;
using GameScripts.Assets.Source.GameData;
using GameScripts.Assets.Source.Localisation;
using GameScripts.Assets.Source.Tools;
using InertialOuija.Ghosts;
using UnityEngine;

namespace InertialOuija
{
	internal static class Extensions
	{

		public static string FriendlyString(this CompressionFriendlyGhostRecording recording)
			=> recording == null ? "<null>" : $"{{{recording.Track} {recording.Direction} {recording.Car} {recording.TotalTime}}}";
		public static string FriendlyString(this GameScripts.LeaderboardEntry entry)
			=> $"{{#{entry.Rank} {entry.Score} {entry.Username}}} [{entry.DataHandle.m_UGCHandle}]";
		public static string FriendlyString(this EventDetails ev)
			=> ev == null ? "<null>" : $"\"{{{ev.EventTitle.GetLocalisedString()}\" #{ev.EventId} {ev.EventPrefab.FriendlyString()}}}";
		public static string FriendlyString(this GameObject obj)
			=> obj == null ? "<null>" : $"{{{obj.name} #{obj.GetInstanceID()}}}";


		public static string GetInvariantString(this LocalisedString value)
			=> value?.GetLocalisedString(Language.English) ?? "";

		public static string GetName(this Track track, TrackDirection direction = TrackDirection.Forward)
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
			return name ?? $"{track} {direction}";
		}

		public static string GetName(this Car car)
		{
			string name = null;
			try
			{
				name = CorePlugin.CarDatabase.GetCarDetails(car)?.DisplayedName.GetInvariantString();
			}
			catch (NullReferenceException)
			{
				// CorePlugin not initialized
			}
			return name ?? car.ToString();
		}
		
		public static GhostLeaderboardType ToEnum(this GameScripts.LeaderboardType leaderboardType)
		{
			return leaderboardType switch
			{
				null => GhostLeaderboardType.None,
				{ Timed: true} => GhostLeaderboardType.Timed,
				{ Distance: true } => GhostLeaderboardType.Distance,
				{ Style: true } => GhostLeaderboardType.Style,
				_ => GhostLeaderboardType.Other,
			};
		}

		public static T LogFailure<T>(this T task, [CallerMemberName] string caller = null) where T : Task
		{
			task.ContinueWith(static (task, caller) =>
			{
				if (task.IsFaulted)
					Log.Error($"Task in {caller} failed", task.Exception);
				else if (task.IsCanceled)
					Log.Debug($"Task in {caller} was canceled");
			}, caller);
			return task;
		}

		public static CustomYieldInstruction AsYield(this Task task)
			=> new YieldTask(task);

		private class YieldTask : CustomYieldInstruction
		{
			private readonly Task _task;

			public override bool keepWaiting => !_task.IsCompleted;

			public YieldTask(Task task)
			{
				_task = task;
			}
		}
	}
}
