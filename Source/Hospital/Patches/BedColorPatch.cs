using HarmonyLib;
using Hospital.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Hospital.Patches;

public class BedColorPatch
{
    /*
     * 	public override Color DrawColorTwo
	{
		get
		{
			bool medical = Medical;
			switch (forOwnerType)
			{
			case BedOwnerType.Prisoner:
				if (!medical)
				{
					return SheetColorForPrisoner;
				}
				return SheetColorMedicalForPrisoner;
			case BedOwnerType.Slave:
				if (!medical)
				{
					return SheetColorForSlave;
				}
				return SheetColorMedicalForSlave;
			default:
				if (medical)
				{
					return SheetColorMedical;
				}
				if (def == ThingDefOf.RoyalBed)
				{
					return SheetColorRoyal;
				}
				return SheetColorNormal;
			}
		}
	}
     */
    
    private static readonly Color SheetColorHospital = new Color(33f / 85f, 33f / 85f, 0.8862745f);
    private static readonly Color SheetColorSurgery = new Color(0f, 0.9f, 1.0f);
    private static readonly Color SheetColorNormal = new Color(0.6313726f, 71f / 85f, 0.7058824f);
    /// <summary>
    /// Even more colors!
    /// </summary>
    [HarmonyPatch(typeof(Building_Bed), nameof(Building_Bed.DrawColorTwo), MethodType.Getter)]
    public class DrawColorTwo
    {
	    [HarmonyPostfix]
	    public static void Postfix(ref Color __result, Building_Bed __instance)
	    {
		    if (!__instance.Medical || __instance.forOwnerType == BedOwnerType.Prisoner || __instance.forOwnerType == BedOwnerType.Slave || __instance.def == ThingDefOf.RoyalBed) return;
		    CompHospitalBed hospitalBed = __instance.TryGetComp<CompHospitalBed>();
		    if (hospitalBed == null) return;
		    if (hospitalBed.Surgery)
		    {
			    __result = SheetColorSurgery;
		    } else			    
		    if (hospitalBed.Hospital)
		    {
			    __result = SheetColorHospital;
		    }
	    }
    }    
}