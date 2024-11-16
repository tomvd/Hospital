using Hospital.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Hospital.MainTab
{
    public class PawnColumnWorker_Bill : PawnColumnWorker_Text
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
                return PatientUtility.CalculateSilverToReceive(pawn, patientData).ToStringMoney();
            }
            return string.Empty;
        }
        
    }
}
