using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Hospital.Utilities;

public class DiseaseUtility
{
    public static bool AddRandomDisease(Pawn pawn, PatientData patientData)
    {
        bool retry = true;
        float loweredSeverity = 0;
        List<HediffDef> list = new List<HediffDef>();
        list = DefDatabase<HediffDef>.defsList.Where(def => def.tendable && def.makesSickThought
        //&& def.defName.ToLower().Contains("infection") //debug
        && !def.defName.ToLower().Contains("abasia")
        && !def.defName.ToLower().Contains("sepsis")
        && !def.defName.ToLower().Contains("animal")
        && !def.defName.ToLower().Contains("gene")
        && !def.defName.ToLower().Contains("infant")).ToList();
        /*foreach (HediffDef def in list)
        {
            Log.Message(def.label);
        }*/

        Hediff hediff = null;
        while (retry)
        {

            hediff = HediffMaker.MakeHediff(list.RandomElement(), pawn);
            //Log.Message(hediff.def.label + " choosen" );
            float severity = Rand.Range(hediff.def.lethalSeverity / 10.0f, hediff.def.lethalSeverity / 4.0f);
            hediff.Severity = severity - loweredSeverity;
            if (!pawn.health.WouldDieAfterAddingHediff(hediff))
            {
                /* special case - infection also adds a wound */
                if (hediff.def.defName.ToLower().Contains("infection"))
                {
                    IEnumerable<BodyPartRecord> source = from x in pawn.health.hediffSet.GetNotMissingParts()
                        where pawn.health.hediffSet.GetPartHealth(x) >= 2f && x.def.canScarify
                        select x;
                    if (!source.Any())
                    {
                        break;
                    }
                    BodyPartRecord bodyPartRecord = source.RandomElement();
                    WoundsUtility.DamagePart(pawn, 5, bodyPartRecord, out var s);
                    pawn.health.AddHediff(hediff, bodyPartRecord); 
                }
                else
                {
                    pawn.health.AddHediff(hediff);                    
                }                
                // check if the disease actually made the patient sick - otherwise we have to try again
                //Log.Message(hediff.def.label + " pawn.health.HasHediffsNeedingTend() " + pawn.health.HasHediffsNeedingTend() );
                //Log.Message(hediff.def.label + " HealthAIUtility.ShouldSeekMedicalRest(pawn) " + HealthAIUtility.ShouldSeekMedicalRest(pawn));
                retry = (!pawn.health.HasHediffsNeedingTend() 
                         && !HealthAIUtility.ShouldSeekMedicalRest(pawn));
            }
            else
            {
                loweredSeverity += 0.1f;
                //Log.Message("disease would kill patient, trying lower severity " + hediff.def.label);
                if (loweredSeverity > 0.9f) return true;
            }
        }

        patientData.Bill = 10;//Medicine.GetMedicineCountToFullyHeal(pawn) * ((int)pawn.playerSettings.medCare * 15.0f) * 2;
        patientData.Diagnosis = hediff.Label;
        patientData.Cure = "CureDisease".Translate();
        return false;
    }
}