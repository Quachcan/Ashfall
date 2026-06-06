using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Combat
{
    public static class DamageCalculator
    {
        /// <summary>
        /// Calculates final combat damage based on attacker stats, defender stats, and attack config.
        /// Includes critical hit RNG evaluation.
        /// </summary>
        public static float Calculate(float attackerStat, float defenderDef, AttackData attackData, out bool isCrit)
        {
            isCrit = false;
            if (attackData == null) return 1f;

            // Base damage calculation
            float baseDamage = (attackerStat * attackData.damageMultiplier) + attackData.flatDamage;

            // Critical Hit check
            if (Random.value <= attackData.critRate)
            {
                baseDamage *= attackData.critMultiplier;
                isCrit = true;
            }

            // Apply defense reduction based on type
            float finalDamage = attackData.damageType switch
            {
                DamageType.Physical => CalculateReduced(baseDamage, defenderDef),
                DamageType.Magic    => CalculateReduced(baseDamage, defenderDef),
                DamageType.True     => baseDamage, // Ignores defense
                _                   => CalculateReduced(baseDamage, defenderDef)
            };

            return Mathf.Max(1f, finalDamage);
        }

        private static float CalculateReduced(float damage, float def)
        {
            float reduction = 100f / (100f + Mathf.Max(0f, def));
            return damage * reduction;
        }
    }
}