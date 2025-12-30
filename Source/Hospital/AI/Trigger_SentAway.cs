using Hospital.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Hospital
{
    public class Trigger_SentAway : Trigger
    {
        public override bool ActivateOn(Lord lord, TriggerSignal signal)
        {
            if (signal.type == TriggerSignalType.Tick && Find.TickManager.TicksAbs % 250 == 0)
            {
                return lord?.ownedPawns.Any(SentAway) == true;
            }
            return false;
        }

        // fire the patient from the hospital when cured
        private static bool SentAway(Pawn pawn)
        {
            //Log.Message($"SentAway? HasHediffsNeedingTend? {pawn.health.HasHediffsNeedingTend()} ShouldSeekMedicalRest? {HealthAIUtility.ShouldSeekMedicalRest(pawn)} pawn.health.surgeryBills.Count? {pawn.health.surgeryBills.Count } pawn.health.healthState? {pawn.health.healthState}");
            if (pawn?.Map == null) return false; // has not arrived yet...
            
            // Check if patient has pending surgery billing
            bool hasPendingSurgery = false;
            if (pawn.IsPatient(out var hospital) && hospital.GetPatientData(pawn, out var patientData))
            {
                hasPendingSurgery = patientData.HasPendingSurgeryBill;
            }

            bool canbedismissed =  (!pawn.health.HasHediffsNeedingTend()
                                   && !HealthAIUtility.ShouldSeekMedicalRest(pawn)
                                   && pawn.health.surgeryBills.Count == 0
                                   && !hasPendingSurgery
                                   && pawn.health.healthState == PawnHealthState.Mobile);
            //Log.Message("result=" + canbedismissed);
            // sometimes patients have to be reminded to stay in bed :)
            if (pawn.IsPatient(out _) && !canbedismissed && (pawn.mindState.duty == null || !pawn.mindState.duty.def.defName.Equals("Patient")))
            {
                Log.Message("mindState duty was " + pawn.mindState.duty?.def.defName.ToStringSafe());
                pawn.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("Patient"), pawn.Position, 100f);
            }
            
            if (PatientUtility.DangersOnMap(pawn.Map, out var reasons) && pawn.IsPatient(out _)) return false; //do not send cured patients into dangerous map
            
            // we indicate that the patient is just resting to get his anesthetic worked out
            /*if (pawn.IsPatient() && !canbedismissed && pawn.health.surgeryBills.Count == 0 && pawn.health.healthState == PawnHealthState.Down)
            {
                pawn.Map..treatment = "resting";
            }*/

            return canbedismissed || !pawn.IsPatient(out _, true);
        }
    }
}