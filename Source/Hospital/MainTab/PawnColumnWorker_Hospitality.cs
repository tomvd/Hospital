using RimWorld;
using UnityEngine;
using Verse;
using Hospital.Utilities;

namespace Hospital.MainTab
{
    public class PawnColumnWorker_PatientRating : PawnColumnWorker_Text
    {
        protected internal float score;

        public override string GetTextFor(Pawn pawn)
        {
            if (pawn.GetPatientRating(out score))
            {
                return Mathf.Clamp01(score).ToStringPercent();
            }

            return string.Empty;
        }

        public override int Compare(Pawn a, Pawn b)
        {
            return -GetValueToCompare(a).CompareTo(GetValueToCompare(b));
        }

        private static int GetValueToCompare(Pawn pawn)
        {
            // Can't use cache
            return pawn.GetPatientRating(out float s) ? (int) (s * 100) : 0;
        }
    }
}
