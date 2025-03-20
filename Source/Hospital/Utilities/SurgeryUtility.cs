using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
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
		            && !recipe.defName.ToLower().Contains("abasia")
		            && !recipe.defName.ToLower().Contains("coma")
		            && !recipe.defName.ToLower().Contains("remove")
		            && !recipe.defName.ToLower().Contains("anesthetize")
		            && !recipe.defName.ToLower().Contains("blood")
		            && !recipe.defName.ToLower().Contains("admin")
		            && !recipe.defName.ToLower().Contains("rib")
		            && !recipe.defName.ToLower().Contains("harvest")
		            && !recipe.defName.ToUpper().Contains("VREA")
					)
		        {
			        if (!(from p in pawn.MapHeld.mapPawns.PawnsInFaction(Faction.OfPlayer)
				            where p.IsFreeColonist || p.IsColonyMechPlayerControlled
				            select p).Any((Pawn col) => recipe.PawnSatisfiesSkillRequirements(col)))
			        {
				        continue;
			        }
			        if (!recipe.PotentiallyMissingIngredients(null, pawn.MapHeld).Any())
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
	        if (selectedRecipe.appliedOnFixedBodyParts != null)
	        {
		        BodyPartDef bpd = selectedRecipe.appliedOnFixedBodyParts.RandomElement();
		        foreach (BodyPartRecord notMissingPart in pawn.health.hediffSet.GetNotMissingParts())
		        {
			        if (notMissingPart.def.Equals(bpd))
			        {
				        part = notMissingPart;
				        //Log.Message($"part selected1:" + part.def.defName);
			        }
		        }
	        }
	        if (part == null && selectedRecipe.Worker.GetPartsToApplyOn(pawn, selectedRecipe).Any())
	        {
		        part = selectedRecipe.Worker.GetPartsToApplyOn(pawn, selectedRecipe).RandomElement();
		        //Log.Message($"part selected2:" + part.def.defName);
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
	        if (selectedRecipe.defName.ToLower().Contains("kidney"))
	        {
		        part = pawn.health.hediffSet.GetNotMissingParts()
			        .Where(record => record.def.label.ToLower().Equals("kidney")).FirstOrFallback(null);		        
	        }
	        if (selectedRecipe.defName.ToLower().Contains("deaf"))
	        {
		        part = pawn.health.hediffSet.GetNotMissingParts()
			        .Where(record => record.def.label.ToLower().Equals("kidney")).FirstOrFallback(null);		        
	        }
	        if (selectedRecipe.defName.ToLower().Contains("glaucoma") || selectedRecipe.defName.ToLower().Contains("strabismus"))
	        {
		        part = pawn.health.hediffSet.GetNotMissingParts()
			        .Where(record => record.def.label.ToLower().Equals("eye")).FirstOrFallback(null);		        
	        }	     
	        if (selectedRecipe.defName.ToLower().Contains("arrhythmia") || selectedRecipe.defName.ToLower().Contains("heart"))
	        {
		        part = pawn.health.hediffSet.GetNotMissingParts()
			        .Where(record => record.def.label.ToLower().Equals("heart")).FirstOrFallback(null);		        
	        }
	        if (selectedRecipe.defName.ToLower().Contains("spinal") || selectedRecipe.defName.ToLower().Contains("hernia"))
	        {
		        part = pawn.health.hediffSet.GetNotMissingParts()
			        .Where(record => record.def.label.ToLower().Equals("spine")).FirstOrFallback(null);		        
	        }	
	        if (selectedRecipe.defName.ToLower().Contains("gastritis"))
	        {
		        part = pawn.health.hediffSet.GetNotMissingParts()
			        .Where(record => record.def.label.ToLower().Equals("stomach")).FirstOrFallback(null);		        
	        }
	        if (part == null)
	        {
		        IEnumerable<BodyPartRecord> source = from x in pawn.health.hediffSet.GetNotMissingParts()
			        where pawn.health.hediffSet.GetPartHealth(x) >= 2f && x.def.canScarify
			        select x;
		        if (!source.Any())
		        {
			        Log.Message($"tried to find part but could not find one");
			        return true;
		        }
		        part = source.RandomElement();
		        //Log.Message($"part selected3:" + part.def.defName);
	        }
	        //Log.Message($"part selected:" + part.Label);
	        if (selectedRecipe.removesHediff != null)
	        {
		        /*
		         * if the operation removes a hediff, we need to add it first...
		         */
		        //Log.Message($"removesHediff selected:" + selectedRecipe.removesHediff.label);
		        Hediff hediff = HediffMaker.MakeHediff(selectedRecipe.removesHediff, pawn);
		        hediff.Part = part;
		        pawn.health.AddHediff(hediff, part);
		        patientData.Diagnosis = selectedRecipe.removesHediff.label;
	        }
	        else if (selectedRecipe.workerClass == typeof(Recipe_InstallArtificialBodyPart) ||
	                 selectedRecipe.workerClass == typeof(Recipe_InstallNaturalBodyPart))
	        {
		        /*
				* if we are installing an artificial body part, it is more fun to amputate the body part first
				*/
		        if (!part.def.canSuggestAmputation)
		        {
			        HediffDef hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(
				        HealthUtility.RandomPermanentInjuryDamageType(part.def.frostbiteVulnerability > 0f &&
				                                                      pawn.RaceProps.ToolUser), pawn, part);
			        pawn.health.AddHediff(hediffDefFromDamage, part);			        
		        }
		        else
		        {
			        Hediff_MissingPart hediffMissingPart =
				        (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, pawn);
			        hediffMissingPart.lastInjury = HealthUtility.GetHediffDefFromDamage(
				        HealthUtility.RandomPermanentInjuryDamageType(part.def.frostbiteVulnerability > 0f &&
				                                                      pawn.RaceProps.ToolUser), pawn, part);
			        hediffMissingPart.Part = part;
			        hediffMissingPart.IsFresh = false;
			        pawn.health.AddHediff(hediffMissingPart, part);			        
		        }


		        patientData.Diagnosis =
			        TranslatorFormattedStringExtensions.Translate("DiagnosisArtificialBodyPart", selectedRecipe.addsHediff != null ? selectedRecipe.addsHediff.label: part.Label);
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

				List<IngredientCount> foo = selectedRecipe.ingredients
			        .FindAll(count => count.filter is { categories: not null } && count.filter.categories.Contains("Medicine"));
		        if (!foo.Empty())
		        {
					materialCost += foo.First().count * ((int)pawn.playerSettings.medCare * 15.0f);
		        }
	        }
	        patientData.Bill = Mathf.Clamp(timeCost,0,100) + Mathf.Clamp(materialCost,0, 3000);
	        return false;
        }
}