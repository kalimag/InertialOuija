extern alias GameScripts;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GameScripts.Assets.Source.CloudStorage;
using GameScripts.Assets.Source.GameData;
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



		public static bool IsFileSystemException(this Exception exception)
			=> exception is (IOException and not EndOfStreamException) or UnauthorizedAccessException or OutOfMemoryException;

		public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
		{
			var component = obj.GetComponent<T>();
			if (!component)
				component = obj.AddComponent<T>();
			return component;
		}

		public static T GetOrAddComponent<T>(this Component component) where T : Component
			=> component.gameObject.GetOrAddComponent<T>();

		public static T LogFailure<T>(this T task, [CallerMemberName] string memberName = null, [CallerArgumentExpression(nameof(task))] string expression = null) where T : Task
		{
			task.ContinueWith(task =>
			{
				if (task.IsFaulted)
					Log.Error($"Task {expression} in {memberName} failed", task.Exception.InnerExceptions.Count == 1 ? task.Exception.InnerException : task.Exception);
				else if (task.IsCanceled)
					Log.Debug($"Task {expression} in {memberName} was canceled");
			}, TaskContinuationOptions.NotOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
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
