using UnityEngine;

namespace InertialOuija.UI;

internal static class UIController
{
	private static int _cursorUsers;



	private static GhostSelectionWindow _ghostSelectionWindow;
	public static void ToggleGhostSelectionWindow() => Toggle(ref _ghostSelectionWindow);

	private static OptionsWindow _ghostOptionsWindow;
	public static void ToggleOptionsWindow() => Toggle(ref _ghostOptionsWindow);



	private static void Toggle<T>(ref T component) where T : Component
	{
		if (!component)
			component = new GameObject().AddComponent<T>();
		else if (component is Window window)
			window.Close();
		else
			Object.Destroy(component.gameObject);
	}

	private static void CreateIfNotExists<T>(ref T component) where T : Component
	{
		if (!component)
			component = new GameObject().AddComponent<T>();
	}


	public static void IncrementCursorUsers()
	{
		_cursorUsers++;
		UpdateCursor();
	}

	public static void DecrementCursorUsers()
	{
		_cursorUsers--;
		if (_cursorUsers < 0)
		{
			_cursorUsers = 0;
			Log.Debug("_cursorUsers unbalanced");
		}
		UpdateCursor();
	}

	public static void UpdateCursor()
	{
		//Cursor.lockState = _cursorUsers > 0 ? CursorLockMode.None : CursorLockMode.Locked;
		Cursor.visible = _cursorUsers > 0;
	}
}
