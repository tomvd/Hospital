using HarmonyLib;
using UnityEngine;
using Verse;

namespace Hospital
{

	[StaticConstructorOnStartup]
	public class HospitalMod : Mod
	{
		public static Harmony harmonyInstance;
		HospitalSettings settings;
		
		public HospitalMod(ModContentPack content) : base(content)
		{
			this.settings = GetSettings<HospitalSettings>();

			harmonyInstance = new Harmony("Adamas.Hospital");
            harmonyInstance.PatchAll();
        }
        /// <summary>
        /// The (optional) GUI part to set your settings.
        /// </summary>
        /// <param name="inRect">A Unity Rect with the size of the settings window.</param>
        public override void DoSettingsWindowContents(Rect inRect)
        {
	        Listing_Standard listingStandard = new Listing_Standard();
	        listingStandard.Begin(inRect);
	        listingStandard.CheckboxLabeled("Accept Patients", ref settings.acceptPatients, "uncheck this if you want to stop patients from visiting");
	        //listingStandard.Label("exampleFloatExplanation");
	        //settings.exampleFloat = listingStandard.Slider(settings.exampleFloat, 100f, 300f);
	        listingStandard.End();
	        base.DoSettingsWindowContents(inRect);
        }

        /// <summary>
        /// Override SettingsCategory to show up in the list of settings.
        /// Using .Translate() is optional, but does allow for localisation.
        /// </summary>
        /// <returns>The (translated) mod name.</returns>
        public override string SettingsCategory()
        {
	        return "Hospital";
        }
	}
}