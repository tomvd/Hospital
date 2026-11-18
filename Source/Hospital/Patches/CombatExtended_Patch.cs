using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hospital.Utilities;
using Verse;
using Verse.AI;

namespace Hospital.Patches;

/*
 * Compatibility with Combat Extended (issue #57).
 * CE gives pawns a tactical "self tend" job through its own comp (CombatExtended.AI.CompTend),
 * which bypasses the vanilla self-tend checks patched in ThinkNode_ConditionalNPCCanSelfTendNow_Patch.
 * Patients then keep getting up to tend themselves and lying back down. Block it for patients.
 * Soft patch: the type is looked up by name, so nothing happens without CE (or if CE renames it).
 */
[HarmonyPatch]
public static class CombatExtended_CompTend_Patch
{
    private static MethodBase target;
    private static MethodInfo selPawnGetter;

    public static bool Prepare()
    {
        if (target != null) return true;
        var type = AccessTools.TypeByName("CombatExtended.AI.CompTend")
                   ?? AccessTools.AllTypes().FirstOrDefault(t => t.Name == "CompTend"
                                                                  && t.Namespace != null
                                                                  && t.Namespace.StartsWith("CombatExtended"));
        if (type == null) return false;
        target = AccessTools.Method(type, "TryGiveTacticalJob");
        selPawnGetter = AccessTools.PropertyGetter(type, "SelPawn");
        if (target == null || selPawnGetter == null)
        {
            Log.Message("[Hospital] Combat Extended found, but its self-tend API changed; self-tend compatibility is off.");
            target = null;
            return false;
        }
        return true;
    }

    public static MethodBase TargetMethod() => target;

    [HarmonyPrefix]
    public static bool Prefix(object __instance, ref Job __result)
    {
        if (selPawnGetter.Invoke(__instance, null) is Pawn pawn && pawn.IsPatient(out _))
        {
            __result = null;
            return false;
        }
        return true;
    }
}
