using System;
using UnityEngine;

namespace InertialOuija.UI;

internal abstract class Window : MonoBehaviour
{
	private static readonly GUIContent _tempContent = new();

	private static readonly Vector2 TooltipMouseOffset = new Vector2(10, 10);
	private static readonly GUI.WindowFunction _tooltipWindowFunction = DrawTooltipWindow;
	private static string _currentTooltip;
	private static string _queuedTooltip;

	private const int ToolTipId = 0xEE0000;
	private static int _nextId = ToolTipId + 1;



	private GUI.WindowFunction _windowFunction;
	private int _id;
	private bool _newWindow;


	protected abstract Rect WindowPosition { get; set; }
	protected abstract Rect InitialPosition { get; }
	protected abstract string Title { get; }
	protected GUILayoutOption[] WindowLayout { get; set; } = new[] { GUILayout.ExpandHeight(true) };
	protected virtual bool StayOpen => false;



	protected virtual void Awake()
	{
		_id = checked(_nextId++);

		_windowFunction = DrawWindow;
		if (WindowPosition.x > Screen.width - 50 || WindowPosition.y > Screen.height - 50 || WindowPosition.x < -50 || WindowPosition.y < -5)
			WindowPosition = Rect.zero;

		if (StayOpen)
			DontDestroyOnLoad(this);
	}

	protected void OnEnable()
	{
		_newWindow = true;
		UIController.IncrementCursorUsers();
	}

	protected void OnDisable()
	{
		UIController.DecrementCursorUsers();
	}

	protected virtual void OnDestroy()
	{ }

	protected virtual void OnGUI()
	{
		Styles.ApplySkin();

		Styles.Scale();

		if (WindowPosition == Rect.zero)
			WindowPosition = InitialPosition;

		WindowPosition = GUILayout.Window((int)_id, WindowPosition, _windowFunction, Title, WindowLayout);

		if (_newWindow)
		{
			_newWindow = false;
			GUI.FocusWindow((int)_id);
		}

		// this seems very convoluted, but appears to be the most practical solution to the order of events around windows and tooltips
		if (!String.IsNullOrEmpty(_queuedTooltip))
		{
			_currentTooltip = _queuedTooltip;
			_queuedTooltip = null;
			var size = Styles.Tooltip.CalcSize(TempContent(_currentTooltip, null));
			GUILayout.Window(ToolTipId,
				new Rect(Event.current.mousePosition + TooltipMouseOffset, size),
				_tooltipWindowFunction,
				"",
				GUIStyle.none);
			GUI.BringWindowToFront(ToolTipId);
		}
	}

	private void DrawWindow(int id)
	{
		DrawWindow();

		if (Cursor.lockState != CursorLockMode.Locked)
			GUI.DragWindow();

		_queuedTooltip = GUI.tooltip;
	}

	private static void DrawTooltipWindow(int id)
	{
		GUILayout.Label(_currentTooltip, Styles.Tooltip);
	}

	protected abstract void DrawWindow();

	public void Close()
	{
		if (this && this.gameObject)
			Destroy(gameObject);
	}

	protected static GUIContent TempContent(string text, string tooltip)
	{
		_tempContent.text = text;
		_tempContent.tooltip = tooltip;
		return _tempContent;
	}

}
