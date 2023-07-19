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
	        //Log.Message($"pawn.def.AllRecipes " + pawn.def.AllRecipes.Count);
	        foreach (RecipeDef recipe in pawn.def.AllRecipes)
	        {
		        if (recipe.AvailableNow 
		            && hospital.IsSurgeryRecipeAllowed(recipe)
		            && !recipe.isViolation
		            //&& recipe.Worker.GetPartsToApplyOn(pawn, recipe).Any()
		            && !recipe.defName.ToLower().Contains("coma")
		            && !recipe.defName.ToLower().Contains("remove")
		            && !recipe.defName.ToLower().Contains("anesthetize")
		            && !recipe.defName.ToLower().Contains("blood")
		            && !recipe.defName.ToLower().Contains("admin")
		            && !recipe.defName.ToLower().Contains("rib")
					)
		        {
			        if (!(from p in pawn.MapHeld.mapPawns.PawnsInFaction(Faction.OfPlayer)
				            where p.IsFreeColonist || p.IsColonyMechPlayerControlled
				            select p).Any((Pawn col) => recipe.PawnSatisfiesSkillRequirements(col)))
			        {
				        continue;
			        }
			        IEnumerable<ThingDef> enumerable = recipe.PotentiallyMissingIngredients(null, pawn.MapHeld);
			        if (!enumerable.Any((ThingDef x) => x.isTechHediff) && !enumerable.Any((ThingDef x) => x.IsDrug) && (!enumerable.Any() || !recipe.dontShowIfAnyIngredientMissing))
			        {
				        list.Add(recipe);
				        //Log.Message($"surgery recipe added:" + recipe.defName);
			        }
		        }
	        }

	        if (list.Count == 0) {
		        Log.Message($"tried to find surgery but could not find one");
		        return true; // in case someone blacklisted all possible surgery recipes
			}

	        RecipeDef selectedRecipe = list.RandomElement();
	        //Log.Message($"recipe selected:" + selectedRecipe.defName);
	        
	        BodyPartRecord part = null;
	        if (selectedRecipe.Worker.GetPartsToApplyOn(pawn, selectedRecipe).Any())
	        {
		        part = selectedRecipe.Worker.GetPartsToApplyOn(pawn, selectedRecipe).RandomElement();
	        }
	        if (selectedRecipe.removesHediff != null)
	        {
		        // some hardcoded parts from disease overhauled
		        if (selectedRecipe.removesHediff.defName.ToLower().Equals("appendicitis"))
		        {
			        part = pawn.health.hediffSet.GetNotMissingParts()
				        .Where(record => record.def.label.ToLower().Equals("torso")).FirstOrFallback(null);
		        }
		        if (selectedRecipe.removesHediff.defName.ToLower().Equals("toothache"))
		        {
			        part = pawn.health.hediffSet.GetNotMissingParts()
				        .Where(record => record.def.label.ToLower().Equals("jaw")).FirstOrFallback(null);
		        }		        
	        }
	        if (part == null)
	        {
		        IEnumerable<BodyPartRecord> source = from x in pawn.health.hediffSet.GetNotMissingParts()
			        where pawn.health.hediffSet.GetPartHealth(x) >= 2f && x.def.canScarify
			        select x;
		        if (!source.Any())
		        {
			        return true;
		        }
		        part = source.RandomElement();			        
	        }
	        //Log.Message($"part selected:" + part.Label);
	        if (selectedRecipe.removesHediff != null)
	        {
		        /*
		         * if the operation removes a hediff, we need to add it first...
		         */
		        Log.Message($"removesHediff selected:" + selectedRecipe.removesHediff.label);
		        Hediff hediff = HediffMaker.MakeHediff(selectedRecipe.removesHediff, pawn);
		        hediff.Part = part;
		        pawn.health.AddHediff(hediff, part);
		        patientData.Diagnosis = selectedRecipe.removesHediff.label;
	        }
	        else if (selectedRecipe.workerClass == typeof(Recipe_InstallArtificialBodyPart))
	        {
		        /*
				* if we are installing an artificial body part, it is more fun to amputate the body part first
				*/
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

		        patientData.Diagnosis =
			        TranslatorFormattedStringExtensions.Translate("DiagnosisArtificialBodyPart", selectedRecipe.addsHediff.label);
	        }
	        else
	        {
		        /*
		         * all other surgeries
		         */
		        patientData.Diagnosis =
			        TranslatorFormattedStringExtensions.Translate("DiagnosisSurgeryOnPart", part.Label);
	        }

	        HealthCardUtility.CreateSurgeryBill(pawn, selectedRecipe, part, null, false);
	        patientData.Cure = selectedRecipe.Worker.GetLabelWhenUsedOn(pawn, part);
	        patientData.CureRecipe = selectedRecipe;

	        if (pawn.health.surgeryBills.Count == 0)
	        {
		        Log.Message($"something went wrong creating surgery bill - recipe:" + selectedRecipe.label + " part:" + part.Label);
		        return true;
	        }

	        // cost of surgery based on required medical skill and amount of work
	        float timeCost = 0;
	        if (selectedRecipe.skillRequirements != null)
	        {
		        var medSkill = selectedRecipe.skillRequirements
			        .Find(requirement => requirement.skill == SkillDefOf.Medicine);
		        // cost of materials are added to this
		        timeCost = (selectedRecipe.workAmount / 100f) * (medSkill?.minLevel ?? 0f);
	        }

	        // fixed ingredients are prosthetics for example
	        float materialCost = 0;
	        if (selectedRecipe.ingredients != null) {
		        foreach (IngredientCount ingredientCount in selectedRecipe.ingredients.Where(count => count.IsFixedIngredient))
		        {
			        materialCost += ingredientCount.FixedIngredient.BaseMarketValue;		        
		        }
		        materialCost +=
			        selectedRecipe.ingredients
				        .Find(count => count.filter != null && count.filter.categories.Contains("Medicine")).count *
			        ((int)pawn.playerSettings.medCare * 15.0f);
	        }
	        patientData.Bill = timeCost + materialCost;
	        return false;
        }
}