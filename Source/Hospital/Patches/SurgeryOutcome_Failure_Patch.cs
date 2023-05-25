using System.Collections.Generic;
using HarmonyLib;
using Hospital.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Hospital.Patches;

public class SurgeryOutcome_Failure_Patch
{
    /// <summary>
    /// Handle a surgery that failed
    /// </summary>
    [HarmonyPatch(typeof(SurgeryOutcome_Failure), "Apply")]
    public static class Apply
    {
        [HarmonyPrefix]
        public static void Prefix(Pawn patient)
        {
            if (patient.IsPatient()) patient.Map.GetComponent<HospitalMapComponent>().SurgeryFailed(patient);
        }
    }
}