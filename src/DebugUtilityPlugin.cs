﻿using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DebugUtilityMod
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class DebugUtilityPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> activateMod;


        public static ConfigEntry<float> gametimerMult;
        public static ConfigEntry<float> XPmult;
        public static ConfigEntry<int> maxPlayerLevel;
        public static ConfigEntry<bool> hasXPPatch;
        public static ConfigEntry<bool> hasInvincibility;
        public static ConfigEntry<bool> hasFastGame;
        public static ConfigEntry<bool> hasInfiniteReroll;
        public static ConfigEntry<bool> hasGunPatch;
        public static ConfigEntry<bool> hasWeakBossesAndElites;

        private static Dictionary<string, ConfigEntry<bool>> _configEntries;

        private static bool _enabledThisSession = false;

        private MethodInfo _registerMethod = null;
        private void RegisterWithReflection<T>(ConfigEntry<T> configEntry, List<T> acceptableValues = null)
        {
            if(_registerMethod == null)
            {
                var modOptionsType = Traverse.CreateWithType("MTDUI.ModOptions").GetValue() as Type;
                _registerMethod = modOptionsType.GetMethod("Register", BindingFlags.Public | BindingFlags.Static);
            }

            _registerMethod.MakeGenericMethod(new Type[] { typeof(T) }).Invoke(null, new object[]{ configEntry, acceptableValues });
        }

        // moved to Start instead of Awake so we can use Chainloader after it's fully loaded
        // this can be reverted once we don't need to use reflection for registration
        public void Start()
        {
            activateMod = Config.Bind("_General", "Activation", true, "If false, the mod does not load");
            hasXPPatch = Config.Bind("XP Patch", "XP Patch activation", false, "Set to True to activate XP Patch");
            XPmult = Config.Bind("XP Patch", "XP multiplier", 1f, "Amount of multiplication bonus applied to XP pickup, 1 is baseXP");
            maxPlayerLevel = Config.Bind("XP Patch", "Max level reachable", 100, "Level after which the player stop receiving XP");

            hasFastGame = Config.Bind("Fast GameTimer", "FastGame activation", false, "Set to True to activate faster game");
            gametimerMult = Config.Bind("Fast GameTimer", "GameTimer speed", 2f, "Increase the speed of the game; x is baseTime/x, 1 is baseTime");

            hasInvincibility = Config.Bind("Invincibility", "Player Invincibility", false, "If active, the player cannot take damage");

            hasInfiniteReroll = Config.Bind("Reroll", "Infinite Reroll", false, "If active, every character can reroll indefinitly");

            hasGunPatch = Config.Bind("Gun", "Infinite Ammo", false, "If active, infinite ammo");
            hasWeakBossesAndElites = Config.Bind("Enemy", "Weak Bosses and Elite", false, "If active, Bosses and Elite have 100 HP");

            var mtdui = Chainloader.PluginInfos.FirstOrDefault(x => x.Key == "dev.bobbie.20mtd.mtdui");
            if(mtdui.Value != null)
            {

                try
                {
                    // This bit is temporary
                    // Since nexus has no proper dependency resolution, I've opted to use reflection so the mod still works without MTDUI
                    // once we migrate from nexus to thunderstore, the chainloader check and reflection bit can be removed
                    // and MTDUI can explicitly be added as a dependency
                    RegisterWithReflection(hasXPPatch);
                    RegisterWithReflection(XPmult, new List<float>() { 1f, 2f, 5f, 10f, 100f });
                    RegisterWithReflection(maxPlayerLevel, new List<int>() { 50, 100, 200, 500, 1000 });
                    RegisterWithReflection(hasFastGame);
                    RegisterWithReflection(gametimerMult, new List<float>() { 0.5f, 1f, 2f, 5f, 10f, 100f });
                    RegisterWithReflection(hasInvincibility);
                    RegisterWithReflection(hasInfiniteReroll);
                    RegisterWithReflection(hasGunPatch);
                    RegisterWithReflection(hasWeakBossesAndElites);
                    /*
                    MTDUI.ModOptions.Register(hasXPPatch);
                    MTDUI.ModOptions.Register(XPmult, new List<float>() { 1f, 2f, 5f, 10f, 100f });
                    MTDUI.ModOptions.Register(maxPlayerLevel, new List<int>() { 50, 100, 200, 500, 1000 });
                    MTDUI.ModOptions.Register(hasFastGame);
                    MTDUI.ModOptions.Register(gametimerMult, new List<float>() { 0.5f, 1f, 2f, 5f, 10f, 100f });
                    MTDUI.ModOptions.Register(hasInvincibility);
                    MTDUI.ModOptions.Register(hasInfiniteReroll);
                    MTDUI.ModOptions.Register(hasGunPatch);
                    MTDUI.ModOptions.Register(hasWeakBossesAndElites);
                    */
                }
                catch (Exception ex) { 
                    Logger.LogError(ex);
                }
            }

            _configEntries = new Dictionary<string, ConfigEntry<bool>>()
            {
                { "Invincibility", hasInvincibility },
                { "GunPatch", hasGunPatch },
                { "Infinite Reroll", hasInfiniteReroll },
                { "FastXP", hasXPPatch },
                { "FastGame", hasFastGame },
                { "Weak Bosses & Elites", hasWeakBossesAndElites }
            };

            try
            {
                Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            }
            catch
            {
                Logger.LogError($"{PluginInfo.PLUGIN_GUID} failed to patch methods.");
            }

            if (!activateMod.Value)
            {
                Logger.LogInfo("<Inactive>");
                return;
            }

            foreach(var configEntry in _configEntries)
            {
                if (configEntry.Value == hasXPPatch)
                {
                    Logger.LogInfo(configEntry.Value.Value ? $"<Active> XPPatch     XP = {XPmult.Value}*baseXP  MaxLevel = {maxPlayerLevel.Value}" : "<Inactive> XPPatch");
                }
                else if (configEntry.Value == hasFastGame)
                {
                    Logger.LogInfo(configEntry.Value.Value ? $"<Active> FastGame    duration = baseTime/{gametimerMult.Value}" : "<Inactive> FastGame");
                }
                else
                {
                    Logger.LogInfo($"{(configEntry.Value.Value ? "<Active>" : "<Inactive>")} {configEntry.Key}");
                }
            }
        }
        
        public static bool PatchEnabled(ConfigEntry<bool> configEntry)
        {
            if (activateMod.Value && configEntry.Value)
            {
                _enabledThisSession = true;
                return true;
            }
            else return false;
        }

        public static bool ProgressionAllowed()
        {
            // nounlocks/nosoulgain should always be active when anything is active, to avoid cheating
            // progression should be blanket blocked if anything has been enabled this session
            // this could be made smarter by checking per-run, but for now it's safe to err on the side of caution
            if (_enabledThisSession) return !_enabledThisSession;

            bool anyPatchesEnabled = false;
            foreach(var patch in _configEntries)
            {
                if(patch.Value.Value) anyPatchesEnabled = true;
            }

            if (anyPatchesEnabled && activateMod.Value)
            {
                _enabledThisSession = true;
                return false;
            }

            return true;
        }
    }
}
