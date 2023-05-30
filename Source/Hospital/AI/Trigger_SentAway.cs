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
            //Log.Message($"SentAway? HasHediffsNeedingTendByPlayer? {pawn.health.HasHediffsNeedingTendByPlayer()} ShouldSeekMedicalRest? {HealthAIUtility.ShouldSeekMedicalRest(pawn)} pawn.health.surgeryBills.Count? {pawn.health.surgeryBills.Count } pawn.health.healthState? {pawn.health.healthState}");
            
            var result =  (!pawn.health.HasHediffsNeedingTendByPlayer() 
                    && !HealthAIUtility.ShouldSeekMedicalRest(pawn)
                    && pawn.health.surgeryBills.Count == 0
                    && pawn.health.healthState == PawnHealthState.Mobile);
            //Log.Message("result=" + result);
            return result || !pawn.IsPatient();
        }
    }
}