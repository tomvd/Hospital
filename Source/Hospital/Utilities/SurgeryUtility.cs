using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Hospital.Utilities;

public class SurgeryUtility
{
        public static bool AddRandomSurgeryBill(Pawn pawn, PatientData patientData, HospitalMapComponent hospital)
        {
	        List<RecipeDef> list = new List<RecipeDef>();
	        foreach (RecipeDef recipe in pawn.def.AllRecipes)
	        {
		        if (recipe.AvailableNow && recipe.Worker.GetPartsToApplyOn(pawn, recipe).Any() && recipe != RecipeDefOf.RemoveBodyPart && hospital.IsSurgeryRecipeAllowed(recipe))
		        {
			        IEnumerable<ThingDef> enumerable = recipe.PotentiallyMissingIngredients(null, pawn.MapHeld);
			        if (!enumerable.Any((ThingDef x) => x.isTechHediff) && !enumerable.Any((ThingDef x) => x.IsDrug) && (!enumerable.Any() || !recipe.dontShowIfAnyIngredientMissing))
			        {
				        list.Add(recipe);
				        //Log.Message($"surgery recipe added:" + recipe.defName);
			        }
		        }
	        }

	        if (list.Count == 0) return true; // in case someone blacklisted all possible surgery recipes

	        RecipeDef selectedRecipe = list.RandomElement();
	        //Log.Message($"recipe selected:" + selectedRecipe.defName);
	        if (selectedRecipe.Worker.GetPartsToApplyOn(pawn, selectedRecipe).Any())
	        {
		        BodyPartRecord part = selectedRecipe.Worker.GetPartsToApplyOn(pawn, selectedRecipe).RandomElement();
		        //Log.Message($"part selected:" + part.Label);
		        if (selectedRecipe.AvailableOnNow(pawn, part))
		        {
			        if (part.def.canSuggestAmputation)
			        {
				        HediffDef hediffDefFromDamage;
				        if (part.def.skinCovered)
				        {
					        hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(
						        HealthUtility.RandomPermanentInjuryDamageType(part.def.frostbiteVulnerability > 0f &&
						                                                      pawn.RaceProps.ToolUser), pawn, part);
				        }
				        else
				        {
					        hediffDefFromDamage = HediffDefOf.MissingBodyPart;
				        }

				        Hediff_MissingPart hediffMissingPart =
					        (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, pawn);
				        hediffMissingPart.lastInjury = hediffDefFromDamage;
				        hediffMissingPart.Part = part;
				        hediffMissingPart.IsFresh = false;
				        pawn.health.AddHediff(hediffMissingPart, part);
			        }

			        HealthCardUtility.CreateSurgeryBill(pawn, selectedRecipe, part, null, false);
			        patientData.Diagnosis = TranslatorFormattedStringExtensions.Translate("DiagnosisSurgeryOnPart",part.Label);
			        patientData.Cure = selectedRecipe.Worker.GetLabelWhenUsedOn(pawn, part);
			        patientData.CureRecipe = selectedRecipe;
		        }
	        }
	        else
	        {
		        HealthCardUtility.CreateSurgeryBill(pawn,selectedRecipe, null, null, false);
		        patientData.Diagnosis = TranslatorFormattedStringExtensions.Translate("DiagnosisSurgeryOnPart");
		        patientData.Cure = selectedRecipe.Worker.GetLabelWhenUsedOn(pawn, null);
		        patientData.CureRecipe = selectedRecipe;
	        }

	        // cost of surgery based on required medical skill - with minimum of 10
	        float skillBasedCost = Math.Max(selectedRecipe.skillRequirements
		        .Find(requirement => requirement.skill == SkillDefOf.Medicine).minLevel * 10f, 10f);
	        // cost of materials are added to this
	        float materialCost = ((int)pawn.playerSettings.medCare * 10.0f); // medicine
	        // fixed ingredients are prosthetics for example
	        foreach (IngredientCount ingredientCount in selectedRecipe.ingredients.Where(count => count.IsFixedIngredient))
	        {
		        materialCost += ingredientCount.FixedIngredient.BaseMarketValue;		        
	        }
	        patientData.Bill = skillBasedCost + materialCost;
	        return false;
        }
}