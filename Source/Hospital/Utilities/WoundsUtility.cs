 using System;
using System.Collections.Generic;
using System.Linq;
 using RimWorld;
 using UnityEngine;
using Verse;

namespace Hospital.Utilities;

public class WoundsUtility
{
    public static void AddRandomWounds(Pawn pawn, PatientData patientData)
    {
        Rand.seed = (uint)pawn.health.summaryHealth.SummaryHealthPercent;
        float rnd = Rand.Value;
        int damage = (int)(Mathf.Lerp(5.0f, 50.0f, rnd) * HospitalMod.Settings.WoundSeverityMultiplier);
        DamageParts(pawn, damage, null);
        patientData.Bill = 10;// Medicine.GetMedicineCountToFullyHeal(pawn) * ((int)pawn.playerSettings.medCare * 15.0f);
        patientData.Cure = "CureWounds".Translate();
        patientData.Diagnosis = "DiagnosisWounds".Translate();
    }

    private static void DamageParts(Pawn p, int totalDamage, DamageDef damageDef)
    {
        int it = 0;
        bool avoidDowning = HospitalMod.Settings.PreventIncapacitatedArrivals;
        p.health.forceDowned = true;
        while (totalDamage > 0 && it < 300)
        {
            it++;
            IEnumerable<BodyPartRecord> source = from x in p.health.hediffSet.GetNotMissingParts()
                where p.health.hediffSet.GetPartHealth(x) >= 2f && x.def.canScarify
                select x;
            if (!source.Any())
            {
                break;
            }
            BodyPartRecord bodyPartRecord = source.RandomElement();
            if (DamagePart(p, totalDamage, bodyPartRecord, out var severity, damageDef)) break;
            totalDamage -= severity;
            // Stop wounding once the patient is on the verge of collapsing, so they arrive
            // hurt-but-mobile instead of already incapacitated (a downed patient can't be
            // dismissed either, so this also lets cured mass-casualty patients leave).
            // We keep at least one wound so the pawn still needs treatment.
            if (avoidDowning && NearlyDowned(p)) break;
        }
        p.health.forceDowned = false;
    }

    // Approximates the vanilla downing triggers (unconsciousness, immobility, pain shock)
    // with a safety margin, without relying on the pawn's transient forceDowned state.
    private static bool NearlyDowned(Pawn p)
    {
        if (p.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness) < 0.40f) return true;
        if (p.health.capacities.GetLevel(PawnCapacityDefOf.Moving) < 0.20f) return true;
        float painShockThreshold = p.GetStatValue(StatDefOf.PainShockThreshold);
        if (p.health.hediffSet.PainTotal >= painShockThreshold - 0.10f) return true;
        return false;
    }

    public static bool DamagePart(Pawn p, int totalDamage, BodyPartRecord bodyPartRecord, out int severity, DamageDef damageDef)
    {
        float maxHealth = bodyPartRecord.def.GetMaxHealth(p);
        float partHealth = p.health.hediffSet.GetPartHealth(bodyPartRecord);
        int min = Mathf.Clamp(Mathf.RoundToInt(maxHealth * 0.12f), 1, (int)partHealth - 1);
        int max = Mathf.Clamp(Mathf.RoundToInt(maxHealth * 0.27f), 1, (int)partHealth - 1);
        severity = Math.Min(Rand.RangeInclusive(min, max), totalDamage);
        if (damageDef == null) damageDef = HealthUtility.RandomViolenceDamageType();
        HediffDef hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(damageDef, p, bodyPartRecord);
        if (p.health.WouldDieAfterAddingHediff(hediffDefFromDamage, bodyPartRecord, severity))
        {
            return true;
        }

        DamageInfo dinfo = new DamageInfo(damageDef, severity, 999f, -1f, null, bodyPartRecord);
        dinfo.SetAllowDamagePropagation(val: false);
        p.TakeDamage(dinfo);
        return false;
    }

    public static void AddGunshotWounds(Pawn pawn, PatientData patientData)
    {
        Rand.seed = (uint)pawn.health.summaryHealth.SummaryHealthPercent;
        float rnd = Rand.Value;
        int damage = (int)(Mathf.Lerp(5.0f, 50.0f, rnd) * HospitalMod.Settings.WoundSeverityMultiplier);
        DamageParts(pawn, damage, DamageDefOf.Bullet);
        patientData.Bill = 10;// Medicine.GetMedicineCountToFullyHeal(pawn) * ((int)pawn.playerSettings.medCare * 15.0f);
        patientData.Cure = "CureWounds".Translate();
        patientData.Diagnosis = "DiagnosisWounds".Translate();
    }

    public static void AddBruisesWounds(Pawn pawn, PatientData patientData)
    {
        Rand.seed = (uint)pawn.health.summaryHealth.SummaryHealthPercent;
        float rnd = Rand.Value;
        int damage = (int)(Mathf.Lerp(5.0f, 50.0f, rnd) * HospitalMod.Settings.WoundSeverityMultiplier);
        DamageParts(pawn, damage, (new List<DamageDef>() {DamageDefOf.Crush, DamageDefOf.Scratch, DamageDefOf.Cut}).RandomElement());
        patientData.Bill = 10;// Medicine.GetMedicineCountToFullyHeal(pawn) * ((int)pawn.playerSettings.medCare * 15.0f);
        patientData.Cure = "CureWounds".Translate();
        patientData.Diagnosis = "DiagnosisWounds".Translate();
    }

    public static void AddBurnWounds(Pawn pawn, PatientData patientData)
    {
        Rand.seed = (uint)pawn.health.summaryHealth.SummaryHealthPercent;
        float rnd = Rand.Value;
        int damage = (int)(Mathf.Lerp(5.0f, 50.0f, rnd) * HospitalMod.Settings.WoundSeverityMultiplier);
        DamageParts(pawn, damage, DamageDefOf.Burn);
        patientData.Bill = 10;// Medicine.GetMedicineCountToFullyHeal(pawn) * ((int)pawn.playerSettings.medCare * 15.0f);
        patientData.Cure = "CureWounds".Translate();
        patientData.Diagnosis = "DiagnosisWounds".Translate();
    }
}