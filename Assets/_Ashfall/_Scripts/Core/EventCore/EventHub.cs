using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace _Ashfall._Scripts.Core.EventCore
{
    [CreateAssetMenu(menuName = "Core/Event/EventHub", fileName = "EventHub", order = -1000)]
    public class EventHub : ScriptableObject
    {
        [FoldoutGroup("System Events", expanded: false)]
        public SystemEvent systemEvents;
        
        [FoldoutGroup("Gameplay Events", expanded: true)]
        public GameplayEvent gameplayEvents;
        
        [FoldoutGroup("Player Events", expanded: true)]
        public PlayerEvent playerEvents;
        
        [FoldoutGroup("Ui Events", expanded: false)]
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