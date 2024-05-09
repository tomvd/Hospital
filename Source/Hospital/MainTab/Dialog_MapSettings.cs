using System.Collections.Generic;
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
           /*listingStandard.SliderLabeled("BedsReserved".Translate(), ref hospital.bedsReserved, hospital.bedsReserved.ToString("0"), 0f, 10f,
               "BedsReservedTooltip".Translate());*/           
           listingStandard.End();
           TimetableUtility.DoHeader(new Rect(0,20,inRect.width,50));
           TimetableUtility.DoCell(new Rect(0,80,inRect.width,20), hospital.openingHours, map);
           Rect rect = new Rect(inRect.x, 114, 170f, 28f);
           Rect rect2 = new Rect(170f, 114, 140f, 28f);
           Widgets.LabelFit(rect, "DefaultMedicineSettings".Translate());
           MedicalCareUtility.MedicalCareSetter(rect2, ref Find.PlaySettings.defaultCareForNeutralFaction);
           rect.y += 34f;
           rect2.y += 34f;

           rect2 = new Rect(0f, rect.y, 140f, 23f);
           Text.Anchor = TextAnchor.MiddleLeft;
           Widgets.Label(rect2, Text.TinyFontSupported ? "FoodPolicy".Translate() : "FoodPolicyShort".Translate());
           GenUI.ResetLabelAlign();
           if (Widgets.ButtonText(new Rect(rect2.width, rect.y, 140f, 23f), hospital.PatientFoodPolicy.label))
           {
               List<FloatMenuOption> list = new List<FloatMenuOption>();
               List<FoodPolicy> allFoodRestrictions = Current.Game.foodRestrictionDatabase.AllFoodRestrictions;
               for (int i = 0; i < allFoodRestrictions.Count; i++)
               {
                   FoodPolicy localRestriction = allFoodRestrictions[i];
                   list.Add(new FloatMenuOption(localRestriction.label, delegate
                   {
                       hospital.PatientFoodPolicy = localRestriction;
                   }));
               }
               list.Add(new FloatMenuOption("ManageFoodPolicies".Translate(), delegate
               {
                   Find.WindowStack.Add(new Dialog_ManageFoodPolicies(null));
               }));
               Find.WindowStack.Add(new FloatMenu(list));
           }
           rect.y += 23f;
           rect2.y += 23f;
           
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
               rect.y += 20;
               rect2.y += 20f;
               if (rect.y > 600) break; // sorry window is full :p
           }
           if (Multiplayer.IsRunning)
               Multiplayer.WatchEnd();
        }
        public override void PostClose()
        {
            base.PostClose();
            Find.PlaySettings.defaultCareForFriendlyFaction = Find.PlaySettings.defaultCareForNeutralFaction;
        }
    }
}
