using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Combat
{
    /// <summary>
    /// Static utility for all damage calculations.
    /// Single source of truth — change formula here, applies everywhere.
    ///
    /// Physical/Magic formula (Dark Souls style):
    ///   damage = ATK * (100 / (100 + DEF))
    ///
    ///   DEF = 0   → 100% damage (ATK * 1.0)
    ///   DEF = 100 → 50%  damage (ATK * 0.5)
    ///   DEF = 300 → 25%  damage (ATK * 0.25)
    ///
    /// True damage ignores all reduction.
    /// Minimum 1 damage always guaranteed.
    /// </summary>
    public static class DamageCalculator
    {
        /// <summary>
        /// Calculate final damage from attacker stats vs defender stats.
        /// Used for basic attacks where damage scales with ATK/DEF.
        /// </summary>
        /// <param name="atk">Attacker's ATK (or MAG) stat.</param>
        /// <param name="def">Defender's DEF stat — used for both Physical and Magic.</param>
        /// <param name="type">Damage type — affects which reduction applies.</param>
        /// <param name="zoneMultiplier">Body zone multiplier (head = 1.5x, legs = 0.8x).</param>
        /// <returns>Final damage, minimum 1.</returns>
        public static float Calculate(float atk, float def, DamageType type, float zoneMultiplier = 1f)
        {
            float raw = type switch
            {
                DamageType.Physical => CalculateReduced(atk, def),
                DamageType.Magic    => CalculateReduced(atk, def),
                DamageType.True     => atk,   // true damage — no reduction
                _                   => CalculateReduced(atk, def)
            };

            return Mathf.Max(1f, raw * zoneMultiplier);
        }

        /// <summary>
        /// Calculate final damage from flat value (skill, trap, environmental).
        /// Still applies zone multiplier but ignores ATK/DEF.
        /// </summary>
        public static float CalculateFlat(float flatDamage, DamageType type,
            float def, float zoneMultiplier = 1f)
        {
            float raw = type == DamageType.True
                ? flatDamage
                : CalculateReduced(flatDamage, def);

            return Mathf.Max(1f, raw * zoneMultiplier);
        }

        // ── Private ───────────────────────────────────────────────────────

        /// <summary>
        /// Dark Souls formula: damage * (100 / (100 + DEF))
        /// Ensures diminishing returns — doubling DEF does not halve damage.
        /// </summary>
        private static float CalculateReduced(float damage, float def)
        {
            float reduction = 100f / (100f + Mathf.Max(0f, def));
            return damage * reduction;
        }
    }
}