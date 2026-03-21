using System;
using UnityEngine;

namespace _Ashfall._Scripts.Core.EventCore.EventClasses
{
    [CreateAssetMenu(menuName = "Core/Event/EventClasses/Void Event", fileName = "Void Event")]
    public class VoidEvent : ScriptableObject
    {
        public event Action OnEventRaised;
        
        public void Raise()
        {
            OnEventRaised?.Invoke();
        }
        
        public void Subscribe(Action callback)
        {
            OnEventRaised += callback;
        }
        
        public void Unsubscribe(Action callback)
        {
            OnEventRaised -= callback;
        }
    }
}