using HarmonyLib;
using RimWorld;
using Verse;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace EncounterBabyAnimals;

// Prevent baby animals from spawning as caravan pack animals.

// A small catch here is that PawnGenerator is asked to create only adult pawns as caravan carriers,
// but the check does not actually work. Fixing that would prevent other places from generating baby
// animals, so a simpler way to implement this is to explicitly check again when generating caravan
// carries using a check that works properly.

// Set a flag when generating caravan carriers.
[HarmonyPatch(typeof(PawnGroupKindWorker_Trader))]
public static class PawnGroupKindWorker_Trader_Patch
{
    public static bool inGenerateCarriers = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GenerateCarriers))]
    public static void GenerateCarriers()
    {
        inGenerateCarriers = true;
    }

    [HarmonyFinalizer]
    [HarmonyPatch(nameof(GenerateCarriers))]
    public static void GenerateCarriersFinalizer()
    {
        inGenerateCarriers = false;
    }
}

[HarmonyPatch]
public static class PawnGenerator_Patch
{
    // The function to patch is an internal function that has random parts in its name.
    // To be robust when those random parts change, find the function by checking the list of functions.
    [HarmonyTargetMethods]
    private static IEnumerable< MethodBase > TargetMethod()
    {
        Type type = typeof( PawnGenerator );
        foreach( string methodName in AccessTools.GetMethodNames( type ))
        {
            if( methodName.Contains( "GenerateRandomAge" ) && methodName.Contains( "AgeAllowed" ))
            {
                yield return AccessTools.Method( type, methodName );
                yield break;
            }
        }
        Log.Error( "Could not find PawnGenerator.GenerateRandomAge() nested AgeAllowed() function" );
    }

    // If generating caravan carriers, do not allow pawns younger than minimal adult age (this check works,
    // unlike checking developmental stage).
    [HarmonyPostfix]
    static bool GenerateRandomAge_AgeAllowed(bool result, Pawn p, float y)
    {
        if( result && PawnGroupKindWorker_Trader_Patch.inGenerateCarriers && y < p.ageTracker?.AdultMinAge )
            return XmlExtensions.SettingsManager.GetSetting( "Yoann.BabiesAnimals", "BabyCarriers" ) == "True";
        return result;
    }
}
