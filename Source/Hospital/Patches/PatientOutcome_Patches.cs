using HarmonyLib;
using Hospital.Utilities;
using RimWorld;
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
            // not IsPatient: a patient dying while being carried has no Map and would be missed
            HospitalMapComponent.FindHospitalOf(__instance)?.PatientDied(__instance);
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
            // a patient carried off the map by a faction mate is not spawned, so don't rely on Map
            HospitalMapComponent hospital = HospitalMapComponent.FindHospitalOf(member);
            if (hospital != null)
            {
                hospital.PatientLeftTheMap(member);
                return false;
            }
            return true;
        }
    }

    /*
     * A captured patient that gets recruited joins the player faction. Clear any lingering
     * patient state, otherwise the new colonist keeps the "Patient" go-to-bed duty and just
     * wanders / refuses to work. Arrest keeps the pawn's original faction, so this only fires
     * on the recruit -> player-faction transition.
     */
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.SetFaction))]
    public static class SetFaction_ClearPatientState
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn __instance, Faction newFaction)
        {
            if (newFaction != Faction.OfPlayer) return;
            __instance?.MapHeld?.GetComponent<HospitalMapComponent>()?.StopBeingPatient(__instance);
        }
    }
}