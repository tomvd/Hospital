using Verse;

namespace Hospital;

public enum PatientType
{
    Test = 0,
    Wounds = 1,
    Disease = 2,
    Surgery = 3
}

public class PatientData : IExposable
{
    public PatientData(int arrivedAtTick, float initialMarketValue, float initialMood, PatientType type)
    {
        this.ArrivedAtTick = arrivedAtTick;
        this.InitialMarketValue = initialMarketValue;
        Type = type;
        InitialMood = initialMood;
    }

    public PatientData()
    {
    }

    public int ArrivedAtTick;
    public float InitialMarketValue;
    public float InitialMood;
    public PatientType Type;
    public float Bill;
    public string Cure;
    public string Diagnosis;
    public RecipeDef CureRecipe;
    public void ExposeData()
    {
        Scribe_Values.Look(ref ArrivedAtTick, "ArrivedAtTick");
        Scribe_Values.Look(ref InitialMarketValue, "InitialMarketValue");
        Scribe_Values.Look(ref InitialMood, "InitialMood");
        Scribe_Values.Look(ref Bill, "Bill");
        Scribe_Values.Look(ref Cure, "Cure");
        Scribe_Values.Look(ref Diagnosis, "Diagnosis");
        Scribe_Defs.Look(ref CureRecipe, "CureRecipe");
    }
}