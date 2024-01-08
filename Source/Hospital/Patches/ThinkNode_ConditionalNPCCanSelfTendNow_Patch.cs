using HarmonyLib;
using RimWorld;
using Verse;
using Hospital.Utilities;

namespace Hospital.Patches
{
    public class ThinkNode_ConditionalNPCCanSelfTendNow_Patch
    {
        /// <summary>
        /// Disable self-tending of patients
        /// </summary>
        [HarmonyPatch(typeof(ThinkNode_ConditionalNPCCanSelfTendNow), nameof(ThinkNode_ConditionalNPCCanSelfTendNow.Satisfied))]
        public class Satisfied
        {
            [HarmonyPostfix]
            public static void Postfix(ref bool __result, Pawn pawn)
            {
                if (!__result) return;
                if (pawn.IsPatient(out _)) __result = false;
            }
        }
    }
    
    public class CanSelfTendNow_Patch
    {
        /// <summary>
        /// Disable self-tending of patients
        /// </summary>
        [HarmonyPatch(typeof(JobDriver_TendPatient), nameof(JobDriver_TendPatient.TryMakePreToilReservations))]
        public class Satisfied
        {
            [HarmonyPrefix]
            public static bool Prefix(ref bool __result, JobDriver_TendPatient __instance)
            {
                if (__instance.Deliveree == __instance.pawn && __instance.pawn.IsPatient(out _))
                {
                    __result = false;
                    return false;
                }

                return true;
            }
        }
    }    
}
