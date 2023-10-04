using System;
using RimWorld;
using UnityEngine;
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
    
    /*
     * TODO : instead of this random/steady stream every now and then we want an orchestrated event of a mass intake
     * possible with some kind of "calm before the storm" period of perhaps 4h. Ideally this happens either once every 2,3 or 4 days
     * ideally during hospital opening hours :p
     * the amount in the event should correspond to the number of free beds (so the whole hospital is occupied)
     * should be a setting to disable if you dont want those events
     */

    public override void MapComponentTick()
    {
        base.MapComponentTick();
        if (GenTicks.TicksGame % 42 == 0) // every ingame minute
        {
            int freebeds = Find.CurrentMap.GetComponent<HospitalMapComponent>().FreeMedicalBeds();
            // every ingame minute there is a chance of patients arriving minute
            float baseChance = 0.004f; // 1% every minute, gives about a patient per 100 minutes  
            if (GenLocalDate.DayOfQuadrum(map) % 2 == 0)
                baseChance = 0.008f; // 2% every minute, gives about a patient per 50 minutes  

            baseChance *= Mathf.InverseLerp(0, 10, freebeds); // the more free beds, the higher the chance of patients
            
            if (Rand.Chance(baseChance))
            {
                IncidentParms parms = new IncidentParms();
                parms.target = map;
                DefDatabase<IncidentDef>.GetNamed("PatientArrives").Worker.TryExecuteWorker(parms);
            }
        }        
    }
}