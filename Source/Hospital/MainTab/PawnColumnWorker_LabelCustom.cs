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
        private int patientCountCached;
        private int bedCountCached;
        private bool full;

        private float lastTimeCached;
        private Map currentMap;
        private readonly string _bedsFilled = "BedsFilled".Translate();

        public override void DoHeader(Rect rect, PawnTable table)
        {
            base.DoHeader(rect, table);

            if (Time.unscaledTime > lastTimeCached + 2 || Find.CurrentMap != currentMap)
            {
                patientCountCached = Find.CurrentMap.GetComponent<HospitalMapComponent>().Patients.Count;
                full = Find.CurrentMap.GetComponent<HospitalMapComponent>().isFull();
                bedCountCached = Find.CurrentMap.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>().Count(bed => bed.Medical && !bed.ForPrisoners && bed.def.building.bed_humanlike && !bed.IsBurning());
                if (HospitalMod.Settings.BedsForColonists > 0.0f)
                {
                    bedCountCached -= (int)Math.Ceiling(Find.CurrentMap.mapPawns.ColonistCount * HospitalMod.Settings.BedsForColonists);
                }
                lastTimeCached = Time.unscaledTime;
                currentMap = Find.CurrentMap;
            }
/*
 *             if ((int)HospitalMod.Settings.PatientLimit > 0 && hospital.Patients.Count >=
                (int)HospitalMod.Settings.PatientLimit) return false;
            // check if we have enough beds left for colonists
                var freeMedicalBeds = map.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>().Count(bed => bed.Medical && !bed.AnyOccupants && bed.def.building.bed_humanlike && !bed.IsBurning());
            if (freeMedicalBeds <= Math.Ceiling(map.mapPawns.ColonistCount * HospitalMod.Settings.BedsForColonists)) return false;
 */
            Text.Font = DefaultHeaderFont;
            GUI.color = full ? Color.red : DefaultHeaderColor;
            Text.Anchor = TextAnchor.LowerLeft;
            Rect label = rect;
            label.y += 3f;
            Widgets.Label(label,  "BedsFilled".Translate(patientCountCached,bedCountCached));
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
