using System;
using System.Linq;
using HarmonyLib;
using Hospital.Utilities;
using RimWorld;
using Verse;

namespace Hospital.Patches;

public class Patients_BedFinder_Patch
{
    /// <summary>
    /// Assigned the best medial beds to surgery patients
    /// </summary>
    [HarmonyPatch(typeof(RestUtility), nameof(RestUtility.FindBedFor),  typeof(Pawn),typeof(Pawn),typeof(bool),typeof(bool),typeof(GuestStatus?))]

    public class FindBedPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref Building_Bed __result, Pawn sleeper, Pawn traveler)
        {
            if (sleeper.IsPatient(out _) && !sleeper.InBed())
            {
                //Find any hospital bed
                __result = sleeper.Map.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>().Where(
                        bed => bed.Medical
                                && bed.GetComp<CompHospitalBed>() != null
                                && bed.GetComp<CompHospitalBed>().Hospital
                                && RestUtility.IsValidBedFor(bed, sleeper, traveler, checkSocialProperness: false))
                    .FirstOrFallback(__result);
                // Try to find hospital bed for a patient that does not require surgery
                if (sleeper.health.surgeryBills.Count <= 0) {
                    
                    __result = sleeper.Map.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>().Where(
                        bed => bed.Medical
                                && bed.GetComp<CompHospitalBed>() != null
                                && bed.GetComp<CompHospitalBed>().Hospital
                                && !bed.GetComp<CompHospitalBed>().Surgery 
                                && RestUtility.IsValidBedFor(bed, sleeper, traveler, checkSocialProperness: false))
                    .FirstOrFallback(__result);
                }
                // Try to find hospital bed for a patient that does surgery
                if (sleeper.health.surgeryBills.Count > 0) {

                    __result = sleeper.Map.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>().Where(
                        bed => bed.Medical
                                && bed.GetComp<CompHospitalBed>() != null
                                && bed.GetComp<CompHospitalBed>().Hospital
                                && bed.GetComp<CompHospitalBed>().Surgery 
                                && RestUtility.IsValidBedFor(bed, sleeper, traveler, checkSocialProperness: false))
                    .FirstOrFallback(__result);
                }
            }
        }
    }
}