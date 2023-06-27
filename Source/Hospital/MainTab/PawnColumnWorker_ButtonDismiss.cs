using RimWorld;
using UnityEngine;
using Verse;

namespace Hospital.MainTab {
    public class PawnColumnWorker_ButtonDismiss : PawnColumnWorker
    {
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (Widgets.ButtonText(rect, "Dismiss".Translate()))
            {
                CameraJumper.TryJumpAndSelect(pawn);
                pawn.Map.GetComponent<HospitalMapComponent>().DismissPatient(pawn);
            }
        }

        public override int GetMinWidth(PawnTable table)
        {
            return Mathf.Max(base.GetMinWidth(table), 60);
        }

        public override int GetMaxWidth(PawnTable table)
        {
            return Mathf.Min(base.GetMaxWidth(table), this.GetMinWidth(table));
        }
    }
}