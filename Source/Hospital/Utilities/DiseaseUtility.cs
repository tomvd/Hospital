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
        list = DefDatabase<HediffDef>.defsList.Where(def => def.tendable && def.scenarioCanAdd
        && !def.defName.ToLower().Contains("sepsis")
        && !def.defName.ToLower().Contains("sepsis")).ToList();
        /*foreach (HediffDef def in list)
        {
            Log.Message(def.label);
        }*/

        Hediff hediff = null;
        while (retry)
        {

            hediff = HediffMaker.MakeHediff(list.RandomElement(), pawn);
            float severity = Rand.Range(hediff.def.lethalSeverity / 10.0f, hediff.def.lethalSeverity / 4.0f);
            hediff.Severity = severity - loweredSeverity;
            if (!pawn.health.WouldDieAfterAddingHediff(hediff))
            {
                pawn.health.AddHediff(hediff);
                // check if the disease actually made the patient sick - otherwise we have to try again
                retry = (!pawn.health.HasHediffsNeedingTend() 
                         && !HealthAIUtility.ShouldSeekMedicalRest(pawn));
            }
            else
            {
                loweredSeverity += 0.1f;
                Log.Message("disease would kill patient, trying lower severity " + hediff.def.label);
                if (loweredSeverity > 0.9f) return true;
            }
        }

        patientData.Bill = Medicine.GetMedicineCountToFullyHeal(pawn) * ((int)pawn.playerSettings.medCare * 15.0f) * 2;
        patientData.Diagnosis = hediff.Label;
        patientData.Cure = "CureDisease".Translate();
        return false;
    }
}