using System.Collections.Generic;
using System.Linq;
using Hospital.Utilities;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Hospital
{
    // modified IncidentWorker_VisitorGroup
    public class IncidentWorker_PatientArrives : IncidentWorker
    {
        public override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }

            Map map = (Map)parms.target;
            return IncidentHelper.CanSpawnPatient(map);
        }
        
        public override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (!IncidentHelper.CanSpawnPatient(map))
            {
                return false;
            }
            List<Faction> factions = Find.FactionManager.AllFactions.Where(f => !f.IsPlayer && !f.defeated && !f.def.hidden && f.AllyOrNeutralTo(Faction.OfPlayer) && f.def.humanlikeFaction && !f.def.defName.ToUpper().Contains("VREA")).ToList();
            /*foreach (Faction def in factions)
            {
                Log.Message(def.def.defName);
            } */           
            parms.faction = factions.RandomElement();
            Pawn pawn = IncidentHelper.GeneratePawn(parms.faction);

            PatientData patient = SpawnPatient(map, pawn);
            var list = new List<Pawn> { pawn };
            LordMaker.MakeNewLord(parms.faction, CreateLordJob(parms, list), map, list);
            //pawn.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("Patient"), pawn.Position, 1000);
            var diagnosis = patient.Diagnosis;
            TaggedString text = def.letterText.Formatted(pawn.Named("PAWN"), diagnosis).AdjustedFor(pawn);
            //text += " " + patient.baseCost.ToStringMoney();
            TaggedString title = def.letterLabel.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn);
            if (HospitalMod.Settings.ShowMessageAtArrival)
            {
                Messages.Message(title + ": " + text, MessageTypeDefOf.PositiveEvent);                
            }
            else
            {
                SendStandardLetter(title, text, LetterDefOf.PositiveEvent, parms, pawn);
            }

            return true;
        }

        protected virtual PatientData SpawnPatient(Map map, Pawn pawn)
        {
            IncidentHelper.SetUpNewPatient(pawn);
            PatientType type = (PatientType)Rand.Range(1, HospitalMod.Settings.AcceptSurgery?4:3);
            //type = PatientType.Surgery; // debug
            //Log.Message(pawn.Label + " -> " +type.ToString());
            PatientData data = new PatientData(GenDate.TicksGame, pawn.MarketValue, pawn.needs.mood.curLevelInt, type);
            //TryFindEntryCell(map, out var cell);
            //GenSpawn.Spawn(pawn, cell, map);
            var spot = map.listerBuildings.AllBuildingsColonistOfDef(ThingDef.Named("PatientLandingSpot")).RandomElement();
            var loc = DropCellFinder.TryFindSafeLandingSpotCloseToColony(map, IntVec2.Two);
            if (spot != null)
            {
                loc = spot.Position;
            }

            var activeDropPodInfo = new ActiveDropPodInfo();
            activeDropPodInfo.innerContainer.TryAdd(pawn, 1);
            activeDropPodInfo.openDelay = 60;
            activeDropPodInfo.leaveSlag = false;
            activeDropPodInfo.despawnPodBeforeSpawningThing = true;
            activeDropPodInfo.spawnWipeMode = WipeMode.Vanish;
            DropPodUtility.MakeDropPodAt(loc, map, activeDropPodInfo);
            
            HospitalMapComponent hospital = map.GetComponent<HospitalMapComponent>();
            PatientUtility.DamagePawn(pawn, data, hospital);
            hospital.PatientArrived(pawn, data);
            // this hack is needed to cancel the current patient goes to bed job and start a new one
            pawn.jobs.StopAll();
            pawn.jobs.JobTrackerTick();
            pawn.foodRestriction = new Pawn_FoodRestrictionTracker(pawn)
            {
                CurrentFoodPolicy = hospital.PatientFoodPolicy
            };
            return data;
        }
        
        protected virtual LordJob_VisitColonyAsPatient CreateLordJob(IncidentParms parms, List<Pawn> pawns)
        {
            //RCellFinder.TryFindRandomSpotJustOutsideColony(pawns[0], out var result);
            return new LordJob_VisitColonyAsPatient(parms.faction);
        }
    }
}