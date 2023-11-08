using System;
using System.Linq;
using Verse;

namespace Hospital;

[StaticConstructorOnStartup]
public static class InjectHospitalBed
{
    // all hospital beds get a hospitalbed comp to store their allow state
    static InjectHospitalBed()
    {
        var defs = DefDatabase<ThingDef>.AllDefsListForReading.Where(def => def.IsBed && def.building.bed_canBeMedical).ToList();
        defs.RemoveDuplicates();
        
        foreach (var def in defs)
        {
            if (def.comps == null) continue;

            if (!def.comps.Any(c => c.GetType() == typeof(CompProperties_HospitalBed)))
            {
                CompProperties_HospitalBed prop =
                    (CompProperties_HospitalBed)Activator.CreateInstance(typeof(CompProperties_HospitalBed));
                def.comps.Add(prop);
                //Log.Message(def.defName + ": potential hospitalbed");
            }
        }
    }
}


