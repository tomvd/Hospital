using System.Reflection;
using HarmonyLib;
using Hospital.Utilities;
using Verse;

namespace Hospital.Patches;

/*
 * Compatibility with Enemy Self Preservation (Continued).
 * ESP postfixes Pawn.PostApplyDamage and makes any non-player pawn in enough pain panic flee.
 * The wounds we give a patient on arrival go through that same postfix, so wounded patients
 * fled the moment they landed. Skip ESP for patients bcause they came here to be treated.
 */
[HarmonyPatch]
public static class EnemySelfPreservation_Patch
{
    private static MethodBase target;

    public static bool Prepare()
    {
        target ??= AccessTools.TypeByName("MUR_ESP.Pawn_PostApplyDamage")?.GetMethod("Postfix", AccessTools.all);
        return target != null;
    }

    public static MethodBase TargetMethod() => target;

    // __0 is the damaged pawn (ESP's own "ref Pawn __instance" parameter)
    [HarmonyPrefix]
    public static bool Prefix(Pawn __0)
    {
        if (__0 == null) return true;
        if (__0 == PatientUtility.PawnBeingInjured) return false; // not registered as patient yet
        return !__0.IsPatient(out _);
    }
}
