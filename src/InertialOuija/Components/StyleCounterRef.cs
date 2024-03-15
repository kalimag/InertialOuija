extern alias GameScripts;

using GameScripts.Assets.Source.Gameplay.Timing;
using UnityEngine;

namespace InertialOuija.Components;

internal class StyleCounterRef : MonoBehaviour
{
	public StyleCounter StyleCounter { get; set; }
}
