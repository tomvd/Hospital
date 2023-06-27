namespace Hospital;

public enum PatientType
{
    Test = 0,
    Wounds = 1,
    Disease = 2,
    Surgery = 3
}

public class PatientData
{
    public PatientData(int arrivedAtTick, float initialMarketValue, float initialMood, PatientType type)
    {
        this.ArrivedAtTick = arrivedAtTick;
        this.InitialMarketValue = initialMarketValue;
        Type = type;
        InitialMood = initialMood;
    }

    public int ArrivedAtTick { get; set; }
    public float InitialMarketValue { get; set; }
    public float InitialMood { get; set; }
    public PatientType Type { get; set; }
    public float baseCost { get; set; }
    public string Cure { get; set; }
    public string Diagnosis { get; set; }
}