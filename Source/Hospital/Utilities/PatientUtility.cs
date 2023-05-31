using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Hospital.Utilities
{
    public static class PatientUtility
    {
        public static bool IsPatient(this Pawn pawn)
        {
            if (pawn == null) return false;

            var cachedComponent = pawn.Map.GetComponent<HospitalMapComponent>();
            return cachedComponent?.Patients.ContainsKey(pawn) == true;
        }
        
        public static bool GetPatientRating(this Pawn pawn, out float score)
        {
            score = 0.0f;
            if (pawn == null) return false;
            score = pawn.needs.mood.curLevelInt;
            //var cachedComponent = pawn.Map.GetComponent<HospitalMapComponent>();
            return true;
        }
        
        public static void DamagePawn(Pawn pawn, PatientData patientData)
        {
            BodyPartRecord partRecord = pawn.health.hediffSet.GetNotMissingParts()
                .Where(x => x.depth == BodyPartDepth.Outside && x.coverageAbs > 0).RandomElement();
            switch (patientData.Type)
            {
                case PatientType.Disease:
                    DiseaseUtility.AddRandomDisease(pawn, patientData);
                    break;
                case PatientType.Wounds:
                    WoundsUtility.AddRandomWounds(pawn, patientData);
	                break;
                case PatientType.Surgery:
	                SurgeryUtility.AddRandomSurgeryBill(pawn, patientData);
                    break;
            }

            ;
        }
        
        public static float CalculateSilverToReceive(Pawn pawn, PatientData patientData)
        {
            float increasedMarketValue = Math.Max(pawn.MarketValue - patientData.InitialMarketValue, 0f); // for some reason some patients leave with decreased market value :)
            float totalPrice = patientData.baseCost; // base price is 100 silver
            /*switch (patientData.Type)
            {
                case PatientType.Disease:
                case PatientType.Wounds:
                case PatientType.Surgery:
                    totalPrice += increasedMarketValue / 10f;
                    break;
            }*/

            return totalPrice;
        }

        public static int CalculateGoodwillToGain(Pawn pawn, PatientData patientData)
        {
            if (pawn.needs?.mood == null) return 0;
            float increasedMood = pawn.needs.mood.curLevelInt - patientData.InitialMood;
            // TODO this should depend on the patients health and mood somehow - perhaps doctor skills as well - to be refined
            if (pawn.needs.mood.curLevelInt > 90) return 5;
            if (pawn.needs.mood.curLevelInt > 60) return 2;
            if (pawn.needs.mood.curLevelInt > pawn.mindState.mentalBreaker.BreakThresholdMinor) return 1;
            if (pawn.needs.mood.curLevelInt > pawn.mindState.mentalBreaker.BreakThresholdMajor) return 0; // between maj and min threshold
            if (pawn.needs.mood.curLevelInt > pawn.mindState.mentalBreaker.BreakThresholdExtreme) return -1; // between maj and critical
            return -2; // at critical break threshold -> very unhappy stay
        }
        


    }
}