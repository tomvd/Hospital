using System;
using System.Collections.Generic;
using System.Linq;
using Hospital.Utilities;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Hospital
{
    // modified IncidentWorker_VisitorGroup
    public class IncidentWorker_MassCasualtyEvent : IncidentWorker
    {
        public enum MCEType
        {
            Pandemic = 0,
            Raid = 1,
            Crash = 2,
            Fire = 3
        }
        
        public override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            
            Map map = (Map)parms.target;
            if (!map.GetComponent<HospitalMapComponent>().MassCasualties)
            {
                return false;
            }            

            return IncidentHelper.CanSpawnPatient(map);
        }

        public override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            HospitalMapComponent hospital = map.GetComponent<HospitalMapComponent>();
            if (!hospital.MassCasualties)
            {
                return false;
            }            
            if (!IncidentHelper.CanSpawnPatient(map))
            {
                return false;
            }

            if (parms.pawnCount == 0)
            {
                parms.pawnCount = hospital.BedCount() + Rand.Range(-hospital.BedCount() / 5, 1); // a bit more than the hospital can handle.
            }
            List<Faction> factions = Find.FactionManager.AllFactions.Where(f => !f.IsPlayer && !f.defeated && !f.def.hidden && !f.HostileTo(Faction.OfPlayer) && f.def.humanlikeFaction && !f.def.defName.ToUpper().Contains("VREA")).ToList();
            parms.faction = factions.RandomElement();
            /*foreach (Faction def in factions)
            {
                Log.Message(def.def.defName);
            } */
            List<Pawn> list = new List<Pawn>();
            list.AddRange(IncidentHelper.GetKnownPawns(parms).InRandomOrder().Take((int)Math.Floor(parms.pawnCount * 0.20f)).ToList());
            int pawnsToGenerate = parms.pawnCount - list.Count;
            for (int i = 0; i < pawnsToGenerate; i++)
            {
                list.Add(IncidentHelper.GeneratePawn(parms.faction));
            }
            
            MCEType type = (MCEType)Rand.Range(0, 4);

            foreach (var pawn in list)
            {
                SpawnPatient(map, pawn, type);
                LordMaker.MakeNewLord(parms.faction, CreateLordJob(parms, list), map, new List<Pawn> { pawn });
            }

            //pawn.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("Patient"), pawn.Position, 1000);
            //var diagnosis = patient.Diagnosis;
            // a pandemic, a raid (gunshot wounds), a transport pod crash (scratches,bruises,etc), a colony fire (burns)
            TaggedString text = def.letterText.Formatted(("MCE_"+type).Translate(), parms.pawnCount);
            //text += " " + patient.baseCost.ToStringMoney();
            TaggedString title = def.letterLabel;
            SendStandardLetter(title, text, LetterDefOf.ThreatSmall, parms, list);

            return true;
        }

        protected virtual PatientData SpawnPatient(Map map, Pawn pawn, MCEType type)
        {
            IncidentHelper.SetUpNewPatient(pawn);
            
            PatientType ptype = PatientType.Wounds;
            if (type.Equals(MCEType.Pandemic)) ptype = PatientType.Disease;
            
            //type = PatientType.Surgery; // debug
            //Log.Message(pawn.Label + " -> " +type.ToString());
            PatientData data = new PatientData(GenDate.TicksGame, pawn.MarketValue, pawn.needs.mood.curLevelInt, ptype);
            //TryFindEntryCell(map, out var cell);
            //GenSpawn.Spawn(pawn, cell, map);
            // RandomElementWithFallback (not RandomElement) so an empty result - i.e. the player
            // has not built a Patient Landing Spot - falls back silently instead of warning.
            var spot = map.listerBuildings.AllBuildingsColonistOfDef(ThingDef.Named("PatientLandingSpot")).RandomElementWithFallback();
            var loc = DropCellFinder.TryFindSafeLandingSpotCloseToColony(map, IntVec2.Two);
            if (spot != null)
            {
                loc = spot.Position;
            }

            HospitalMapComponent hospital = map.GetComponent<HospitalMapComponent>();

            // Spawn the pawn on the map first so it has a valid Map while we apply damage/disease.
            // Damaging an unspawned pawn (pawn.Map == null) collides with hediff comps and other
            // mods' patches that dereference pawn.Map (sound makers, FleckMaker, CompPostPostAdd, ...).
            GenSpawn.Spawn(pawn, loc, map);
            PatientUtility.DamagePawn(pawn, data, hospital, type);

            // Now scoop the (already damaged) pawn back into a drop pod for the arrival animation.
            if (pawn.Spawned) pawn.DeSpawn();
            var activeDropPodInfo = new ActiveTransporterInfo();
            activeDropPodInfo.innerContainer.TryAdd(pawn, 1);
            activeDropPodInfo.openDelay = 60;
            activeDropPodInfo.leaveSlag = false;
            activeDropPodInfo.despawnPodBeforeSpawningThing = true;
            activeDropPodInfo.spawnWipeMode = WipeMode.Vanish;
            DropPodUtility.MakeDropPodAt(loc, map, activeDropPodInfo);

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