namespace _Ashfall._Scripts.Gameplay.Player
{
    /// <summary>
    /// All possible states for player FSM
    /// </summary>
    public enum PlayerState 
    {
        Idle,
        Move,
        Dash,
        Attack,
        Block,
        TakeHit,
        GuardBreak,
        Dead
    }
}