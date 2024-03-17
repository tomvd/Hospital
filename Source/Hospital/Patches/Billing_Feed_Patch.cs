using HarmonyLib;
using Hospital.Utilities;
using RimWorld;
using UnityEngine;

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
            (x) =>
            {
                HospitalMapComponent hospital;
                if (__instance.Deliveree.IsPatient(out hospital))
                {
                    __instance.Deliveree.AddToBill(hospital, Mathf.Clamp(__instance.Food.MarketValue * 1.1f, 0, 70));
                }
            });
        }
    }
}