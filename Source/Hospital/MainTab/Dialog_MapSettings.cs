using System.Linq;
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
           var listingStandard = new Listing_Standard {ColumnWidth = inRect.width};
           listingStandard.Begin(inRect);
           listingStandard.CheckboxLabeled("AcceptPatients".Translate(), ref hospital.openForBusiness, "AcceptPatientsTooltip".Translate());
           listingStandard.End();
           TimetableUtility.DoHeader(new Rect(0,0,inRect.width,50));
           TimetableUtility.DoCell(new Rect(0,60,inRect.width,20), hospital.openingHours, map);
           Widgets.Label(new Rect(0,80,inRect.width,50),  "RefusedOperations".Translate());
           int row = 0;
           foreach (var hospitalRefusedOperation in hospital.refusedOperations.ToList())
           {
               row++;
               Widgets.Label(new Rect(0,100 + row*20,inRect.width,20),  hospitalRefusedOperation.LabelCap);
               if (Widgets.ButtonText(new Rect(400,100 + row*20,20,20), "X", true, false))
               {
                   hospital.UnRefuseOperation(hospitalRefusedOperation);
               }
           }
        }
    }
}
