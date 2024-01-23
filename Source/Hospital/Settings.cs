using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Hospital;

public class Settings : ModSettings
{
    public bool MassCasualties;
    public bool AcceptSurgery;
    public bool ShowMessageAtArrival;
    public float SilverMultiplier = 1f;
    
    public override void ExposeData()
    {
        Scribe_Values.Look(ref MassCasualties, "massCasualties", false);
        Scribe_Values.Look(ref AcceptSurgery, "acceptSurgery", true);
        Scribe_Values.Look(ref SilverMultiplier, "silverMultiplier", 1f);
        Scribe_Values.Look(ref ShowMessageAtArrival, "showMessageAtArrival", false);
        base.ExposeData();
    }

    public void DoWindowContents(Rect inRect)
    {
        Listing_Standard listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);
        listingStandard.CheckboxLabeled("MassCasualties".Translate(), ref MassCasualties, "MassCasualtiesTooltip".Translate());
        listingStandard.CheckboxLabeled("AcceptSurgery".Translate(), ref AcceptSurgery, "AcceptSurgeryTooltip".Translate());
        listingStandard.CheckboxLabeled("ShowMessageAtArrival".Translate(), ref ShowMessageAtArrival, "ShowMessageAtArrivalTooltip".Translate());
        listingStandard.SliderLabeled("SilverMultiplier".Translate(), ref SilverMultiplier, SilverMultiplier.ToString("0%"), 0f, 2f,
            "SilverMultiplierTooltip".Translate());        
        listingStandard.End();
    }
}