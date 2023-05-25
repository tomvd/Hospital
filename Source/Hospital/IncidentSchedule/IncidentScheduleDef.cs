using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Hospital;

public class IncidentScheduleDef : Def
{
    public IncidentDef incident;
    public List<int> scheduledHours = new List<int>();
}