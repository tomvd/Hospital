using System;
using System.Collections.Generic;
using System.Linq;
using Hospital.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Hospital
{
    public class HospitalMapComponent : MapComponent
    {
        public Dictionary<Pawn, PatientData> Patients;
        private List<Pawn> _colonistsKeysWorkingList;
        private List<PatientData> _colonistsValuesWorkingList;
        
        public bool openForBusiness = false;
        public List<bool> openingHours = new System.Collections.Generic.List<bool>
        {
            false,false,false,false,false,false,false,true, //8
            false,false,false,true, //12
            false,false,false,true,//16
            false,false,false,true,//20
            false,false,false,false
        };
        public List<RecipeDef> refusedOperations = new List<RecipeDef>();
        public FoodPolicy PatientFoodPolicy;
        public bool MassCasualties;
        public bool AcceptSurgery;
        
        public HospitalMapComponent(Map map) : base(map)
        {
            Patients = new Dictionary<Pawn, PatientData>();
            PatientFoodPolicy = Current.Game.foodRestrictionDatabase.DefaultFoodRestriction();
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            openingHours ??= new System.Collections.Generic.List<bool>
            {
                false,false,false,false,false,false,false,true, //8
                false,false,false,true, //12
                false,false,false,true,//16
                false,false,false,true,//20
                false,false,false,false
            };
            refusedOperations ??= new List<RecipeDef>();
            
            Scribe_Collections.Look(ref openingHours, "openingHours");
            Scribe_Collections.Look(ref refusedOperations, "refusedOperations");
            Scribe_Values.Look(ref openForBusiness, "openForBusiness", false);
            Patients ??= new Dictionary<Pawn, PatientData>();
            Scribe_Collections.Look(ref Patients, "patients", LookMode.Reference, LookMode.Deep, ref _colonistsKeysWorkingList, ref _colonistsValuesWorkingList);
            Scribe_References.Look(ref PatientFoodPolicy, "PatientFoodPolicy");
            PatientFoodPolicy ??= Current.Game.foodRestrictionDatabase.DefaultFoodRestriction();
            Scribe_Values.Look(ref MassCasualties, "massCasualties", false);
            Scribe_Values.Look(ref AcceptSurgery, "acceptSurgery", true);
        }

        public bool IsOpen()
        {
            if (!openForBusiness) return false;
            return openingHours[GenLocalDate.HourOfDay(map)];
        }

        public void PatientArrived(Pawn pawn, PatientData data)
        {
            Patients.Add(pawn, data);
            MainTabWindowUtility.NotifyAllPawnTables_PawnsChanged();
        }
        
        public void PatientLeaves(Pawn pawn)
        {
            if (Patients.TryGetValue(pawn, out var patientData) && !patientData.Dismissed)
            {
                float silver = PatientUtility.CalculateSilverToReceive(pawn, patientData);
                silver = Mathf.Clamp(silver, 0, 4000);
                if (silver > 0)
                {
                    if (pawn.Faction != null)
                    {
                        int goodwill = PatientUtility.CalculateGoodwillToGain(pawn, patientData);
                        Messages.Message(
                            $"{pawn.NameFullColored} leaves: +" + silver.ToStringMoney() + ", goodwill change: " +
                            goodwill + " " +
                            pawn.Faction.name, MessageTypeDefOf.NeutralEvent);
                        pawn.Faction.TryAffectGoodwillWith(Faction.OfPlayer, goodwill, false);
                    }
                    else
                    {
                        Messages.Message($"{pawn.NameFullColored} leaves: +" + silver.ToStringMoney(), MessageTypeDefOf.NeutralEvent);                        
                    }
                    var silverThing = ThingMaker.MakeThing(ThingDefOf.Silver);
                    silverThing.stackCount = (int)silver;
                    GenPlace.TryPlaceThing(silverThing, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                }

                RemoveFromPatientList(pawn);
            }
            else
            {
                Log.Message($"{pawn.NameFullColored} leaves but is not a patient anymore?");   
            }
        }

        public void PatientDied(Pawn pawn)
        {
            if (Patients.TryGetValue(pawn, out var patientData))
            {
                Messages.Message($"{pawn.NameFullColored} died: -10 "+pawn.Faction.name, MessageTypeDefOf.PawnDeath);
                pawn.Faction.TryAffectGoodwillWith(Faction.OfPlayer, -10, false);
                RemoveFromPatientList(pawn);
                PatientLeftTheMap(pawn);
            }
            // else - was not a patient?
        }
        
        public void SurgeryFailed(Pawn pawn)
        {
            if (Patients.TryGetValue(pawn, out var patientData))
            {
                Messages.Message($"{pawn.NameFullColored} failed: -5 "+pawn.Faction.name, MessageTypeDefOf.PawnDeath);
                pawn.Faction.TryAffectGoodwillWith(Faction.OfPlayer, -1, false);
                patientData.Bill = 0f;
                RemoveFromPatientList(pawn);
                PatientLeftTheMap(pawn);
            }
            // else - was not a patient?
        }        
        
        public void DismissPatient(Pawn pawn)
        {
            if (Patients.TryGetValue(pawn, out var patientData) && !patientData.Dismissed)
            {
                Messages.Message(
                    $"{pawn.NameFullColored} dismissed.", MessageTypeDefOf.NeutralEvent);
                RemoveFromPatientList(pawn);
            }
            // else - was not a patient?
            
        }

        private void RemoveFromPatientList(Pawn pawn)
        {
            if (Patients.TryGetValue(pawn, out var patientData))
            {
                patientData.Dismissed = true;
            }
            pawn.playerSettings.selfTend = true; // in case the patient gets hurt while walking home
            pawn.guest.SetGuestStatus(null); // might fix the "patients stay hanging around" issue
            MainTabWindowUtility.NotifyAllPawnTables_PawnsChanged();
        }
        
        public void PatientLeftTheMap(Pawn pawn)
        {
            Patients.Remove(pawn);    
        }

        public bool IsSurgeryRecipeAllowed(RecipeDef recipe)
        {
            return !refusedOperations.Exists(def => def.Equals(recipe));
        }

        public void RefuseOperation(Pawn pawn, RecipeDef recipe)
        {
            if (!refusedOperations.Exists(def => def.Equals(recipe)))
            {
                refusedOperations.Add(recipe);
                Messages.Message(
                    $"{recipe.LabelCap} blacklisted.", MessageTypeDefOf.NeutralEvent); 
            }

            DismissPatient(pawn);
        }
        
        public void UnRefuseOperation(RecipeDef recipe)
        {
            if (refusedOperations.Exists(def => def.Equals(recipe)))
            {
                refusedOperations.Remove(recipe);
            }
        }

        public bool IsFull()
        {
            // check if we have enough beds left for colonists
            return FreeMedicalBeds() <= 0;
        }

        public int FreeMedicalBeds()
        {
            return BedCount() - Patients.Count(pair => !pair.Value.Dismissed);            
        }

        public int BedCount()
        {
            return map.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>().Count(bed => bed.Medical
                && !bed.ForPrisoners
                && bed.def.building.bed_humanlike
                && !bed.IsBurning()
                && bed.Spawned
                && bed.TryGetComp<CompHospitalBed>() != null
                && bed.TryGetComp<CompHospitalBed>().Hospital
                && bed.Map == map);
        }

        public IEnumerable<Pawn> ActivePatientsList()
        {
            return Patients.Where(pair => !pair.Value.Dismissed).Select(pair => pair.Key);
        }

        public bool GetPatientData(Pawn pawn, out PatientData patientData)
        {
            return Patients.TryGetValue(pawn, out patientData);
        }
    }

 
}