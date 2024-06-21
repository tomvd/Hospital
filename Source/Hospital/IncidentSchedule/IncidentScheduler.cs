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
    private bool MCEtriggered;
    
    public IncidentScheduler(Map map) : base(map)
    {
    }
    
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref MCEtriggered, "MCEtriggered");
    }
    
    public override void MapComponentTick()
    {
        base.MapComponentTick();
        if (GenTicks.TicksGame % 42 == 0) // every ingame minute
        {
            int freebeds = Find.CurrentMap.GetComponent<HospitalMapComponent>().FreeMedicalBeds();
            // every ingame minute there is a chance of patients arriving minute
            float baseChance = 0.004f; // 1% every minute, gives about a patient per 100 minutes  
            if (GenDate.DaysPassed % 2 == 0)
                baseChance = 0.008f; // 2% every minute, gives about a patient per 50 minutes  

            baseChance *= Mathf.InverseLerp(0, 20, freebeds); // the more free beds, the higher the chance of patients
            
            if (Rand.Chance(baseChance))
            {
                IncidentParms parms = new IncidentParms();
                parms.target = map;
                DefDatabase<IncidentDef>.GetNamed("PatientArrives").Worker.TryExecuteWorker(parms);
            }

            if (GenDate.DaysPassed % 7 == 0 && GenDate.DaysPassed > 0)
            {
                if (!MCEtriggered)
                {
                    IncidentParms parms = new IncidentParms();
                    parms.target = map;
                    MCEtriggered = DefDatabase<IncidentDef>.GetNamed("MassCasualtyEvent").Worker
                        .TryExecuteWorker(parms);
                }
            }
            else
            {
                MCEtriggered = false;
            }
        }        
    }
}