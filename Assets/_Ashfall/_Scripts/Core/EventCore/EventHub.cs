using System;
using UnityEngine;

namespace _Ashfall._Scripts.Core.EventCore
{
    [CreateAssetMenu(menuName = "Core/Event/EventHub", fileName = "EventHub", order = -1000)]
    public class EventHub : ScriptableObject
    {
        public SystemEvent systemEvents;
        
        public GameplayEvent gameplayEvents;
        
        public PlayerEvent playerEvents;
        
        public UIEvent uiEvents;
        
        [Serializable]
        public struct SystemEvent
        {
            
        }
        
        [Serializable]
        public struct GameplayEvent
        {
            
        }
        
        [Serializable]
        public struct PlayerEvent
        {
           
        }

        [Serializable]
        public struct UIEvent
        {
            
        }
    }
}