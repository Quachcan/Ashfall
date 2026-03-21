namespace _Ashfall._Scripts.Core.Scene
{
    public interface ISceneService
    {
        void TransitionTo(string zoneName, string fromZoneId = null);
    }
}