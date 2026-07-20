using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hospital.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Hospital.Patches;












public class Billing_Surgery_Patch
{
    /// <summary>
    /// Bill patients for any completed medical bill (surgery, implant install, natural
    /// transplant, administered ingestible, ...).
    ///
    /// Previously this hooked three specific recipe worker classes
    /// (Recipe_InstallArtificialBodyPart / Recipe_InstallNaturalBodyPart /
    /// Recipe_AdministerIngestible), so any modded surgery or implant that uses its own
    /// worker class was never billed and the patient paid 0 silver. Hooking the generic
    /// bill-completion callback instead bills every medical bill regardless of the recipe's
    /// worker class, which covers modded content (Integrated Implants, More Injuries, ...).
    /// </summary>
    [HarmonyPatch(typeof(Bill), nameof(Bill.Notify_IterationCompleted))]
    public class BillPatientsForMedicalBill
    {
        // Capture the patient in the prefix: Notify_IterationCompleted may delete the bill from
        // its stack before returning, after which billStack.billGiver is no longer reachable.
        [HarmonyPrefix]
        public static void Prefix(Bill __instance, out Pawn __state)
        {
            __state = null;
            if (__instance is Bill_Medical && __instance.billStack?.billGiver is Pawn pawn)
            {
                __state = pawn;
            }
        }

        [HarmonyPostfix]
        public static void Postfix(Bill __instance, Pawn __state)
        {
            // BillForSurgery no-ops for non-patients, so a failed surgery (whose patient was
            // already removed from the roster) is naturally not billed.
            if (__state != null) BillingHelper.BillForSurgery(__state, __instance);
        }
    }

    /// <summary>
    /// Helper class to avoid code duplication in surgery billing
    /// </summary>
    public static class BillingHelper
    {
        public static void BillForSurgery(Pawn pawn, Bill bill)
        {
            if (pawn.IsPatient(out var hospital) && hospital.GetPatientData(pawn, out var patientData))
            {
                if (bill?.recipe != null)
                {
                    float timeCost = 0;
                    if (bill.recipe.skillRequirements != null)
                    {
                        var medSkill = bill.recipe.skillRequirements
                            .Find(requirement => requirement.skill == SkillDefOf.Medicine);
                        timeCost = (bill.recipe.workAmount / 100f) * (medSkill?.minLevel ?? 0f);
                    }

                    float materialCost = 0;
                    if (bill.recipe.ingredients != null)
                    {
                        foreach (IngredientCount ingredientCount in bill.recipe.ingredients.Where(count => count.IsFixedIngredient))
                        {
                            materialCost += ingredientCount.FixedIngredient.BaseMarketValue;
                        }

                        List<IngredientCount> medicine = bill.recipe.ingredients
                            .FindAll(count => count.filter is { categories: not null } && count.filter.categories.Contains("Medicine"));
                        if (!medicine.Empty())
                        {
                            materialCost += medicine.First().count * ((int)pawn.playerSettings.medCare * 15.0f);
                        }
                    }

                    float surgeryBill = Mathf.Clamp(timeCost, 0, 100) + Mathf.Clamp(materialCost, 0, 3000);
                    hospital.AddSurgeryBill(pawn, surgeryBill);

                    patientData.HasPendingSurgeryBill = false;
                }
            }
        }
    }

    /// <summary>
    /// Clear pending surgery flag when bill is manually removed (note this method also is called after a surgery)
    /// </summary>
    [HarmonyPatch(typeof(BillStack), nameof(BillStack.Delete))]
    public class ClearPendingSurgeryOnBillRemoval
    {
        [HarmonyPrefix]
        public static void Prefix(BillStack __instance, Bill bill)
        {
            // Check if this is a surgery bill on a patient pawn
            if (bill is Bill_Medical && __instance.billGiver is Pawn pawn)
            {
                if (pawn.IsPatient(out var hospital) && hospital.GetPatientData(pawn, out var patientData))
                {
                    // Clear the pending flag so patient can be dismissed
                    patientData.HasPendingSurgeryBill = false;
                }
            }
        }
    }
}
