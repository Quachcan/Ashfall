namespace _Ashfall._Scripts.Core.StateMachineCore
{
    public interface IState
    {
        void Enter();
        void Exit();
        void Tick();
        void FixedTick();
    }
}