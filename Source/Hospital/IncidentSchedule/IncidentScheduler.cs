using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Hospital;

public class IncidentScheduler : MapComponent
{
    private int hour;
    private List<IncidentScheduleDef> schedulers;
    
    public IncidentScheduler(Map map) : base(map)
    {
    }
    
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref hour, "hour");
    }

    public override void FinalizeInit()
    {
        base.FinalizeInit();
        schedulers = DefDatabase<IncidentScheduleDef>.defsList;
    }

    public override void MapComponentTick()
    {
        base.MapComponentTick();
        if (GenLocalDate.HourOfDay(map) != hour)
        {
            foreach (var incidentScheduleDef in schedulers)
            {
                if (incidentScheduleDef.scheduledHours.Contains(GenLocalDate.HourOfDay(map)))
                {
                    IncidentParms parms = new IncidentParms();
                    parms.target = map;
                    incidentScheduleDef.incident.Worker.TryExecuteWorker(parms);
                }
            }
            hour = GenLocalDate.HourOfDay(map);
        }
    }
}