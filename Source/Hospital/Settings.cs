using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Hospital;

public class Settings : ModSettings
{
    public bool AcceptPatients;
    public bool AcceptSurgery;
    public override void ExposeData()
    {
        Scribe_Values.Look(ref AcceptPatients, "acceptPatients", true);
        Scribe_Values.Look(ref AcceptSurgery, "acceptSurgery", true);
        base.ExposeData();
    }

    public void DoWindowContents(Rect inRect)
    {
        Listing_Standard listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);
        listingStandard.CheckboxLabeled("Accept Patients", ref AcceptPatients, "uncheck this if you want to stop patients from visiting");
        listingStandard.CheckboxLabeled("Accept Surgery", ref AcceptSurgery, "uncheck this if you do not want to get surgery events");
        listingStandard.End();
    }
}