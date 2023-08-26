using HarmonyLib;
using Hospital.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Hospital.Patches;

public class AllowOperationsPatch
{
    /// <summary>
    /// Allow operations on patients
    /// </summary>
    [HarmonyPatch(typeof(HealthCardUtility), nameof(HealthCardUtility.DrawHealthSummary))]
    public class AllowOperationsOnPatients
    {
        [HarmonyPrefix]
        public static void Prefix(Pawn pawn, ref bool allowOperations)
        {
            if (pawn.IsPatient(out _))
            {
                allowOperations = true;
            }
        }
    }
}