using System;
using System.Collections.Generic;
using System.Linq;
using Hospital.Utilities;
using RimWorld;
using Verse;
using Verse.AI.Group;
using Random = UnityEngine.Random;

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
            return CanSpawnPatient(map);
        }

        public virtual Pawn GeneratePawn(Faction faction)
        {
            return PawnGenerator.GeneratePawn(new PawnGenerationRequest(def.pawnKind, faction,
                PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true, allowDead: false, allowDowned: false,
                canGeneratePawnRelations: true, def.pawnMustBeCapableOfViolence, 1f,
                forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: true, allowFood: true,
                allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false,
                forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f,
                null, null, null, null, null, null, null, null, null, null, null, null));
        }

        public virtual bool CanSpawnPatient(Map map)
        {
            if (!HospitalMod.Settings.AcceptPatients) return false;
            // get number of hospital beds - we need to have more than ceil(colonists/2)
            // so if you have 5 colonists, you need more than 3 beds to receive a patient
            var freeMedicalBeds = map.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>().Count(bed => bed.Medical && !bed.AnyOccupants);
            if (freeMedicalBeds <= Math.Ceiling(map.mapPawns.ColonistCount / 2.0f)) return false;
            // 50% chance a new patient spawns this time
            if (Rand.Chance(0.5f)) return false;
            IntVec3 cell;
            return TryFindEntryCell(map, out cell);
        }



        public override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (!CanSpawnPatient(map))
            {
                return false;
            }

            Faction faction = Find.FactionManager.AllFactions.Where(f => !f.IsPlayer && !f.defeated && !f.def.hidden && !f.HostileTo(Faction.OfPlayer)).RandomElement();
            parms.faction = faction;
            Pawn pawn = GeneratePawn(faction);
            PatientData patient = SpawnPatient(map, pawn);
            var list = new List<Pawn> { pawn };
            LordMaker.MakeNewLord(parms.faction, CreateLordJob(parms, list), map, list);
            //pawn.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("Patient"), pawn.Position, 1000);
            var cure = patient.Cure;
            TaggedString text = def.letterText.Formatted(pawn.Named("PAWN"), NamedArgumentUtility.Named(cure, "CURE"))
                    .AdjustedFor(pawn);
            //text += " " + patient.baseCost.ToStringMoney();
            TaggedString title = def.letterLabel.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn);
            SendStandardLetter(title, text, LetterDefOf.PositiveEvent, parms, pawn);
            return true;
        }

        protected virtual PatientData SpawnPatient(Map map, Pawn pawn)
        {
            pawn.guest.SetGuestStatus(Faction.OfPlayer); // mark as guest otherwise the pawn just wanders off again
            pawn.playerSettings.medCare = MedicalCareCategory.NormalOrWorse;
            pawn.playerSettings.selfTend = false;

            PatientType type = (PatientType)Random.Range(1, HospitalMod.Settings.AcceptSurgery?4:3);
            PatientData data = new PatientData(GenDate.TicksGame, pawn.MarketValue, pawn.needs.mood.curLevelInt, type);
            //TryFindEntryCell(map, out var cell);
            //GenSpawn.Spawn(pawn, cell, map);

            var loc = DropCellFinder.TryFindSafeLandingSpotCloseToColony(map, IntVec2.Two);
            var activeDropPodInfo = new ActiveDropPodInfo();
            activeDropPodInfo.innerContainer.TryAdd(pawn, 1);
            activeDropPodInfo.openDelay = 60;
            activeDropPodInfo.leaveSlag = false;
            activeDropPodInfo.despawnPodBeforeSpawningThing = true;
            activeDropPodInfo.spawnWipeMode = WipeMode.Vanish;
            DropPodUtility.MakeDropPodAt(loc, map, activeDropPodInfo);
            
            PatientUtility.DamagePawn(pawn, data);
            map.GetComponent<HospitalMapComponent>().PatientArrived(pawn, data);
            return data;
        }
        
        private bool TryFindEntryCell(Map map, out IntVec3 cell)
        {
            return CellFinder.TryFindRandomEdgeCellWith(
                (IntVec3 c) => map.reachability.CanReachColony(c) && !c.Fogged(map), map,
                CellFinder.EdgeRoadChance_Neutral, out cell);
        }
        
        protected virtual LordJob_VisitColonyAsPatient CreateLordJob(IncidentParms parms, List<Pawn> pawns)
        {
            //RCellFinder.TryFindRandomSpotJustOutsideColony(pawns[0], out var result);
            return new LordJob_VisitColonyAsPatient(parms.faction);
        }
    }
}