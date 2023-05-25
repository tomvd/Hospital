using System.Collections.Generic;
using Hospital.Utilities;
using RimWorld;
using Verse;

namespace Hospital
{
    public class HospitalMapComponent : MapComponent
    {
        public Dictionary<Pawn, PatientData> Patients { get; } = new();
        
        public HospitalMapComponent(Map map) : base(map)
        {
        }

        public void PatientArrived(Pawn pawn, PatientData data)
        {
            Patients.Add(pawn, data);
        }

        public void PatientLeaves(Pawn pawn)
        {
            if (Patients.TryGetValue(pawn, out var patientData))
            {
                float silver = PatientUtility.CalculateSilverToReceive(pawn, patientData);
                int goodwill = PatientUtility.CalculateGoodwillToGain(pawn, patientData);
                Messages.Message($"{pawn.NameFullColored} leaves: +"+silver.ToStringMoney()+", +"+goodwill+" "+pawn.Faction.name, MessageTypeDefOf.NeutralEvent);
                pawn.Faction.TryAffectGoodwillWith(Faction.OfPlayer, goodwill, false);
                var silverThing = ThingMaker.MakeThing(ThingDefOf.Silver);
                silverThing.stackCount = (int)silver;
                GenPlace.TryPlaceThing(silverThing, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                Patients.Remove(pawn);    
            }
            else
            {
                Log.Message($"{pawn.NameFullColored} leaves but is not a patient anymore?");   
            }
        }

        public void PatientDied(Pawn pawn)
        {
            if (Patients.TryGetValue(pawn, out var patientData))
            {
                Messages.Message($"{pawn.NameFullColored} died: -20 "+pawn.Faction.name, MessageTypeDefOf.PawnDeath);
                pawn.Faction.TryAffectGoodwillWith(Faction.OfPlayer, -20, false);
                Patients.Remove(pawn);    
            }
            // else - was not a patient?
        }
        
        public void SurgeryFailed(Pawn pawn)
        {
            if (Patients.TryGetValue(pawn, out var patientData))
            {
                Messages.Message($"{pawn.NameFullColored} surgery failed: -10 "+pawn.Faction.name, MessageTypeDefOf.PawnDeath);
                pawn.Faction.TryAffectGoodwillWith(Faction.OfPlayer, -10, false);
                Patients.Remove(pawn);    
            }
            // else - was not a patient?
        }
    }

 
}