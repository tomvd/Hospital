using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Hospital.MainTab
{
    public class PawnTable_Patients : PawnTable
    {
        public PawnTable_Patients(PawnTableDef def, Func<IEnumerable<Pawn>> pawnsGetter, int uiWidth, int uiHeight) : base(def, pawnsGetter, uiWidth, uiHeight) { }
        
        // Removed, so lord groups can be drawn by default (pawns are ordered by lord)
/*        public override IEnumerable<Pawn> LabelSortFunction(IEnumerable<Pawn> input)
        {
            return input;
        }*/
    }
}
