extern alias GameScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GameScripts.Assets.Source.UI;
using InertialOuija.Components;
using UnityEngine;

namespace InertialOuija;

internal class GameAssets
{
	private static readonly Lazy<Dictionary<string, HudPrefabInfo>> HudPrefabInfos = new(
		static () => Resources.FindObjectsOfTypeAll<HudPrefabInfo>()
			.Where(info => info.gameObject.scene.buildIndex == -1)
			.ToDictionary(info => info.name),
		LazyThreadSafetyMode.PublicationOnly);

	public static HudPrefabInfo GetHudPrefabInfo(string name) => HudPrefabInfos.Value[name];

	public static T CreateTimeHud<T>(Transform parent, int? layoutIndex) where T : RivalTimeHud
	{
		var prefab = GetHudPrefabInfo("TimeAttackHud").RivalRoot.gameObject;
		var instance = UnityEngine.Object.Instantiate(prefab, parent);
		instance.GetComponent<RectTransform>().IntegrateInLayout(layoutIndex);
		return instance.AddComponent<T>();
	}
}
