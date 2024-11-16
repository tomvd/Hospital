using RimWorld;
using UnityEngine;
using Verse;

namespace Hospital.MainTab
{
    public class PawnColumnWorker_Cure : PawnColumnWorker_Text
    {
        private float lastTimeCached;
        private HospitalMapComponent hospital;

        public override string GetTextFor(Pawn pawn)
        {
            if (Time.unscaledTime > lastTimeCached + 2 || Find.CurrentMap != hospital.map)
            {
                lastTimeCached = Time.unscaledTime;
                hospital = Find.CurrentMap.GetComponent<HospitalMapComponent>();
            }

            PatientData patientData;
            if (hospital.GetPatientData(pawn, out patientData))
            {
                return patientData.Cure;
            }
            return string.Empty;
        }
        
    }
}
