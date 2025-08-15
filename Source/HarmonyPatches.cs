using HarmonyLib;
using Verse;
using System.Reflection;

namespace EncounterBabyAnimals;

// Initialize harmony support.

[StaticConstructorOnStartup]
public class HarmonyPatches
{
    static HarmonyPatches()
    {
        var harmony = new Harmony("Yoann.BabiesAnimals");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}
