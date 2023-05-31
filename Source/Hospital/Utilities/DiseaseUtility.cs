using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Hospital.Utilities;

public class DiseaseUtility
{
    private static List<String> diseases = new List<string> {
        "Flu",
        "Plague",
        "Malaria",
        "SleepingSickness",
        "FibrousMechanites",
        "SensoryMechanites",
        "GutWorms",
        "MuscleParasites",
    };
    public static void AddRandomDisease(Pawn pawn, PatientData patientData)
    {
        String hediffString = diseases.RandomElement();
        var hediff = HediffMaker.MakeHediff(HediffDef.Named(hediffString), pawn);
        if (hediffString == "GutWorms")
        {
            patientData.baseCost = 20;
            BodyPartRecord stomach = pawn.health.hediffSet.GetNotMissingParts().Where(record => record.def.Equals(BodyPartDefOf.Stomach)).FirstOrFallback();
            pawn.health.AddHediff(hediff, stomach);
        }
        else
        {
            patientData.baseCost = 40;
            pawn.health.AddHediff(hediff);
        }
        patientData.Cure = "cure " + hediff.Label;
    }
}