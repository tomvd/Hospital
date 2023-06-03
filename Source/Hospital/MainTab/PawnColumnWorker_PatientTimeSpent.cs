using System;
using RimWorld;
using UnityEngine;
using Verse;
using Hospital.Utilities;

namespace Hospital.MainTab
{
    public class PawnColumnWorker_PatientTimeSpent : PawnColumnWorker_Text
    {
        protected internal int ticks;
        private float lastTimeCached;
        private HospitalMapComponent hospital;

        public override string GetTextFor(Pawn pawn)
        {
            if (Time.unscaledTime > lastTimeCached + 2 || Find.CurrentMap != hospital.map)
            {
                lastTimeCached = Time.unscaledTime;
                hospital = Find.CurrentMap.GetComponent<HospitalMapComponent>();
            }
            
            if (pawn.GetPatientTimeInHospital(out ticks, hospital))
            {
                return Math.Truncate(ticks / 2500.0) + " h";
            }

            return string.Empty;
        }
    }
}
