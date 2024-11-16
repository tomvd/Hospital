using System.Collections.Generic;
using HarmonyLib;
using Hospital.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Hospital.Patches;

public class PatientOutcome_Patches
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
            if (__instance.IsPatient(out var hospital)) hospital.PatientDied(__instance);
        }
    }
    
    /*
     * from a  failed surgery you should not receive money
     */
    [HarmonyPatch(typeof(SurgeryOutcome_Failure), "TryGainBotchedSurgeryThought")]
    public static class TryGainBotchedSurgeryThought
    {
        [HarmonyPrefix]
        public static void Prefix(Pawn patient, Pawn surgeon)
        {
            if (patient.IsPatient(out var hospital)) hospital.SurgeryFailed(patient);
        }
    }
    
    /*
     * no extra goodwill when running a hospital - the goodwill already has been given
     */
    
    [HarmonyPatch(typeof(Faction), "Notify_MemberExitedMap")]
    public static class Notify_MemberExitedMap
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn member, bool freed)
        {
            HospitalMapComponent hospital = member?.Map?.GetComponent<HospitalMapComponent>();
            if (hospital != null && hospital.GetPatientData(member, out var patientData))
            {
                hospital.PatientLeftTheMap(member);
                return false;
            }
            return true;
        }
    }        
    
}