using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Hospital.MainTab
{
    public class Dialog_MapSettings : Window
    {
        private readonly Map map;
        private readonly HospitalMapComponent hospital;

        public Dialog_MapSettings(Map map)
        {
            this.map = map;
            this.hospital = map.GetComponent<HospitalMapComponent>();
            forcePause = true;
            doCloseX = true;
            doCloseButton = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
        }

        public override Vector2 InitialSize => new Vector2(450f, 600f);

        public override void DoWindowContents(Rect inRect)
        {
            if (Multiplayer.IsRunning)
                Multiplayer.WatchBegin();
           var listingStandard = new Listing_Standard {ColumnWidth = inRect.width};
           listingStandard.Begin(inRect);
           listingStandard.CheckboxLabeled("AcceptPatients".Translate(), ref hospital.openForBusiness, "AcceptPatientsTooltip".Translate());
           listingStandard.End();
           TimetableUtility.DoHeader(new Rect(0,0,inRect.width,50));
           TimetableUtility.DoCell(new Rect(0,60,inRect.width,20), hospital.openingHours, map);
           Rect rect = new Rect(inRect.x, 94, 170f, 28f);
           Rect rect2 = new Rect(170f, 94, 140f, 28f);
           Widgets.LabelFit(rect, "DefaultMedicineSettings".Translate());
           MedicalCareUtility.MedicalCareSetter(rect2, ref Find.PlaySettings.defaultCareForNeutralFaction);
           rect.y += 34f;
           rect2.y += 34f;
           if (hospital.refusedOperations == null) return;
           Widgets.Label(rect, "RefusedOperations".Translate());
           rect2.x = 350;
           rect.y += 20f;
           rect2.y += 20f;
           rect2.width = rect2.height; 
           foreach (var hospitalRefusedOperation in hospital.refusedOperations.ToList())
           {
               Widgets.Label(rect, hospitalRefusedOperation.LabelCap);
               if (Widgets.ButtonText(rect2, "X", true, false))
               {
                   hospital.UnRefuseOperation(hospitalRefusedOperation);
               }
               rect.y += 34f;
               rect2.y += 34f;
           }
           if (Multiplayer.IsRunning)
               Multiplayer.WatchEnd();
        }
    }
}
