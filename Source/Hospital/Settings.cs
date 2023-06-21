using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Hospital;

public class Settings : ModSettings
{
    public bool AcceptPatients;
    public bool AcceptSurgery;
    public float PatientLimit;
    public override void ExposeData()
    {
        Scribe_Values.Look(ref AcceptPatients, "acceptPatients", true);
        Scribe_Values.Look(ref AcceptSurgery, "acceptSurgery", true);
        Scribe_Values.Look(ref PatientLimit, "patientLimit", 0);
        base.ExposeData();
    }

    public void DoWindowContents(Rect inRect)
    {
        Listing_Standard listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);
        listingStandard.CheckboxLabeled("Accept Patients", ref AcceptPatients, "uncheck this if you want to stop patients from visiting");
        listingStandard.CheckboxLabeled("Accept Surgery", ref AcceptSurgery, "uncheck this if you do not want to get surgery events");
        listingStandard.SliderLabeled("Patient Limit", ref PatientLimit, PatientLimit.ToString("0"), 0f, 100f,
            "maximum number of patients to accept, 0=unlimited");
        listingStandard.End();
    }
}