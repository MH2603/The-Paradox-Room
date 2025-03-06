using System;
using UnityEngine;

namespace MH.Core.EventHub
{
    
    public struct TextEvent : IEvent
    {
        public string text;
    }
    
    
    public class EventHubExam : MonoBehaviour
    {
        private EventHub _eventHub;
    
        public void Start()
        {
            _eventHub = new EventHub();
        
            _eventHub.AddListener<TextEvent>(HandleEvent);
            _eventHub.AddListener<TextEvent>(HandleEvent3);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _eventHub.DispatchEvent(new TextEvent { text = "Hello World!" });
            }
        }

        private void HandleEvent()
        {
            Debug.Log("Recieved event!");
        }

        private void HandleEvent2(IEvent eventData)
        {
            Debug.Log("Recieved 02 event!");
        }
        private void HandleEvent3(TextEvent eventData)
        {
            Debug.Log("Recieved 03 event!" + eventData.text);
        }
    
    }
}



