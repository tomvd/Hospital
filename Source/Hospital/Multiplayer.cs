using HarmonyLib;
using Multiplayer.API;
using Verse;

namespace Hospital
{
    [StaticConstructorOnStartup]
    internal static class Multiplayer
    {
        internal static readonly ISyncField[] mapFields;
        static Multiplayer()
        {
            if (!MP.enabled) return;

            // HospitalMapComponent
            mapFields = new [] {
                MP.RegisterSyncField(typeof(HospitalMapComponent), nameof(HospitalMapComponent.Patients)),
                MP.RegisterSyncField(typeof(HospitalMapComponent), nameof(HospitalMapComponent.openForBusiness)),
                MP.RegisterSyncField(typeof(HospitalMapComponent), nameof(HospitalMapComponent.openingHours)),
                MP.RegisterSyncField(typeof(HospitalMapComponent), nameof(HospitalMapComponent.refusedOperations)),
                MP.RegisterSyncField(typeof(HospitalMapComponent), nameof(HospitalMapComponent.bedsReserved))
            };

            // methods
            {
                MP.RegisterSyncMethod(AccessTools.Method(typeof(HospitalMapComponent), nameof(HospitalMapComponent.PatientArrived)));
                MP.RegisterSyncMethod(AccessTools.Method(typeof(HospitalMapComponent), nameof(HospitalMapComponent.PatientLeaves)));
                MP.RegisterSyncMethod(AccessTools.Method(typeof(HospitalMapComponent), nameof(HospitalMapComponent.PatientDied)));
                MP.RegisterSyncMethod(AccessTools.Method(typeof(HospitalMapComponent), nameof(HospitalMapComponent.SurgeryFailed)));
                MP.RegisterSyncMethod(AccessTools.Method(typeof(HospitalMapComponent), nameof(HospitalMapComponent.DismissPatient)));
                MP.RegisterSyncMethod(AccessTools.Method(typeof(HospitalMapComponent), nameof(HospitalMapComponent.RefuseOperation)));
                MP.RegisterSyncMethod(AccessTools.Method(typeof(HospitalMapComponent), nameof(HospitalMapComponent.UnRefuseOperation)));
                MP.RegisterSyncMethod(AccessTools.Method(typeof(IncidentWorker_PatientArrives),
                    nameof(IncidentWorker_PatientArrives.TryExecuteWorker)));
                MP.RegisterSyncMethod(AccessTools.Method(typeof(TransitionAction_EnsureHaveNearbyExitDestination),
                    nameof(TransitionAction_EnsureHaveNearbyExitDestination.DoAction)));
                
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
    }
}

