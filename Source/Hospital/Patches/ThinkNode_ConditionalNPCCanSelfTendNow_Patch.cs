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
                if (pawn.IsPatient()) __result = false;
            }
        }
    }
}
