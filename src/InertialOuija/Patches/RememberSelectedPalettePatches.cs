extern alias GameScripts;

using GameScripts.Assets.Source.CarModel;
using GameScripts.Assets.Source.Enums;
using GameScripts.Assets.Source.UI.Menus;
using HarmonyLib;
using System;
using static InertialOuija.Configuration.ModConfig;

namespace InertialOuija.Patches;

[HarmonyPatch]
internal class RememberSelectedPalettePatches
{
	[HarmonyPostfix, HarmonyPatch(typeof(CarPaletteToggle), nameof(CarPaletteToggle.Toggle))]
	static void CarPaletteToggle_Toggle()
	{
		try
		{
			Config.Customization.Palettes[MenuSelection.CurrentCharacter] = MenuSelection.ColourPaletteIndex;
			Config.Save();
		}
		catch (Exception ex)
		{
			Log.Error(ex);
		}
	}

	[HarmonyPostfix, HarmonyPatch(typeof(MenuSelection), nameof(MenuSelection.CurrentCharacter), MethodType.Setter)]
	static void MenuSelection_CurrentCharacter_set()
	{
		try
		{
			if (MenuSelection.CurrentCharacter != Character.None && Config.Customization.Palettes.TryGetValue(MenuSelection.CurrentCharacter, out int palette))
				MenuSelection.ColourPaletteIndex = palette;
		}
		catch (Exception ex)
		{
			Log.Error(ex);
		}
	}
}
