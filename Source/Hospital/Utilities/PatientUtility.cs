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
        public static bool IsPatient(this Pawn pawn,out HospitalMapComponent hospital)
        {
            hospital = null;
            if (pawn?.Map == null) return false;

            hospital = pawn.Map.GetComponent<HospitalMapComponent>(); // TODO cache this?
            return hospital?.Patients.ContainsKey(pawn) == true;
        }
        public static void AddToBill(this Pawn pawn, HospitalMapComponent hospital, float silver)
        {
            if (pawn == null) return;
            PatientData patientData = hospital.Patients.TryGetValue(pawn);
            if (patientData == null) return;
            patientData.Bill += silver;
        }
        
        public static bool GetPatientRating(this Pawn pawn, out float score, HospitalMapComponent hospital)
        {
            // idea: Rating new screen. Stars. Last 10 reviews. Store thoughts gained during stay. New thought about staff friendliness. Goodwill gained for 4 and 5 star. Loss for less than 3.
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
                patientData.Type = PatientType.Wounds;
                WoundsUtility.AddRandomWounds(pawn, patientData);
            }

        }
        
        public static float CalculateSilverToReceive(Pawn pawn, PatientData patientData)
        {
            return patientData.Bill * HospitalMod.Settings.SilverMultiplier;
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