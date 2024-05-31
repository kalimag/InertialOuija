extern alias GameScripts;

using System;
using System.Collections.Generic;
using GameScripts.Assets.Source.UI.Menus;
using HarmonyLib;
using UnityEngine;

namespace InertialOuija.Patches;

/// <summary>
/// Some UI elements for P2 car selection don't have required fields set,
/// causing NRE's and the menu becoming unresponsive when selecting certain cars.
/// Patch copies the missing values over from other buttons to fix this.
/// </summary>
[HarmonyPatch]
internal class FixSplitscreenCarSelectionPatches
{
	[HarmonyPostfix, HarmonyPatch(typeof(ManageUnlockedCars), nameof(ManageUnlockedCars.OnEnable))]
	static void ManageUnlockedCars_OnEnable(List<CharacterSelectButton> ___Buttons)
	{
		try
		{
			MenuNavigator eventMenu = null;
			Animator animator = null;

			foreach (var button in ___Buttons)
			{
				if (!eventMenu)
					eventMenu = button.EventMenu;
				if (!button.EventMenu)
				{
					Log.Debug($"Assigning missing EventMenu on {button.name} ({button.Character})");
					button.EventMenu = eventMenu;
				}

				var handler = button.GetComponent<SetAnimationTriggerButtonHandler>();
				if (handler)
				{
					if (!animator)
						animator = handler.Animator;
					if (!handler.Animator)
					{
						handler.Animator = animator;
						Log.Debug($"Assigning missing Animator on {handler.name} ({button.Character})");
					}
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex);
		}
	}
}
