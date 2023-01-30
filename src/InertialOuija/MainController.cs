using System.Collections;
using System.Threading.Tasks;
using InertialOuija.Ghosts;
using UnityEngine;

namespace InertialOuija;

internal static class MainController
{
	private static GameObject _rootObject;
	private static CoroutineComponent _coroutineComponent;



	public static void Initialize()
	{
		if (_rootObject)
			return;

		Log.Debug("Initialize", nameof(MainController));

		_rootObject = new GameObject("InertialOuija") { hideFlags = HideFlags.HideAndDontSave };

		AddGlobalComponent<HotkeyComponent>();

		Task.Run(() => ExternalGhostManager.RefreshDatabase()).LogFailure();
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

	private class CoroutineComponent : MonoBehaviour
	{ }
}
