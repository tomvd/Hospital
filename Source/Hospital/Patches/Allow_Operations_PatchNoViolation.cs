using HarmonyLib;
using Hospital.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Hospital.Patches;

public class AllowOperationsPatchNoViolation
{
    
    [HarmonyPatch(typeof(RecipeWorker), nameof(RecipeWorker.IsViolationOnPawn))]
    public class AllowOperationsOnPatientNoViolation
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result, Pawn pawn)
        {
            if (!pawn.IsPatient()) return true;
            __result = false;
            return false;
        }
    }
}