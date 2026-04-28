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
    /// Bill patients for artificial body part installations
    /// </summary>
    [HarmonyPatch(typeof(Recipe_InstallArtificialBodyPart), nameof(Recipe_InstallArtificialBodyPart.ApplyOnPawn))]
    public class BillPatientsForArtificialTransplant
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            BillingHelper.BillForSurgery(pawn, bill);
        }
    }

    /// <summary>
    /// Bill patients for natural body part transplants (specific patch)
    /// </summary>
    [HarmonyPatch(typeof(Recipe_InstallNaturalBodyPart), nameof(Recipe_InstallNaturalBodyPart.ApplyOnPawn))]
    public class BillPatientsForNaturalTransplant
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            BillingHelper.BillForSurgery(pawn, bill);
        }
    }

    /// <summary>
    /// Bill patients for administered ingestibles (e.g. psychite tea, drugs)
    /// </summary>
    [HarmonyPatch(typeof(Recipe_AdministerIngestible), nameof(Recipe_AdministerIngestible.ApplyOnPawn))]
    public class BillPatientsForAdministeredIngestible
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            BillingHelper.BillForSurgery(pawn, bill);
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
