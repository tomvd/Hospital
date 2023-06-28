using System.Collections.Generic;
using Hospital.Utilities;
using RimWorld;
using Verse;

namespace Hospital
{
    public class HospitalMapComponent : MapComponent
    {
        public Dictionary<Pawn, PatientData> Patients;
        private List<Pawn> _colonistsKeysWorkingList;
        private List<PatientData> _colonistsValuesWorkingList;
        
        public HospitalMapComponent(Map map) : base(map)
        {
            Patients = new Dictionary<Pawn, PatientData>();
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Patients ??= new Dictionary<Pawn, PatientData>();
            Scribe_Collections.Look(ref Patients, "patients", LookMode.Reference, LookMode.Deep, ref _colonistsKeysWorkingList, ref _colonistsValuesWorkingList);
        }

        public void PatientArrived(Pawn pawn, PatientData data)
        {
            Patients.Add(pawn, data);
            MainTabWindowUtility.NotifyAllPawnTables_PawnsChanged();
        }

        public void PatientLeaves(Pawn pawn)
        {
            if (Patients.TryGetValue(pawn, out var patientData))
            {
                float silver = PatientUtility.CalculateSilverToReceive(pawn, patientData);
                int goodwill = PatientUtility.CalculateGoodwillToGain(pawn, patientData);
                Messages.Message(
                    $"{pawn.NameFullColored} leaves: +" + silver.ToStringMoney() + ", goodwill change: " + goodwill + " " +
                    pawn.Faction.name, MessageTypeDefOf.NeutralEvent);
                pawn.Faction.TryAffectGoodwillWith(Faction.OfPlayer, goodwill, false);
                var silverThing = ThingMaker.MakeThing(ThingDefOf.Silver);
                silverThing.stackCount = (int)silver;
                GenPlace.TryPlaceThing(silverThing, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                Patients.Remove(pawn);
                MainTabWindowUtility.NotifyAllPawnTables_PawnsChanged();
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
                MainTabWindowUtility.NotifyAllPawnTables_PawnsChanged();
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
                MainTabWindowUtility.NotifyAllPawnTables_PawnsChanged();
            }
            // else - was not a patient?
        }
        
        public void DismissPatient(Pawn pawn)
        {
            if (Patients.TryGetValue(pawn, out var patientData))
            {
                Messages.Message(
                    $"{pawn.NameFullColored} dismissed.", MessageTypeDefOf.NeutralEvent); 
                Patients.Remove(pawn);    
                MainTabWindowUtility.NotifyAllPawnTables_PawnsChanged();
            }
            // else - was not a patient?
            
        }
    }

 
}