using System;
using System.Reflection;
using InertialOuija.UI;
using UnityEngine;

namespace InertialOuija;

internal class HotkeyComponent : MonoBehaviour
{
	private void Update()
	{
		if (!Input.anyKeyDown)
			return;

		//bool alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
		//bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
		//bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

		if (Input.GetKeyDown(KeyCode.F2))
			UIController.ToggleGhostSelectionWindow();

#if DEBUG
		if (Input.GetKeyDown(KeyCode.F7))
			LoadUnityExplorer();

		if (Input.GetKeyDown(KeyCode.F12))
		{
			Log.Debug($"{Application.runInBackground}={Application.runInBackground}");
			Application.runInBackground = false;
		}
#endif
	}

#if DEBUG
	private static bool _unityExplorerLoaded;
	private static void LoadUnityExplorer()
	{
		if (_unityExplorerLoaded)
			return;
		_unityExplorerLoaded = true;
		try
		{
			UIController.IncrementCursorUsers();
			Assembly.Load("UnityExplorer.STANDALONE.Mono")
				?.GetType("UnityExplorer.ExplorerStandalone")
				?.GetMethod("CreateInstance", new Type[] { })
				?.Invoke(null, null);
		}
		catch (Exception ex)
		{
			Log.Error("Failed to load UnityExplorer", ex);
		}
	}
#endif
}
