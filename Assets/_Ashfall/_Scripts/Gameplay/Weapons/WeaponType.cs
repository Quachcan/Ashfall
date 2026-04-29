namespace _Ashfall._Scripts.Gameplay.Weapons
{
    public enum WeaponType
    {
        Unarmed,    // No weapon — uses AC_Player_Base default clips (punch/kick combos)
        Sword,      // One-hand — balanced melee, 3-hit combo, block & parry
        DualSword,  // Fast — long combo, dodge only
        Staff,      // Ranged magic — AOE, burst
        Bow         // Ranged physical — trap, charge shot
    }
}
