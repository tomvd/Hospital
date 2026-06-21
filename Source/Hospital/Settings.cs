using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Hospital;

public class Settings : ModSettings
{
    public bool ShowMessageAtArrival;
    public float SilverMultiplier = 1f;

    // Difficulty tuning - all defaults reproduce the original hardcoded behaviour.
    public float GoodwillGainMultiplier = 1f;
    public int DeathGoodwillPenalty = 10;
    public int SurgeryFailGoodwillPenalty = 1;
    public float DiseaseSeverityMultiplier = 1f;
    public float WoundSeverityMultiplier = 1f;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref SilverMultiplier, "silverMultiplier", 1f);
        Scribe_Values.Look(ref ShowMessageAtArrival, "showMessageAtArrival", false);
        Scribe_Values.Look(ref GoodwillGainMultiplier, "goodwillGainMultiplier", 1f);
        Scribe_Values.Look(ref DeathGoodwillPenalty, "deathGoodwillPenalty", 10);
        Scribe_Values.Look(ref SurgeryFailGoodwillPenalty, "surgeryFailGoodwillPenalty", 1);
        Scribe_Values.Look(ref DiseaseSeverityMultiplier, "diseaseSeverityMultiplier", 1f);
        Scribe_Values.Look(ref WoundSeverityMultiplier, "woundSeverityMultiplier", 1f);
        base.ExposeData();
    }

    public void DoWindowContents(Rect inRect)
    {
        Listing_Standard listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);

        listingStandard.CheckboxLabeled("ShowMessageAtArrival".Translate(), ref ShowMessageAtArrival, "ShowMessageAtArrivalTooltip".Translate());

        listingStandard.GapLine();
        Text.Font = GameFont.Medium;
        listingStandard.Label("DifficultySettings".Translate());
        Text.Font = GameFont.Small;

        listingStandard.SliderLabeled("SilverMultiplier".Translate(), ref SilverMultiplier, SilverMultiplier.ToString("0%"), 0f, 2f,
            "SilverMultiplierTooltip".Translate());
        listingStandard.SliderLabeled("GoodwillGainMultiplier".Translate(), ref GoodwillGainMultiplier, GoodwillGainMultiplier.ToString("0%"), 0f, 3f,
            "GoodwillGainMultiplierTooltip".Translate());

        float deathPenalty = DeathGoodwillPenalty;
        listingStandard.SliderLabeled("DeathGoodwillPenalty".Translate(), ref deathPenalty, deathPenalty.ToString("0"), 0f, 50f,
            "DeathGoodwillPenaltyTooltip".Translate());
        DeathGoodwillPenalty = Mathf.RoundToInt(deathPenalty);

        float surgeryFailPenalty = SurgeryFailGoodwillPenalty;
        listingStandard.SliderLabeled("SurgeryFailGoodwillPenalty".Translate(), ref surgeryFailPenalty, surgeryFailPenalty.ToString("0"), 0f, 50f,
            "SurgeryFailGoodwillPenaltyTooltip".Translate());
        SurgeryFailGoodwillPenalty = Mathf.RoundToInt(surgeryFailPenalty);

        listingStandard.SliderLabeled("DiseaseSeverityMultiplier".Translate(), ref DiseaseSeverityMultiplier, DiseaseSeverityMultiplier.ToString("0%"), 0.25f, 2f,
            "DiseaseSeverityMultiplierTooltip".Translate());
        listingStandard.SliderLabeled("WoundSeverityMultiplier".Translate(), ref WoundSeverityMultiplier, WoundSeverityMultiplier.ToString("0%"), 0.25f, 2f,
            "WoundSeverityMultiplierTooltip".Translate());

        if (listingStandard.ButtonText("ResetToDefaults".Translate(), widthPct: 0.35f))
        {
            ResetToDefaults();
        }

        listingStandard.End();
    }

    private void ResetToDefaults()
    {
        SilverMultiplier = 1f;
        GoodwillGainMultiplier = 1f;
        DeathGoodwillPenalty = 10;
        SurgeryFailGoodwillPenalty = 1;
        DiseaseSeverityMultiplier = 1f;
        WoundSeverityMultiplier = 1f;
    }
}
