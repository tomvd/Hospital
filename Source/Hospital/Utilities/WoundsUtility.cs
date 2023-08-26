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
        int damage = (int)Mathf.Lerp(5.0f, 50.0f, rnd);
        DamageParts(pawn, damage);
        patientData.Bill = 10;// Medicine.GetMedicineCountToFullyHeal(pawn) * ((int)pawn.playerSettings.medCare * 15.0f);
        patientData.Cure = "CureWounds".Translate();
        patientData.Diagnosis = "DiagnosisWounds".Translate();
    }

    private static void DamageParts(Pawn p, int totalDamage)
    {
        int it = 0;
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
            if (DamagePart(p, totalDamage, bodyPartRecord, out var severity)) break;
            totalDamage -= severity;
        }
        p.health.forceDowned = false;
    }

    public static bool DamagePart(Pawn p, int totalDamage, BodyPartRecord bodyPartRecord, out int severity)
    {
        float maxHealth = bodyPartRecord.def.GetMaxHealth(p);
        float partHealth = p.health.hediffSet.GetPartHealth(bodyPartRecord);
        int min = Mathf.Clamp(Mathf.RoundToInt(maxHealth * 0.12f), 1, (int)partHealth - 1);
        int max = Mathf.Clamp(Mathf.RoundToInt(maxHealth * 0.27f), 1, (int)partHealth - 1);
        severity = Math.Min(Rand.RangeInclusive(min, max), totalDamage);
        DamageDef damageDef = HealthUtility.RandomViolenceDamageType();
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
}