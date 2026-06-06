namespace _Ashfall._Scripts.Gameplay.Combat
{
    /// <summary>
    /// Exposes combat-relevant attributes for damage calculations.
    /// Implemented by both PlayerController and Enemy Controller.
    /// </summary>
    public interface ICombatStats
    {
        float ATK { get; }
        float MAG { get; }
        float DEF { get; }
    }
}