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

		if (Input.GetKeyDown(KeyCode.F2))
			UIController.ToggleGhostSelectionWindow();

		if (Input.GetKeyDown(KeyCode.F3))
		{
			if (CtrlHeld() && ShiftHeld())
				UIController.ToggleDangerousOptionsWindow();
			else
				UIController.ToggleOptionsWindow();
		}

		if (Input.GetKeyDown(KeyCode.F4))
			UIController.ToggleTrackOptionsWindow();

		if (Input.GetKeyDown(KeyCode.F11))
			Components.HudControl.ToggleHud();

#if DEBUG
		if (Input.GetKeyDown(KeyCode.F7))
			LoadUnityExplorer();
#endif

		static bool CtrlHeld() => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
		static bool ShiftHeld() => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		//static bool AltHeld() => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
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
