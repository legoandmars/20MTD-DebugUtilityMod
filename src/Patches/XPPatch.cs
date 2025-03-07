﻿using HarmonyLib;
using flanne.Core;
using flanne.Player;
using flanne;

namespace DebugUtilityMod
{
    [HarmonyPatch]
    static class XPPatch
    {
        static bool doGainXP = true;
        static int lvl = 1;

        [HarmonyPatch(typeof(PlayerXP), "Awake")]
        [HarmonyPostfix]
        static void PlayerXPAwake_postfix(ref StatMod ___xpMultiplier)
        {
            if (!DebugUtilityPlugin.PatchEnabled(DebugUtilityPlugin.hasXPPatch)) return;

            // Add multiplier to XP amount gain, value stored in config (v0.1)
            ___xpMultiplier.AddMultiplierBonus(DebugUtilityPlugin.XPmult.Value - 1);
            lvl = 1;
            doGainXP = true;
        }


        [HarmonyPatch(typeof(PlayerXP), "GainXP")]
        [HarmonyPrefix]
        static bool GainXP_prefix()
        {
            if (!DebugUtilityPlugin.PatchEnabled(DebugUtilityPlugin.hasXPPatch)) return true;

            //Skip XP gain if playerLVL >= maxLVL
            return doGainXP;
        }

        [HarmonyPatch(typeof(CombatState), "OnLevelUP")]
        [HarmonyPrefix]
        static void OnLevelUp_prefix()
        {
            if (!DebugUtilityPlugin.PatchEnabled(DebugUtilityPlugin.hasXPPatch)) return;

            lvl++;
            doGainXP = lvl < DebugUtilityPlugin.maxPlayerLevel.Value;
        }
    }
}
