using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Hospital;
public class LordJob_VisitColonyAsPatient : LordJob
{
        public LordJob_VisitColonyAsPatient()
        {
            // Required
        }

        public LordJob_VisitColonyAsPatient(Faction faction)
        {

        }

        public override bool NeverInRestraints => true;

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override StateGraph CreateGraph()
        {
            StateGraph graphArrive = new StateGraph();
            StateGraph graphExit = new LordJob_TravelAndExit(IntVec3.Invalid).CreateGraph();
            // Be a patient
            var toilVisiting = new LordToil_Patient();
            graphArrive.lordToils.Add(toilVisiting);
            // Exit
            LordToil toilExit = graphArrive.AttachSubgraph(graphExit).StartingToil;

            // Leave if patient is cured
            {
                Transition transition = new Transition(toilVisiting, toilExit);
                transition.triggers.Add(new Trigger_SentAway()); // Sent away during stay
                transition.preActions.Add(new TransitionAction_EnsureHaveNearbyExitDestination());
                transition.postActions.Add(new TransitionAction_WakeAll());
                graphArrive.transitions.Add(transition);
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