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
            if (pawn?.Map == null) return false;

            var hospital = pawn.Map.GetComponent<HospitalMapComponent>(); // TODO cache this?
            return hospital?.Patients.ContainsKey(pawn) == true;
        }
        
        public static bool GetPatientRating(this Pawn pawn, out float score, HospitalMapComponent hospital)
        {
            score = 0.0f;
            if (pawn == null) return false;
            PatientData patientData = hospital.Patients.TryGetValue(pawn);
            if (patientData == null) return false;
            score = Math.Min((pawn.needs.mood.curLevelInt - patientData.InitialMood)*3.0f, 100.0f);
            return true;
        }
        
        public static bool GetPatientTimeInHospital(this Pawn pawn, out int ticks, HospitalMapComponent hospital)
        {
            ticks = 0;
            if (pawn == null) return false;
            PatientData patientData = hospital.Patients.TryGetValue(pawn);
            if (patientData == null) return false;
            ticks = GenDate.TicksGame - patientData.ArrivedAtTick;
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
            float totalPrice = Math.Max(patientData.baseCost, increasedMarketValue); // base price is 100 silver
            int ticks = GenDate.TicksGame - patientData.ArrivedAtTick;
            int days = ticks / 2500 / 24;
            if (days > 1) totalPrice += 20;
            if (days > 2) totalPrice += 20;
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
            float score = Math.Min((pawn.needs.mood.curLevelInt - patientData.InitialMood)*3.0f, 100.0f);
            if (score > 90) return 5;
            if (score > 80) return 2;
            if (score > 70) return 1;
            if (pawn.needs.mood.curLevelInt < pawn.mindState.mentalBreaker.BreakThresholdMajor) return -1; // very unhappy stay
            return 0;
        }
        


    }
}