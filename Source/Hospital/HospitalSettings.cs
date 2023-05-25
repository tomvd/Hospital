using System.Collections.Generic;
using Verse;

namespace Hospital;

public class HospitalSettings : ModSettings
{
    /// <summary>
    /// The three settings our mod has.
    /// </summary>
    public bool acceptPatients;
    //public float exampleFloat = 200f;
    //public List<Pawn> exampleListOfPawns = new List<Pawn>();

    /// <summary>
    /// The part that writes our settings to file. Note that saving is by ref.
    /// </summary>
    public override void ExposeData()
    {
        Scribe_Values.Look(ref acceptPatients, "acceptPatients", true);
        //Scribe_Values.Look(ref exampleFloat, "exampleFloat", 200f);
        //Scribe_Collections.Look(ref exampleListOfPawns, "exampleListOfPawns", LookMode.Reference);
        base.ExposeData();
    }
}