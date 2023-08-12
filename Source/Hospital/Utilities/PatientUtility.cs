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
            score = Score(pawn, patientData);
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
        
        public static void DamagePawn(Pawn pawn, PatientData patientData, HospitalMapComponent hospital)
        {
            bool failed = false;
            switch (patientData.Type)
            {
                case PatientType.Disease:
                    failed = DiseaseUtility.AddRandomDisease(pawn, patientData);
                    break;
                case PatientType.Wounds:
                    WoundsUtility.AddRandomWounds(pawn, patientData);
	                break;
                case PatientType.Surgery:
                    failed = SurgeryUtility.AddRandomSurgeryBill(pawn, patientData, hospital);
                    break;
            }
            if (failed)
            {
                WoundsUtility.AddRandomWounds(pawn, patientData);
            }

        }
        
        public static float CalculateSilverToReceive(Pawn pawn, PatientData patientData)
        {
            float increasedMarketValue = Math.Max(pawn.MarketValue - patientData.InitialMarketValue, 0f); // for some reason some patients leave with decreased market value :)
            float totalPrice = Math.Max(patientData.Bill, increasedMarketValue); // base price is 100 silver
            int ticks = GenDate.TicksGame - patientData.ArrivedAtTick;
            int days = ticks / 2500 / 6;
            totalPrice += 20 * days;
            /*switch (patientData.Type)
            {
                case PatientType.Disease:
                case PatientType.Wounds:
                case PatientType.Surgery:
                    totalPrice += increasedMarketValue / 10f;
                    break;
            }*/

            return totalPrice * HospitalMod.Settings.SilverMultiplier;
        }

        public static int CalculateGoodwillToGain(Pawn pawn, PatientData patientData)
        {
            if (pawn.needs?.mood == null) return 0;
            float score = Score(pawn, patientData);
            if (score > 0.9f) return 5;
            if (score > 0.8f) return 4;
            if (score > 0.7f) return 3;
            if (score > 0.6f) return 2;
            if (pawn.needs.mood.curLevelInt > pawn.mindState.mentalBreaker.BreakThresholdMinor) return 1; // ok stay
            if (pawn.needs.mood.curLevelInt < pawn.mindState.mentalBreaker.BreakThresholdMajor) return -1; // very unhappy stay
            return 0;
        }

        private static float Score(Pawn pawn, PatientData patientData)
        {
            if (pawn.needs?.mood == null) return 0;
            return Math.Min((pawn.needs.mood.CurInstantLevel - patientData.InitialMood)*3.5f, 100.0f);
        }
    }
}