using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Hospital.Utilities;

public class SurgeryUtility
{
        public static void AddRandomSurgeryBill(Pawn pawn, PatientData patientData)
        {
	        List<RecipeDef> list = new List<RecipeDef>();
	        foreach (RecipeDef recipe in pawn.def.AllRecipes)
	        {
		        if (recipe.AvailableNow && recipe.Worker.GetPartsToApplyOn(pawn, recipe).Any() && recipe != RecipeDefOf.RemoveBodyPart) // only adding body part recipes for now
		        {
			        IEnumerable<ThingDef> enumerable = recipe.PotentiallyMissingIngredients(null, pawn.MapHeld);
			        if (!enumerable.Any((ThingDef x) => x.isTechHediff) && !enumerable.Any((ThingDef x) => x.IsDrug) && (!enumerable.Any() || !recipe.dontShowIfAnyIngredientMissing))
			        {
				        list.Add(recipe);
				        //Log.Message($"surgery recipe added:" + recipe.defName);
			        }
		        }
	        }
	        

	        RecipeDef selectedRecipe = list.RandomElement();
	        //Log.Message($"recipe selected:" + selectedRecipe.defName);
	        if (selectedRecipe.targetsBodyPart)
	        {
		        BodyPartRecord part = selectedRecipe.Worker.GetPartsToApplyOn(pawn, selectedRecipe).RandomElement();
		        Log.Message($"part selected:" + part.Label);
		        if (selectedRecipe.AvailableOnNow(pawn, part))
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

			        HealthCardUtility.CreateSurgeryBill(pawn, selectedRecipe, part, null, false);
			        patientData.Cure = selectedRecipe.Worker.GetLabelWhenUsedOn(pawn, part);
		        }
	        }
	        else
	        {
		        //HealthCardUtility.CreateSurgeryBill(pawn,selectedRecipe, null, null, false);
		        //patientData.Report = selectedRecipe.Worker.GetLabelWhenUsedOn(pawn, null);
	        }

	        // cost of surgery based on required medical skill - with minimum of 10
	        float skillBasedCost = Math.Max(selectedRecipe.skillRequirements
		        .Find(requirement => requirement.skill == SkillDefOf.Medicine).minLevel * 10f, 10f);
	        // cost of materials are added to this
	        float materialCost = 5; // medicine
	        // fixed ingredients are prosthetics for example
	        foreach (IngredientCount ingredientCount in selectedRecipe.ingredients.Where(count => count.IsFixedIngredient))
	        {
		        materialCost += ingredientCount.FixedIngredient.BaseMarketValue * 0.4f;		        
	        }
	        patientData.baseCost = skillBasedCost + materialCost;
        }
}