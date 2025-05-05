extern alias GameScripts;

using GameScripts.Assets.Source.Messaging;
using GameScripts.Assets.Source.Messaging.Messages;
using GameScripts.Assets.Source.Tools;
using InertialOuija.Ghosts;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static InertialOuija.Configuration.ModConfig;

namespace InertialOuija.UI.Components;
internal class ChosenGhostDisplay : BaseComponent, IReceiveMessages<RaceStartMessage>, IReceiveMessages<RestartEventMessage>, IReceiveMessages<QuitEventMessage>
{
	private static readonly Vector2 Margin = new(50, 50);
	private const float ItemHeight = 64;

	private static ChosenGhostDisplay _instance;
	public static ChosenGhostDisplay Instance => _instance ?? (_instance = MainController.AddGlobalComponent<ChosenGhostDisplay>());

	private readonly List<Item> _items = new();
	private string _additionalGhosts;

	public void SetGhosts(IReadOnlyCollection<ExternalGhostInfo> ghosts)
	{
		int maxCount = (int)((Screen.safeArea.height - (Margin.y * 2)) / ItemHeight);

		_items.Clear();
		foreach (var ghost in ghosts.Take(maxCount))
			_items.Add(new(ghost.Time.ToString(true, " : "), ghost.Username, ghost.Car.GetName(), ghost.Date?.LocalDateTime.ToShortDateString() ?? ""));

		_additionalGhosts = ghosts.Count > maxCount ? $"And {ghosts.Count - maxCount} more" : null;

		enabled = _items.Count > 0 && Config.UI.ShowChosenGhosts;
	}

	void OnGUI()
	{
		Styles.ApplySkin();
		Styles.Scale(Margin.x, Margin.y);

		var rect = new Rect(Margin.x, Margin.y, 500, Screen.safeArea.height - (Margin.y * 2));
		GUILayout.BeginArea(rect);

		GUILayout.FlexibleSpace();

		foreach (var item in _items)
		{
			using (Styles.Vertical(Styles.ChosenGhost))
			{
				using (Styles.Horizontal())
				{
					GUILayout.Label(item.Time, Styles.ChosenGhostScoreLabel);
					GUILayout.Label(item.Username, Styles.ChosenGhostNameLabel);
				}
				using (Styles.Horizontal())
				{
					GUILayout.Label(GUIContent.none, Styles.ChosenGhostScoreLabel);
					GUILayout.Label(item.Car, Styles.ChosenGhostMiscLabel);
					GUILayout.FlexibleSpace();
					GUILayout.Label(item.Date, Styles.ChosenGhostMiscLabel);
				}
			}
		}

		if (_additionalGhosts != null)
			GUILayout.Label(_additionalGhosts, Styles.ChosenGhostMessage);

		GUILayout.EndArea();
	}

	void IReceiveMessages<RaceStartMessage>.HandleMessage(RaceStartMessage message) => enabled = false;
	void IReceiveMessages<RestartEventMessage>.HandleMessage(RestartEventMessage message) => enabled = _items.Count > 0 && Config.UI.ShowChosenGhosts;
	void IReceiveMessages<QuitEventMessage>.HandleMessage(QuitEventMessage message)
	{
		_items.Clear();
		enabled = false;
	}

	private record struct Item(string Time, string Username, string Car, string Date);
}
