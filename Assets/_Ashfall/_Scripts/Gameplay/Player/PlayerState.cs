namespace _Ashfall._Scripts.Gameplay.Player
{
    /// <summary>
    /// All possible states for player FSM
    /// </summary>
    public enum PlayerState 
    {
        Idle,
        Run,
        Jump,
        Fall,
        Dash,
        Attack,
        BowAttack,
        CrouchIdle,
        CrouchWalk,
        Block,
        Parry,
        GuardBreak,
        Knockback,
        Dead
    }
}