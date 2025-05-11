extern alias GameScripts;

using GameScripts.Assets.Source.Enums;
using GameScripts.TinyJSON;
using System.Collections.Generic;

namespace InertialOuija.Configuration;

internal class CustomizationConfig
{
	[Include] public Dictionary<Character, int> Palettes { get; set; } = [];
}
