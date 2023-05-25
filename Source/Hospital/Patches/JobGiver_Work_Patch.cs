using HarmonyLib;
using Hospital.Utilities;
using RimWorld;
using Verse;

namespace Hospitality.Patches
{
    public class JobGiver_Work_Patch
    {
        /// <summary>
        /// Allow visitors that are patients to get treated
        /// </summary>
        [HarmonyPatch(typeof(JobGiver_Work), nameof(JobGiver_Work.PawnCanUseWorkGiver))]
        public class PawnCanUseWorkGiver
        {
            [HarmonyPrefix]
            public static bool Prefix(ref bool __result, Pawn pawn, WorkGiver giver)
            {
                if (!pawn.IsPatient()) return true;

                if (!IsPatientWork(giver.def.workType)) return false;

                __result = true;
                return false;
            }

            private static bool IsPatientWork(WorkTypeDef workTypeDef)
            {
                return workTypeDef.defName.EqualsIgnoreCase("Patient") || workTypeDef.defName.EqualsIgnoreCase("PatientBedRest");
            }
        }
    }
}