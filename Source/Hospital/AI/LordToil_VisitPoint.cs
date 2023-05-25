using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Hospital
{
    internal class LordToilData_VisitPoint : LordToilData
    {
        public float radius;


        public override void ExposeData()
        {
            Scribe_Values.Look(ref radius, "radius", 45f);
        }
    }

    internal class LordToil_VisitPoint : LordToil
    {
        public LordToilData_VisitPoint Data => (LordToilData_VisitPoint) data;

        public LordToil_VisitPoint()
        {
            data = new LordToilData_VisitPoint();
        }

        public override void Init()
        {
            base.Init();
            Arrive();
        }

        private void Arrive()
        {
            //Log.Message("Init State_VisitPoint "+brain.ownedPawns.Count + " - "+brain.faction.name);
        }

        public override void Cleanup()
        {
            Leave();

            base.Cleanup();
        }

        private void Leave()
        {
            var hospital = Map.GetComponent<HospitalMapComponent>();
            foreach (var pawn in lord.ownedPawns.ToArray())
            {
                hospital.PatientLeaves(pawn);
            }
        }
        public override void UpdateAllDuties()
        {
            foreach (Pawn pawn in lord.ownedPawns)
            {
                pawn.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("Patient"), pawn.Position, Data.radius);
            }
        }
    }
}
