namespace _Ashfall._Scripts.Gameplay.Player
{
    /// <summary>
    /// All possible states for player FSM
    /// </summary>
    public enum PlayerState 
    {
        Idle,
        Run,
        CrouchIdle,
        CrouchWalk,
        Jump,
        Fall,
        Dash,
        Attack,
        Dead
    }
}