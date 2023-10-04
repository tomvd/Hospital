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
            HospitalMapComponent hospital;
            if (sleeper.IsPatient(out hospital))
            {
                if (sleeper.health.surgeryBills.Count > 0)
                {
                    // surgery patient - give bed with highest surgerysuccesschancefactor
                    Building_Bed newBed = sleeper.Map.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>().Where(
                            bed => bed.Medical
                                   && RestUtility.IsValidBedFor(bed, sleeper, traveler, checkSocialProperness: false))
                        .OrderByDescending(
                            bed => bed.GetStatValue(StatDefOf.SurgerySuccessChanceFactor)
                        ).FirstOrFallback(__result);
                     __result = newBed;
                }
                else
                {
                    // normal patient - surgerysuccesschancefactor does not matter, so give them the worst beds first
                    Building_Bed newBed = sleeper.Map.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>().Where(
                            bed => bed.Medical
                                   && RestUtility.IsValidBedFor(bed, sleeper, traveler, checkSocialProperness: false))
                        .OrderBy(
                            bed => bed.GetStatValue(StatDefOf.SurgerySuccessChanceFactor)
                        ).FirstOrFallback(__result);
                    __result = newBed;                    
                }
            }
        }
    }
}