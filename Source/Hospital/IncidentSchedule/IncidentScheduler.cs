using RimWorld;
using Verse;

namespace Hospital;

/*
 * tries to trigger an incident every hour
 */
public class IncidentScheduler : MapComponent
{
    
    public IncidentScheduler(Map map) : base(map)
    {
    }
    
    public override void ExposeData()
    {
        base.ExposeData();
        //Scribe_Values.Look(ref hour, "hour");
    }

    public override void MapComponentTick()
    {
        base.MapComponentTick();
        if (GenTicks.TicksGame % 42 == 0) // every ingame minute
        {
            // every ingame minute there is a chance of patients arriving minute
            float baseChance = 0.005f; // 1% every minute, gives about a patient per 100 minutes  
            if (GenLocalDate.DayOfQuadrum(map) % 2 == 0)
                baseChance = 0.02f;
            // 7 days to die ...
            if (GenLocalDate.DayOfQuadrum(map) == 6 || GenLocalDate.DayOfQuadrum(map) == 13)
                baseChance = 0.333f;
            if (Rand.Chance(baseChance))
            {
                IncidentParms parms = new IncidentParms();
                parms.target = map;
                DefDatabase<IncidentDef>.GetNamed("PatientArrives").Worker.TryExecuteWorker(parms);
            }
        }        
    }
}