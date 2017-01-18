using GTANetworkServer;
using System;
using System.Collections.Generic;
using ServerEventTrigger = GTANetworkServer.API.ServerEventTrigger;

namespace Fuel.resources.fuel2.Server.Helpers
{
    public class EventHandlingHelper : IDisposable
    {
        private API _apiInstance;
        private bool _ignoreEventNameCase;
        private Dictionary<string, List<ServerEventTrigger>> _eventHandlers = new Dictionary<string, List<ServerEventTrigger>>();

        public EventHandlingHelper(API apiInstance, bool ignoreEventNameCase = true)
        {
            _apiInstance = apiInstance;
            _ignoreEventNameCase = ignoreEventNameCase;

            _apiInstance.onClientEventTrigger += OnClientEventTrigger;
        }

        private string GetCaseSensitiveEventName(string eventName)
        {
            return _ignoreEventNameCase ? eventName.ToLowerInvariant() : eventName;
        }

        public void AddEventHandler(string eventName, ServerEventTrigger eventHandler)
        {
            eventName = GetCaseSensitiveEventName(eventName);

            if (!_eventHandlers.ContainsKey(eventName))
            {
                _eventHandlers.Add(eventName, new List<ServerEventTrigger> { eventHandler });
                return;
            }

            _eventHandlers[eventName].Add(eventHandler);
        }

        public void RemoveEventHandler(string eventName)
        {
            eventName = GetCaseSensitiveEventName(eventName);

            if (!_eventHandlers.ContainsKey(eventName)) return;

            _eventHandlers.Remove(eventName);
        }

        public void RemoveEventHandler(string eventName, ServerEventTrigger eventHandler)
        {
            eventName = GetCaseSensitiveEventName(eventName);

            if (!_eventHandlers.ContainsKey(eventName)) return;

            var eventHandlers = _eventHandlers[eventName];

            if (!eventHandlers.Contains(eventHandler)) return;

            eventHandlers.Remove(eventHandler);
        }

        private void OnClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            eventName = GetCaseSensitiveEventName(eventName);

            if (!_eventHandlers.ContainsKey(eventName)) return;

            _eventHandlers[eventName].ForEach(handler => handler.Invoke(sender, eventName, arguments));
        }

        public void Dispose()
        {
            _apiInstance.onClientEventTrigger -= OnClientEventTrigger;
        }
    }
}
