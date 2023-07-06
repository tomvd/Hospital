using RimWorld;
using Verse;

namespace Hospital;

/*
 * tries to trigger an incident every hour
 */
public class IncidentScheduler : MapComponent
{
    private int hour;
    
    public IncidentScheduler(Map map) : base(map)
    {
    }
    
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref hour, "hour");
    }

    public override void MapComponentTick()
    {
        base.MapComponentTick();
        if (GenLocalDate.HourOfDay(map) != hour)
        {
            IncidentParms parms = new IncidentParms();
            parms.target = map;
            DefDatabase<IncidentDef>.GetNamed("PatientArrives").Worker.TryExecuteWorker(parms);
            hour = GenLocalDate.HourOfDay(map);
        }
    }
}