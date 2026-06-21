using System;
using System.Collections.Generic;
using System.Linq;
using Hospital.Utilities;
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
            List<bool> prevOpeningHours = [];
            if (Multiplayer.IsRunning)
            {
                Multiplayer.WatchBegin();
                Multiplayer.Watch(Multiplayer.mapFields, hospital);
                Multiplayer.defaultCareForNeutralFactionField.Watch();
                prevOpeningHours = [.. hospital.openingHours];
            }
            var listingStandard = new Listing_Standard { ColumnWidth = inRect.width };
            listingStandard.Begin(inRect);
            listingStandard.Label("HospitalSettings".Translate());
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("AcceptPatients".Translate(), ref hospital.openForBusiness, "AcceptPatientsTooltip".Translate());
            /*listingStandard.SliderLabeled("BedsReserved".Translate(), ref hospital.bedsReserved, hospital.bedsReserved.ToString("0"), 0f, 10f,
                "BedsReservedTooltip".Translate());*/
            listingStandard.CheckboxLabeled("MassCasualties".Translate(), ref hospital.MassCasualties, "MassCasualtiesTooltip".Translate());
            listingStandard.CheckboxLabeled("AcceptSurgery".Translate(), ref hospital.AcceptSurgery, "AcceptSurgeryTooltip".Translate());
            listingStandard.CheckboxLabeled("AcceptDanger".Translate(), ref hospital.AcceptDanger, "AcceptDangerTooltip".Translate());
            listingStandard.End();
            int y = 120;
            TimetableUtility.DoHeader(new Rect(0, y, inRect.width, 50));
            y += 60;
            TimetableUtility.DoCell(new Rect(0, y, inRect.width, 20), hospital.openingHours, map);
            y += 34;
            Rect rect = new Rect(inRect.x, y, 170f, 28f);
            Rect rect2 = new Rect(170f, y, 140f, 28f);
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
                        hospital.SetPatientFoodPolicy(localRestriction);
                    }));
                }
                list.Add(new FloatMenuOption("ManageFoodPolicies".Translate(), delegate
                {
                    Find.WindowStack.Add(new Dialog_ManageFoodPolicies(null));
                }));
                Find.WindowStack.Add(new FloatMenu(list));
            }
            rect.y += 28f;

            // Blacklist pickers - keep the settings window compact by opening a dedicated dialog per category.
            Widgets.Label(new Rect(0f, rect.y, inRect.width, 24f), "Blacklists".Translate());
            rect.y += 26f;

            float btnWidth = inRect.width;
            if (Widgets.ButtonText(new Rect(0f, rect.y, btnWidth, 30f),
                    "BlacklistSurgeries".Translate(hospital.refusedOperations.Count)))
            {
                OpenBlacklist("BlacklistSurgeriesTitle".Translate(),
                    SurgeryUtility.CandidateSurgeries(),
                    d => !hospital.IsSurgeryRecipeAllowed((RecipeDef)d),
                    (d, refused) => hospital.SetRefusedOperation((RecipeDef)d, refused));
            }
            rect.y += 34f;
            if (Widgets.ButtonText(new Rect(0f, rect.y, btnWidth, 30f),
                    "BlacklistDiseases".Translate(hospital.refusedDiseases.Count)))
            {
                OpenBlacklist("BlacklistDiseasesTitle".Translate(),
                    DiseaseUtility.CandidateDiseases(),
                    d => !hospital.IsDiseaseAllowed((HediffDef)d),
                    (d, refused) => hospital.SetRefusedDisease((HediffDef)d, refused));
            }
            rect.y += 34f;
            if (Widgets.ButtonText(new Rect(0f, rect.y, btnWidth, 30f),
                    "BlacklistFactions".Translate(hospital.refusedFactions.Count)))
            {
                OpenBlacklist("BlacklistFactionsTitle".Translate(),
                    CandidateFactions(),
                    d => !hospital.IsFactionAllowed((FactionDef)d),
                    (d, refused) => hospital.SetRefusedFaction((FactionDef)d, refused));
            }

            if (Multiplayer.IsRunning)
            {
                for (int i = 0; i < hospital.openingHours.Count; i++)
                {
                    if (hospital.openingHours[i] != prevOpeningHours[i])
                    {
                        hospital.SyncOpeningHour(i, hospital.openingHours[i]);
                    }
                }
                Multiplayer.WatchEnd();
            }
        }
        private void OpenBlacklist(string title, IEnumerable<Def> candidates, Func<Def, bool> isBlacklisted, Action<Def, bool> setBlacklisted)
        {
            Find.WindowStack.Add(new Dialog_Blacklist(title, candidates, isBlacklisted, setBlacklisted));
        }

        // Humanlike factions that could send patients (matches the IncidentWorker_PatientArrives filter).
        private static IEnumerable<Def> CandidateFactions()
        {
            return DefDatabase<FactionDef>.AllDefs
                .Where(f => f.humanlikeFaction && !f.isPlayer && !f.hidden && !f.defName.ToUpper().Contains("VREA"));
        }

        public override void PostClose()
        {
            base.PostClose();
            Find.PlaySettings.defaultCareForFriendlyFaction = Find.PlaySettings.defaultCareForNeutralFaction;
        }
    }
}
