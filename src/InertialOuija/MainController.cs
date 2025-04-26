using System;
using System.Collections;
using System.Threading;
using InertialOuija.Ghosts;
using InertialOuija.UI;
using UnityEngine;

namespace InertialOuija;

internal static class MainController
{
	private static GameObject _rootObject;
	private static CoroutineComponent _coroutineComponent;
	private static SynchronizationContext _synchronizationContext;


	public static void Initialize()
	{
		if (_rootObject)
			return;

		_synchronizationContext = SynchronizationContext.Current;

		Log.Debug("Initialize", nameof(MainController));

		_rootObject = CreatePersistentObject("InertialOuija");

		AddGlobalComponent<HotkeyComponent>();
		AddGlobalComponent<VersionLabel>();

		ExternalGhostManager.Initialize();
	}

	public static GameObject CreatePersistentObject(string name)
	{
		var obj = new GameObject(name) { hideFlags = HideFlags.HideAndDontSave };
		if (_rootObject)
			obj.transform.SetParent(_rootObject.transform);
		return obj;
	}

	public static T CreatePersistentObject<T>(string name)
		where T : Component
	{
		var obj = CreatePersistentObject(name);
		return obj.AddComponent<T>();
	}

	public static T AddGlobalComponent<T>() where T : Component
	{
		Initialize();
		return _rootObject.AddComponent<T>();
	}

	public static Coroutine StartCoroutine(IEnumerator routine)
	{
		if (!_coroutineComponent)
			_coroutineComponent = AddGlobalComponent<CoroutineComponent>();
		return _coroutineComponent.StartCoroutine(routine);
	}

	public static bool TrySendMainThread(Action action)
	{
		if (_synchronizationContext != null)
		{
			_synchronizationContext.Send(static state => ((Action)state)(), action);
			return true;
		}
		else
		{
			return false;
		}
	}

	public static bool TryPostMainThread(Action action)
	{
		if (_synchronizationContext != null)
		{
			_synchronizationContext.Post(static state => ((Action)state)(), action);
			return true;
		}
		else
		{
			return false;
		}
	}

	private class CoroutineComponent : MonoBehaviour
	{ }
}
