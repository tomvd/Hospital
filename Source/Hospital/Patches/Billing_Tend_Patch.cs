using HarmonyLib;
using Hospital.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Hospital.Patches;

public class Billing_Tend_Patch
{
    /// <summary>
    /// Bill patients for tending medicine
    /// </summary>
    [HarmonyPatch(typeof(TendUtility), nameof(TendUtility.DoTend))]
    public class BillPatientsForMedicine
    {
        [HarmonyPrefix]
        public static void Prefix(Pawn doctor, Pawn patient, Medicine medicine)
        {
            HospitalMapComponent hospitalMapComponent = null;
            if (patient.health.HasHediffsNeedingTend() && doctor != null && doctor.Faction == Faction.OfPlayer &&
                doctor != patient
                && medicine != null && patient.IsPatient(out hospitalMapComponent))
            {
                patient.AddToBill(hospitalMapComponent, Mathf.Clamp(medicine.MarketValue * 1.2f, 0, 70));
            }
        }
    }
}