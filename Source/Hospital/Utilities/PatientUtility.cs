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
        public static bool IsPatient(this Pawn pawn,out HospitalMapComponent hospital, bool includeDismissed = false)
        {
            hospital = null;
            if (pawn?.Map == null) return false;
            hospital = pawn.Map.GetComponent<HospitalMapComponent>(); // TODO cache this?
            if (hospital == null) return false;
            return hospital.GetPatientData(pawn, out var patientData) && (!patientData.Dismissed || !includeDismissed);
        }
        public static void AddToBill(this Pawn pawn, HospitalMapComponent hospital, float silver)
        {
            if (pawn == null) return;
            if (!hospital.GetPatientData(pawn, out var patientData)) return;
            patientData.Bill += silver;
            patientData.Bill = Mathf.Clamp(patientData.Bill, 0, 4000);
        }
        
        public static bool GetPatientRating(this Pawn pawn, out float score, HospitalMapComponent hospital)
        {
            // idea: Rating new screen. Stars. Last 10 reviews. Store thoughts gained during stay. New thought about staff friendliness. Goodwill gained for 4 and 5 star. Loss for less than 3.
            score = 0.0f;
            if (pawn == null) return false;
            if (!hospital.GetPatientData(pawn, out var patientData)) return false;
            score = Score(pawn, patientData);
            return true;
        }
        
        public static bool GetPatientTimeInHospital(this Pawn pawn, out int ticks, HospitalMapComponent hospital)
        {
            ticks = 0;
            if (pawn == null) return false;
            if (!hospital.GetPatientData(pawn, out var patientData)) return false;
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
        public static void DamagePawn(Pawn pawn, PatientData patientData, HospitalMapComponent hospital, IncidentWorker_MassCasualtyEvent.MCEType type)
        {

            switch (type)
            {
                case IncidentWorker_MassCasualtyEvent.MCEType.Pandemic:
                    DiseaseUtility.AddPlague(pawn, patientData);
                    break;
                case IncidentWorker_MassCasualtyEvent.MCEType.Raid:
                    WoundsUtility.AddGunshotWounds(pawn, patientData);
                    break;
                case IncidentWorker_MassCasualtyEvent.MCEType.Crash:
                    WoundsUtility.AddBruisesWounds(pawn, patientData);
                    break;
                case IncidentWorker_MassCasualtyEvent.MCEType.Fire:
                    WoundsUtility.AddBurnWounds(pawn, patientData);
                    break;
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
        
        public static bool DangersOnMap(Map map, out TaggedString reasons)
        {

            reasons = null;
        
            var fallout = map.GameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout);
            var potentiallyDangerous = map.mapPawns.AllPawnsSpawned.Where(p => !p.Dead && !p.IsPrisoner && !p.Downed && !IsFogged(p) && !p.InContainerEnclosed).ToArray();
            var hostileFactions = potentiallyDangerous.Where(p => p.Faction != null).Select(p => p.Faction).Where(f => f.HostileTo(Faction.OfPlayer)).ToArray();
            var winter = map.GameConditionManager.ConditionIsActive(GameConditionDefOf.VolcanicWinter);
            var manhunters = potentiallyDangerous.Where(p => p.InAggroMentalState);        

            if (!fallout && !winter && !hostileFactions.Any() && !manhunters.Any()) return false; // All clear

            var reasonList = new List<string>(); // string, so we can check for Distinct later
            if (fallout) reasonList.Add("!!! " + GameConditionDefOf.ToxicFallout.LabelCap);
            if (winter) reasonList.Add("!!! " + GameConditionDefOf.VolcanicWinter.LabelCap);

            foreach (var f in hostileFactions)
            {
                reasonList.Add("!!! " + f.def.pawnsPlural.CapitalizeFirst());
            }

            var manhunterNames = manhunters.GroupBy(p => p.MentalStateDef);
            foreach (var manhunter in manhunterNames)
            {
                if (manhunter.Count() > 1)
                    reasonList.Add($"!!! {manhunter.First().GetKindLabelPlural()} ({manhunter.First().MentalStateDef.label})");
                else if (manhunter.Count() == 1)
                    reasonList.Add($"!!! {manhunter.First().LabelShort} ({manhunter.First().MentalStateDef.label})");
            }

            reasons = reasonList.Distinct().Aggregate((a, b) => a + " " + b);
            return true;
        }   
        
        public static bool IsFogged(Pawn pawn)
        {
            return pawn.MapHeld.fogGrid.IsFogged(pawn.PositionHeld);
        }


    }
}