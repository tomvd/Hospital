using System;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using static System.String;

namespace Hospital.MainTab
{
    public class PawnColumnWorker_LabelCustom : PawnColumnWorker_Label
    {
        private int guestCountCached;
        private int bedCountCached;

        private float lastTimeCached;
        private Map currentMap;

        public override void DoHeader(Rect rect, PawnTable table)
        {
            base.DoHeader(rect, table);

            if (Time.unscaledTime > lastTimeCached + 2 || Find.CurrentMap != currentMap)
            {
                guestCountCached = Find.CurrentMap.GetComponent<HospitalMapComponent>().Patients.Count;
                bedCountCached = Find.CurrentMap.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>().Count(bed => bed.Medical && !bed.AnyOccupants);
                lastTimeCached = Time.unscaledTime;
                currentMap = Find.CurrentMap;
            }

            Text.Font = DefaultHeaderFont;
            GUI.color = guestCountCached + Math.Ceiling(Find.CurrentMap.mapPawns.ColonistCount / 2.0f) > bedCountCached ? Color.red : DefaultHeaderColor;
            Text.Anchor = TextAnchor.LowerLeft;
            Rect label = rect;
            label.y += 3f;
            Widgets.Label(label, "BedsFilled".Translate(guestCountCached, bedCountCached));
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
        }

        public override int Compare(Pawn a, Pawn b)
        {
            return CompareOrdinal(a.Name.ToStringShort, b.Name.ToStringShort);
        }
    }
}
