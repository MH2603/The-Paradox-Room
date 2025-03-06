using System;
using System.Collections.Generic;

namespace  MH.Core.EventHub
{
    // we refection to event hub by ServiceLocator or create Singleton class
    public class EventHub
    {
        // Dictionary to hold event listeners with a more flexible delegate type.
        private Dictionary<Type, HashSet<Delegate>> _eventListeners = new();

        public EventHub()
        {
            _eventListeners = new();
        }

        // Add a listener with any type of callback signature
        public void AddListener<T>(Action<T> callBack) where T : IEvent
        {
            if (callBack == null) return;

            if (!_eventListeners.ContainsKey(typeof(T)))
            {
                _eventListeners[typeof(T)] = new HashSet<Delegate>();
            }

            _eventListeners[typeof(T)].Add(callBack);
        }

        // Overload to add a listener without parameters
        public void AddListener<T>(Action callBack)
        {
            if (callBack == null) return;

            if (!_eventListeners.ContainsKey(typeof(T)))
            {
                _eventListeners[typeof(T)] = new HashSet<Delegate>();
            }

            _eventListeners[typeof(T)].Add(callBack);
        }

        // Remove a listener with any type of callback signature
        public void RemoveListener<T>(Action<T> callBack) where T : IEvent
        {
            if (callBack == null) return;

            if (_eventListeners.TryGetValue(typeof(T), out var listeners))
            {
                listeners.Remove(callBack);
                if (listeners.Count == 0) _eventListeners.Remove(typeof(T));
            }
        }

        // Overload to remove a listener without parameters
        /*public void RemoveListener<T>(Action callBack)
        {
            if ( callBack == null)
            {
                return;
            }

            if (_eventListeners.ContainsKey(typeof(T)))
            {
                _eventListeners[typeof(T)].Remove(callBack);

                // Clean up the list if it's empty
                if (_eventListeners[typeof(T)].Count == 0)
                {
                    _eventListeners.Remove(typeof(T));
                }
            }
        }*/

        // Dispatch event with no data
        public void DispatchEvent<T>(T eventData) where T : IEvent
        {
            if (_eventListeners.TryGetValue(typeof(T), out var listeners))
            {
                foreach (var action in listeners)
                {
                    (action as Action<T>)?.Invoke(eventData);
                    (action as Action)?.Invoke();
                }
            }
        }
        
    }
}

