using System.Collections.Generic;
using HarmonyLib;
using Hospital.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Hospital.Patches;

public class PatientDied_Patch
{
    /// <summary>
    /// Handle a patient that died
    /// </summary>
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class Kill
    {
        [HarmonyPrefix]
        public static void Prefix(Pawn __instance)
        {
            HospitalMapComponent hospitalMapComponent;
            if (__instance.IsPatient(out hospitalMapComponent)) hospitalMapComponent.PatientDied(__instance);
        }
    }
}