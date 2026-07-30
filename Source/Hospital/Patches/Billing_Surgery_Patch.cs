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
            if (!pawn.IsPatient(out var hospital) || !hospital.GetPatientData(pawn, out var patientData)) return;
            if (bill?.recipe == null) return;

            RecipeDef recipe = bill.recipe;

            float timeCost = 0;
            if (recipe.skillRequirements != null)
            {
                var medSkill = recipe.skillRequirements
                    .Find(requirement => requirement.skill == SkillDefOf.Medicine);
                timeCost = (recipe.workAmount / 100f) * (medSkill?.minLevel ?? 0f);
            }

            float materialCost = 0;
            if (recipe.ingredients != null)
            {
                foreach (IngredientCount ingredientCount in recipe.ingredients)
                {
                    materialCost += IngredientCost(ingredientCount, pawn);
                }
            }

            float surgeryBill = Mathf.Clamp(timeCost, 0, 100) + Mathf.Clamp(materialCost, 0, 3000);

            // A completed surgery is never free. A recipe that asks for no medicine skill and
            // no priced ingredient - which happens with modded surgeries - used to bill 0, and
            // a patient who owes nothing walks out having cost us the operation for free.
            if (surgeryBill <= 0f) surgeryBill = Mathf.Clamp(recipe.workAmount / 100f, 10f, 100f);

            hospital.AddSurgeryBill(pawn, surgeryBill);

            patientData.HasPendingSurgeryBill = false;
        }

        /// <summary>
        /// Price one ingredient line of a recipe.
        ///
        /// Only fixed ingredients (a filter naming exactly one thingDef, e.g. a bionic arm) used
        /// to be counted, so any recipe whose ingredients are a choice - "any advanced component",
        /// modded material options - contributed nothing at all. Those are priced from the
        /// cheapest thing the recipe would accept, which is what a patient could reasonably be
        /// charged for. Counts are honoured too; previously a recipe eating 4 of something was
        /// billed as if it ate 1.
        /// </summary>
        private static float IngredientCost(IngredientCount ingredientCount, Pawn pawn)
        {
            if (ingredientCount?.filter == null) return 0f;

            float count = Mathf.Max(ingredientCount.GetBaseCount(), 1f);

            if (ingredientCount.IsFixedIngredient)
            {
                return ingredientCount.FixedIngredient.BaseMarketValue * count;
            }

            // Medicine is charged at the patient's medical care level rather than market value,
            // so the player is paid for the quality of care they chose to give.
            if (ingredientCount.filter.categories != null && ingredientCount.filter.categories.Contains("Medicine"))
            {
                int medCare = pawn.playerSettings != null ? (int)pawn.playerSettings.medCare : 0;
                return count * (medCare * 15.0f);
            }

            float cheapest = 0f;
            foreach (ThingDef allowed in ingredientCount.filter.AllowedThingDefs)
            {
                if (cheapest <= 0f || allowed.BaseMarketValue < cheapest) cheapest = allowed.BaseMarketValue;
            }
            return cheapest * count;
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
