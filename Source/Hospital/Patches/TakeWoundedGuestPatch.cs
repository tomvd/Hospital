using Hospital.Utilities;
using RimWorld;
using Verse;

namespace Hospital.Patches;
using HarmonyLib;

public class TakeWoundedGuestPatch
{
    // do not allow visitors to take patients out of the hospital
    [HarmonyPatch(typeof(KidnapAIUtility), nameof(KidnapAIUtility.ReachableWoundedGuest))]
    public class HasJobOnThing_Patch
    {
        public static void Postfix(ref Pawn __result)
        {
            if (__result.IsPatient(out _))
            {
                __result = null;
            }
        }
    }
}