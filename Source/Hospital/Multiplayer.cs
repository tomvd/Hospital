using System.Linq;
using HarmonyLib;
using Multiplayer.API;
using RimWorld;
using Verse;

namespace Hospital
{
    [StaticConstructorOnStartup]
    internal static class Multiplayer
    {
        internal static readonly ISyncField[] mapFields;

        internal static readonly ISyncField defaultCareForNeutralFactionField;
        static Multiplayer()
        {
            if (!MP.enabled) return;

            // HospitalMapComponent
            mapFields = new [] {
                //MP.RegisterSyncField(typeof(HospitalMapComponent), nameof(HospitalMapComponent.Patients)),
                MP.RegisterSyncField(typeof(HospitalMapComponent), nameof(HospitalMapComponent.openForBusiness)),
                MP.RegisterSyncField(typeof(HospitalMapComponent), nameof(HospitalMapComponent.MassCasualties)),
                MP.RegisterSyncField(typeof(HospitalMapComponent), nameof(HospitalMapComponent.AcceptSurgery)),
                MP.RegisterSyncField(typeof(HospitalMapComponent), nameof(HospitalMapComponent.AcceptDanger)),
                //MP.RegisterSyncField(typeof(HospitalMapComponent), nameof(HospitalMapComponent.refusedOperations)),
                //MP.RegisterSyncField(typeof(HospitalMapComponent), nameof(HospitalMapComponent.openingHours)),
                MP.RegisterSyncField(typeof(HospitalMapComponent), nameof(HospitalMapComponent.PatientFoodPolicy)),
                //MP.RegisterSyncField(typeof(HospitalMapComponent), nameof(HospitalMapComponent.bedsReserved))
            };
            //Interface not supported, either tricky for registered one or tricky for inner register method
            {
                var FieldRegisterMethod = AccessTools.Method(AccessTools.TypeByName("Multiplayer.Client.Sync"), "Field", [typeof(System.Type), typeof(string), typeof(string)]);
                defaultCareForNeutralFactionField = ((ISyncField)FieldRegisterMethod.Invoke(null, [null, "Verse.Current/Game/playSettings", nameof(PlaySettings.defaultCareForNeutralFaction)])).SetBufferChanges();

            }


            // methods
            {
                //MP.RegisterSyncMethod(AccessTools.Method(typeof(HospitalMapComponent), nameof(HospitalMapComponent.PatientArrived)));
                //MP.RegisterSyncMethod(AccessTools.Method(typeof(HospitalMapComponent), nameof(HospitalMapComponent.PatientLeaves)));
                //MP.RegisterSyncMethod(AccessTools.Method(typeof(HospitalMapComponent), nameof(HospitalMapComponent.PatientDied)));
                MP.RegisterSyncMethod(AccessTools.Method(typeof(HospitalMapComponent), nameof(HospitalMapComponent.DismissPatient)));
                MP.RegisterSyncMethod(AccessTools.Method(typeof(HospitalMapComponent), nameof(HospitalMapComponent.RefuseOperation)));
                MP.RegisterSyncMethod(AccessTools.Method(typeof(HospitalMapComponent), nameof(HospitalMapComponent.UnRefuseOperation)));
                MP.RegisterSyncMethod(AccessTools.Method(typeof(HospitalMapComponent), nameof(HospitalMapComponent.SetPatientFoodPolicy)));
                MP.RegisterSyncMethod(AccessTools.Method(typeof(Multiplayer), nameof(SyncOpeningHour)));
                //MP.RegisterSyncMethod(AccessTools.Method(typeof(IncidentWorker_PatientArrives),
                //    nameof(IncidentWorker_PatientArrives.TryExecuteWorker)));
                //MP.RegisterSyncMethod(AccessTools.Method(typeof(TransitionAction_EnsureHaveNearbyExitDestination),
                //    nameof(TransitionAction_EnsureHaveNearbyExitDestination.DoAction)));
                // Use reflection to avoid a compile-time reference to ParentMethodType, which is present
                // in the NuGet API package but absent in the DLL shipped by older Multiplayer mod installs.
                // A direct call bakes the ParentMethodType token into Hospital.dll, causing TypeLoadException
                // for every user when the Multiplayer mod's DLL loads first.
                var registerLambda = typeof(MP).GetMethods()
                    .FirstOrDefault(m => m.Name == nameof(MP.RegisterSyncMethodLambda));
                if (registerLambda != null)
                {
                    var pms = registerLambda.GetParameters();
                    foreach (int lambdaIndex in new[] { 1, 3 })
                    {
                        var args = new object[pms.Length];
                        args[0] = typeof(CompHospitalBed);
                        args[1] = nameof(CompHospitalBed.CompGetGizmosExtra);
                        args[2] = lambdaIndex;
                        for (int i = 3; i < pms.Length; i++) args[i] = pms[i].DefaultValue;
                        registerLambda.Invoke(null, args);
                    }
                }

            }
        }

        internal static bool IsRunning => MP.IsInMultiplayer;

        internal static void WatchBegin() => MP.WatchBegin();

        internal static void WatchEnd() => MP.WatchEnd();

        internal static void Watch(this ISyncField[] fields, object obj)
        {
            if (!MP.IsInMultiplayer) return;

            foreach(var field in fields)
            {
                field.Watch(obj);
            }
        }
        internal static void SyncOpeningHour(this HospitalMapComponent hospital, int index, bool value)
        {
            hospital.openingHours[index] = value;
        }
    }
}

