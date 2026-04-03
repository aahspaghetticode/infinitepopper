using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

[BepInPlugin("com.h3nw.infinitepopper", "Infinite Party Popper", "1.0.0")]
public class InfinitePopperPlugin : BaseUnityPlugin
{
    private static ManualLogSource Log;
    private readonly Harmony harmony = new Harmony("com.h3nw.infinitepopper");

    public static ConfigEntry<bool> ModEnabled { get; private set; }

    private void Awake()
    {
        Log = Logger;

        ModEnabled = Config.Bind(
            "General",           // Section
            "Enabled",           // Key
            true,                // Default value
            "Enable infinite party popper uses." // Description
        );

        harmony.PatchAll();
        Logger.LogInfo("Infinite Party Popper loaded!");
    }
}

[HarmonyPatch(typeof(PartyPopper), "Update")]
public class PartyPopperPatch
{
    static void Postfix(PartyPopper __instance,
                        ref bool ___wasUsedOnConfig,
                        ref bool ___wasConfig)
    {
        // Respect the config toggle
        if (!InfinitePopperPlugin.ModEnabled.Value) return;

        var usedEntryField = AccessTools.Field(typeof(PartyPopper), "usedEntry");
        var stashEntryField = AccessTools.Field(typeof(PartyPopper), "stashAbleEntry");

        if (usedEntryField == null || stashEntryField == null) return;

        var usedEntry = (OnOffEntry)usedEntryField.GetValue(__instance);
        var stashEntry = (StashAbleEntry)stashEntryField.GetValue(__instance);

        if (usedEntry == null || stashEntry == null) return;

        if (usedEntry.on && ___wasUsedOnConfig)
        {
            usedEntry.on = false;
            usedEntry.SetDirty();

            stashEntry.isStashAble = true;
            stashEntry.SetDirty();

            ___wasUsedOnConfig = false;

            var chargesGOField = AccessTools.Field(typeof(PartyPopper), "chargesLeftGO");
            var go = chargesGOField?.GetValue(__instance) as GameObject;
            go?.SetActive(true);
        }
    }
}
