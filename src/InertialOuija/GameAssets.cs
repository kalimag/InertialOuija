extern alias GameScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GameScripts.Assets.Source.UI;
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
}
