using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Hospital;

public static class IncidentHelper
{
    public static Pawn GeneratePawn(Faction faction)
    {
        Pawn p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(faction.RandomPawnKind(), faction,
            PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true, allowDead: false, allowDowned: false,
            canGeneratePawnRelations: true, false, 1f,
            forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: true, allowFood: true,
            allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false,
            forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f,
            null, null, null, null, null, null, null, null, null, null, null, null));
        return p;
    }
    
    public static IEnumerable<Pawn> GetKnownPawns(IncidentParms parms)
    {
        return Find.WorldPawns.AllPawnsAlive.Where(pawn => ValidGuest(pawn, parms.faction));
    }

    private static bool ValidGuest(Pawn pawn, Faction faction)
    {
        var validGuest = !pawn.Discarded && !pawn.Dead && !pawn.Spawned && !pawn.NonHumanlikeOrWildMan() && !pawn.Downed && pawn.Faction == faction;
        if (!validGuest) return false;
        // Leader only comes when relations are good
        if (faction.leader == pawn && faction.PlayerGoodwill < 80) return false;
        if (pawn.kindDef == PawnKindDefOf.Empire_Royal_Bestower) return false;
        if (QuestUtility.IsReservedByQuestOrQuestBeingGenerated(pawn)) return false;
        return true;
    }        

    public static bool CanSpawnPatient(Map map)
    {
        var hospital = map.GetComponent<HospitalMapComponent>();
        if (!hospital.IsOpen()) return false;
        if (hospital.IsFull()) return false;
        if (map.GameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout)) return false;
        if (map.GameConditionManager.ConditionIsActive(GameConditionDefOf.VolcanicWinter)) return false;
        //Log.Message((int)HospitalMod.Settings.PatientLimit + " - " + map.GetComponent<HospitalMapComponent>().Patients.Count);
        IntVec3 cell;
        return TryFindEntryCell(map, out cell);
    }
            
    private static bool TryFindEntryCell(Map map, out IntVec3 cell)
    {
        return CellFinder.TryFindRandomEdgeCellWith(
            (IntVec3 c) => map.reachability.CanReachColony(c) && !c.Fogged(map), map,
            CellFinder.EdgeRoadChance_Neutral, out cell);
    }

    public static void SetUpNewPatient(Pawn pawn)
    {
        pawn.guest.SetGuestStatus(Faction.OfPlayer); // mark as guest otherwise the pawn just wanders off again
        pawn.playerSettings.selfTend = false; // otherwise they will start self tending :p

        pawn.equipment.DestroyAllEquipment(); // avoid exploits
        pawn.inventory.DestroyAll();
    }
}