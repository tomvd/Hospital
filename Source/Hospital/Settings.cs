using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Hospital;

public class Settings : ModSettings
{
    public bool ShowMessageAtArrival;
    public float SilverMultiplier = 1f;
    
    public override void ExposeData()
    {
        Scribe_Values.Look(ref SilverMultiplier, "silverMultiplier", 1f);
        Scribe_Values.Look(ref ShowMessageAtArrival, "showMessageAtArrival", false);
        base.ExposeData();
    }

    public void DoWindowContents(Rect inRect)
    {
        Listing_Standard listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);
        listingStandard.CheckboxLabeled("ShowMessageAtArrival".Translate(), ref ShowMessageAtArrival, "ShowMessageAtArrivalTooltip".Translate());
        listingStandard.SliderLabeled("SilverMultiplier".Translate(), ref SilverMultiplier, SilverMultiplier.ToString("0%"), 0f, 2f,
            "SilverMultiplierTooltip".Translate());        
        listingStandard.End();
    }
}