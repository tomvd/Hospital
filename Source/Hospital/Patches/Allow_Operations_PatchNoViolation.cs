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
    
    [HarmonyPatch(typeof(Recipe_AdministerIngestible), nameof(Recipe_AdministerIngestible.IsViolationOnPawn))]
    public class AllowOperationsOnPatientNoViolation_Recipe_AdministerIngestible
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result, Pawn pawn)
        {
            if (!pawn.IsPatient()) return true;
            __result = false;
            return false;
        }
    }
    
    [HarmonyPatch(typeof(Recipe_InstallArtificialBodyPart), nameof(Recipe_InstallArtificialBodyPart.IsViolationOnPawn))]
    public class AllowOperationsOnPatientNoViolation_Recipe_InstallArtificialBodyPart
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