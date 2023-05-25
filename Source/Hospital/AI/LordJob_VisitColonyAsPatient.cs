using Hospitality;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Hospital;
public class LordToil_Obsolete : LordToil
{
    public override void UpdateAllDuties() { }
}
public class LordJob_VisitColonyAsPatient : LordJob
{
    
private Faction faction;
        private IntVec3 chillSpot;
        private int checkEventId = -1;
        public bool leaving;
        
        public LordJob_VisitColonyAsPatient()
        {
            // Required
        }

        public LordJob_VisitColonyAsPatient(Faction faction, IntVec3 chillSpot)
        {
            this.faction = faction;
            this.chillSpot = chillSpot;
            leaving = false;
        }

        public override bool NeverInRestraints => true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref faction, "faction");
            Scribe_Values.Look(ref chillSpot, "chillSpot");
            Scribe_Values.Look(ref checkEventId, "checkEventId", -1);
            Scribe_Values.Look(ref leaving, "leaving");
        }

        public void OnLeaveTriggered()
        {
            leaving = true;
        }

        public override StateGraph CreateGraph()
        {
            StateGraph graphArrive = new StateGraph();
            StateGraph graphExit = new LordJob_TravelAndExit(IntVec3.Invalid).CreateGraph();
            StateGraph travelGraph = new LordJob_Travel(chillSpot).CreateGraph();
            travelGraph.StartingToil = new LordToil_CustomTravel(chillSpot, 0.49f, 50);
            // Arriving
            LordToil toilArriving = graphArrive.AttachSubgraph(travelGraph).StartingToil;
            // Visiting
            var toilVisiting = new LordToil_VisitPoint();
            graphArrive.lordToils.Add(toilVisiting);
            // Exit
            LordToil toilExit = graphArrive.AttachSubgraph(graphExit).StartingToil;
            // Leave map
            LordToil toilLeaveMap = graphExit.lordToils[1];
            // Take wounded
            LordToil toilTakeWounded = new LordToil_TakeWoundedGuest {lord = lord}; // This fixes the issue of missing lord when showing leave message
            graphExit.AddToil(toilTakeWounded);

            // Arrived
            {
                Transition t1 = new Transition(toilArriving, toilVisiting);
                t1.triggers.Add(new Trigger_Memo("TravelArrived"));
                graphArrive.transitions.Add(t1);
            }
            // Leave if patient is cured
            {
                Transition t5 = new Transition(toilVisiting, toilExit);
                t5.triggers.Add(new Trigger_SentAway()); // Sent away during stay
                t5.preActions.Add(new TransitionAction_EnsureHaveNearbyExitDestination());
                t5.postActions.Add(new TransitionAction_WakeAll());
                graphArrive.transitions.Add(t5);
            }
            return graphArrive;
        }
        
        public override bool ShouldRemovePawn(Pawn p, PawnLostCondition reason)
        {
            if (reason == PawnLostCondition.Incapped) return false; // they are incapped when under surgery
            return true;
        }

        public override void Notify_PawnLost(Pawn pawn, PawnLostCondition condition)
        {
            Log.Message($"{pawn.NameFullColored} lost because of {condition}");
        }
}