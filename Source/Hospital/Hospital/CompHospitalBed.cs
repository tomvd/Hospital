using System.Collections.Generic;
using Hospital.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Hospital;

    public class CompHospitalBed : ThingComp
    {
        private bool hospitalInt = true;
        private bool surgeryInt;
        public bool Hospital
        {
            get => hospitalInt;
            set
            {
                if (value == hospitalInt) return;
                RemoveAllOwners();
                hospitalInt = value;
                parent.Notify_ColorChanged();
            }
        }        
        
        public bool Surgery
        {
            get => surgeryInt;
            set
            {
                if (value == surgeryInt) return;
                if (value)
                {
                    hospitalInt = true; // surgery means hospital as well
                }
                surgeryInt = value;
                parent.Notify_ColorChanged();
            }
        }   
        
        private void RemoveAllOwners(bool destroyed = false)
        {
            for (int num = ((Building_Bed)parent).OwnersForReading.Count - 1; num >= 0; num--)
            {
                Pawn pawn = ((Building_Bed)parent).OwnersForReading[num];
                if ((!hospitalInt && pawn.IsPatient(out _)) || (hospitalInt && !pawn.IsPatient(out _)))
                {
                    // pawns in the wrong bed should look for another one...
                    pawn.ownership.UnclaimBed();
                    string key = "MessageBedLostAssignment";
                    if (destroyed)
                    {
                        key = "MessageBedDestroyed";
                    }
                    Messages.Message(key.Translate(parent.def, pawn), new LookTargets(parent, pawn),
                        MessageTypeDefOf.CautionInput, historical: false);
                }
            }
        }        

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref hospitalInt, "hospitalInt", true);
            Scribe_Values.Look(ref surgeryInt, "surgeryInt");
        }
        
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent is Building_Bed { Medical: true, ForPrisoners:false, def.building.bed_humanlike: true })
            {
                Command_Toggle commandToggle2 = new Command_Toggle();
                commandToggle2.defaultLabel = "CommandBedSetAsHospitalLabel".Translate();
                commandToggle2.defaultDesc = "CommandBedSetAsHospitalDesc".Translate();
                commandToggle2.icon = ContentFinder<Texture2D>.Get("UI/Commands/AsHospital");
                commandToggle2.isActive = () => Hospital;
                commandToggle2.toggleAction = delegate
                {
                    Hospital = !Hospital;
                };
                commandToggle2.hotKey = KeyBindingDefOf.Misc4;
                yield return commandToggle2;
                
                Command_Toggle commandToggle3 = new Command_Toggle();
                commandToggle3.defaultLabel = "CommandBedSetAsSurgeryLabel".Translate();
                commandToggle3.defaultDesc = "CommandBedSetAsSurgeryDesc".Translate();
                commandToggle3.icon = ContentFinder<Texture2D>.Get("UI/Commands/AsSurgery");
                commandToggle3.isActive = () => Surgery;
                commandToggle3.toggleAction = delegate
                {
                    Surgery = !Surgery;
                };
                commandToggle3.hotKey = KeyBindingDefOf.Misc5;
                yield return commandToggle3;              
            }
        }
    }

    public class CompProperties_HospitalBed : CompProperties
    {
        public CompProperties_HospitalBed()
        {
            compClass = typeof(CompHospitalBed);
        }
    }
