extern alias GameScripts;
using System;
using System.Linq;
using GameScripts.Assets.Source.UI.Menus;
using HarmonyLib;
using Rewired;

namespace InertialOuija.Patches;

/// <summary>
/// Create mapped action for reversing, which is missing from default keyboard mapping.
/// Fixes broken keyboard binding settings.
/// </summary>
[HarmonyPatch]
internal class AllowKeyboardReverseBindingPatches
{
	[HarmonyFinalizer, HarmonyPatch(typeof(RemapButton), nameof(RemapButton.UpdateContext))]
	static Exception RemapButton_UpdateContext(InputMapper.Context ____context, Exception __exception)
	{
		if (__exception != null)
			Log.Error("Exception in RemapButton.UpdateContext", __exception);

		if (__exception is not InvalidOperationException ||
			____context?.actionId != GameScripts.RewiredConsts.Action.Reverse ||
			____context.controllerMap?.controllerType != ControllerType.Keyboard ||
			____context.controllerMap.ElementMapsWithAction(____context.actionId).Any())
			return __exception;

		Log.Info("Creating input mapping for reverse action");
		____context.controllerMap.CreateElementMap(GameScripts.RewiredConsts.Action.Reverse, Pole.Positive, UnityEngine.KeyCode.None, ModifierKeyFlags.None, out var map);
		____context.actionElementMapToReplace = map;

		return null;
	}
}
