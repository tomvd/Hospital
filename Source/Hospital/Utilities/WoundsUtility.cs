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
        patientData.Bill = Medicine.GetMedicineCountToFullyHeal(pawn) * ((int)pawn.playerSettings.medCare * 15.0f);
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
            float maxHealth = bodyPartRecord.def.GetMaxHealth(p);
            float partHealth = p.health.hediffSet.GetPartHealth(bodyPartRecord);
            int min = Mathf.Clamp(Mathf.RoundToInt(maxHealth * 0.12f), 1, (int)partHealth - 1);
            int max = Mathf.Clamp(Mathf.RoundToInt(maxHealth * 0.27f), 1, (int)partHealth - 1);
            int severity = Math.Min(Rand.RangeInclusive(min, max), totalDamage);
            DamageDef damageDef = HealthUtility.RandomViolenceDamageType();
            HediffDef hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(damageDef, p, bodyPartRecord);
            if (p.health.WouldDieAfterAddingHediff(hediffDefFromDamage, bodyPartRecord, severity))
            {
                break;
            }
            DamageInfo dinfo = new DamageInfo(damageDef, severity, 999f, -1f, null, bodyPartRecord);
            dinfo.SetAllowDamagePropagation(val: false);
            p.TakeDamage(dinfo);
            totalDamage -= severity;
        }
        p.health.forceDowned = false;
    }


}