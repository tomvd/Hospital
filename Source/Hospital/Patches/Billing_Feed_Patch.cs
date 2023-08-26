using HarmonyLib;
using Hospital.Utilities;
using RimWorld;
using Verse;
using Verse.AI;

namespace Hospital.Patches;

public class Billing_Feed_Patch
{
    /// <summary>
    /// Bill patients for tending medicine
    /// </summary>
    [HarmonyPatch(typeof(JobDriver_FoodFeedPatient), nameof(JobDriver_FoodFeedPatient.MakeNewToils))]
    public class BillPatientsForFood
    {
        [HarmonyPostfix]
        public static void Postfix(JobDriver_FoodFeedPatient __instance)
        {
            __instance.AddFinishAction(
            () =>
            {
                HospitalMapComponent hospital;
                if (__instance.Deliveree.IsPatient(out hospital))
                {
                    __instance.Deliveree.AddToBill(hospital, __instance.Food.MarketValue * 1.1f);
                }
            });
        }
    }
}