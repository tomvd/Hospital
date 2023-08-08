using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Hospital;

public class Settings : ModSettings
{
    public bool AcceptSurgery;
    public bool ShowMessageAtArrival;
    
    public override void ExposeData()
    {
        Scribe_Values.Look(ref AcceptSurgery, "acceptSurgery", true);
        Scribe_Values.Look(ref ShowMessageAtArrival, "showMessageAtArrival", false);
        base.ExposeData();
    }

    public void DoWindowContents(Rect inRect)
    {
        Listing_Standard listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);
        listingStandard.CheckboxLabeled("AcceptSurgery".Translate(), ref AcceptSurgery, "AcceptSurgeryTooltip".Translate());
        listingStandard.CheckboxLabeled("ShowMessageAtArrival".Translate(), ref ShowMessageAtArrival, "ShowMessageAtArrivalTooltip".Translate());
        listingStandard.End();
    }
}