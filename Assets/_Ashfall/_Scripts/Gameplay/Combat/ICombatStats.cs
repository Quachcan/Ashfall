namespace _Ashfall._Scripts.Gameplay.Combat
{
    /// <summary>
    /// Exposes combat-relevant stats for damage calculation.
    /// Implemented by PlayerController and EnemyBase.
    /// DamageCalculator reads through this interface — decoupled from Player/Enemy specifics.
    /// </summary>
    public interface ICombatStats
    {
        /// <summary>Physical and magic attack power.</summary>
        float ATK { get; }
        float MAG { get; }

        /// <summary>Defense — reduces both Physical and Magic damage.</summary>
        float DEF { get; }
    }
}