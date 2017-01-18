using Fuel.resources.fuel2.Server.Helpers;
using GTANetworkServer;
using GTANetworkShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fuel2
{
    public class PumpHandler
    {
        private const int PumpNozzleDistanceCheckTimeout = 1000;
        private const int PumpNozzleDisconnectDistance = 3;
        private const string UsePumpEvent = "USE_PUMP";

        private EventHandlingHelper _eventHandlingHelper;
        private Dictionary<Client, UsingPumpInfo> _playersUsingPump = new Dictionary<Client, UsingPumpInfo>();
        private int _lastPumpNozzleDistanceCheckTime = Environment.TickCount;

        public PumpHandler(API apiInstance)
        {
            _eventHandlingHelper = new EventHandlingHelper(apiInstance);
            _eventHandlingHelper.AddEventHandler(UsePumpEvent, HandleUsePumpEvent);

            apiInstance.onUpdate += OnUpdate;
            apiInstance.onResourceStop += OnResourceStop;
            apiInstance.onPlayerEnterVehicle += OnPlayerEnterVehicle;
        }

        private void OnResourceStop()
        {
            _eventHandlingHelper.Dispose();
            _playersUsingPump.Keys.ToList().ForEach(DisconnectPlayerFromPump);
        }

        private void OnUpdate()
        {
            if (Environment.TickCount - _lastPumpNozzleDistanceCheckTime >= PumpNozzleDistanceCheckTimeout)
            {
                _lastPumpNozzleDistanceCheckTime = Environment.TickCount;
                CheckNozzleDistances();
            }
        }

        private void OnPlayerEnterVehicle(Client player, NetHandle vehicle)
        {
            DisconnectPlayerFromPump(player);
        }

        private void HandleUsePumpEvent(Client sender, string eventName, params object[] arguments)
        {
            TogglePlayerConnectedToPump(sender, (Vector3)arguments[0]);
        }

        private void CheckNozzleDistances()
        {
            foreach (var playerUsingPump in _playersUsingPump)
            {
                var distanceFromPump = playerUsingPump.Key.position.DistanceTo(playerUsingPump.Value.PumpPosition);
                if (distanceFromPump <= PumpNozzleDisconnectDistance) continue;

                playerUsingPump.Key.sendChatMessage("You went too far from the pump!");

                DisconnectPlayerFromPump(playerUsingPump.Key);
            }
        }
        
        public void ConnectPlayerToPump(Client player, Vector3 pumpPosition)
        {
            if (_playersUsingPump.ContainsKey(player)) return;

            var nozzle = API.shared.createObject(API.shared.getHashKey("prop_cs_fuel_nozle"), player.position, new Quaternion());
            API.shared.attachEntityToEntity(nozzle, player, "SKEL_R_Hand", new Vector3(0.12, 0.05, -0.01), new Vector3(180, 90, 90));

            _playersUsingPump.Add(player, new UsingPumpInfo
            {
                NozzleObjectHandle = nozzle,
                PumpPosition = pumpPosition
            });
        }

        public void DisconnectPlayerFromPump(Client player)
        {
            if (!_playersUsingPump.ContainsKey(player)) return;

            var usingPumpInfo = _playersUsingPump[player];
            API.shared.deleteEntity(usingPumpInfo.NozzleObjectHandle);
            _playersUsingPump.Remove(player);
        }

        public void TogglePlayerConnectedToPump(Client player, Vector3 pumpPosition)
        {
            if (_playersUsingPump.ContainsKey(player))
            {
                DisconnectPlayerFromPump(player);
                return;
            }

            ConnectPlayerToPump(player, pumpPosition);
        }
    }
}
