using System;
using GTANetworkServer;
using GTANetworkShared;
using System.Linq;
using Newtonsoft.Json;
using Fuel2.Models;
using Fuel2.Enums;

namespace Fuel2
{

    public class FuelMain : Script
    {
        private const string FuelInfoDataKey = "FUEL_STATUS";

        private Config _config = Config.LoadConfig();
        private PumpHandler _pumpHandler;

        public FuelMain()
        {
            _pumpHandler = new PumpHandler(API);

            API.onResourceStart += OnResourceStart;
            API.onPlayerEnterVehicle += OnPlayerEnterVehicle;
        }

        private void OnResourceStart()
        {
            foreach (var player in API.getAllPlayers().Where(p => p.isInVehicle))
            {
                GetVehicleFuelStatus(player.vehicle.handle);
            }
        }

        private void OnPlayerEnterVehicle(Client player, NetHandle vehicle)
        {
            GetVehicleFuelStatus(vehicle);
        }

        private void SetVehicleFuelStatus(NetHandle vehicle, VehicleFuelStatus vehicleFuelStatus)
        {
            var json = JsonConvert.SerializeObject(vehicleFuelStatus);
            API.setEntitySyncedData(vehicle, FuelInfoDataKey, json);
        }

        private VehicleFuelStatus GetVehicleFuelStatus(NetHandle vehicle)
        {
            var currentFuelStatusJson = (string)API.getEntitySyncedData(vehicle, FuelInfoDataKey);
            VehicleFuelStatus currentFuelStatus = null;

            if (string.IsNullOrWhiteSpace(currentFuelStatusJson))
            {
                var vehicleFuelSpecification = _config.GetFuelSpecificationForVehicleHash((VehicleHash)API.getEntityModel(vehicle));
                currentFuelStatus = vehicleFuelSpecification.GetFuelStatusBasedOnSpecification();

                SetVehicleFuelStatus(vehicle, currentFuelStatus);
            }
            else
            {
                currentFuelStatus = JsonConvert.DeserializeObject<VehicleFuelStatus>(currentFuelStatusJson);
            }

            return currentFuelStatus;
        }

        public static void Log(string message)
        {
            API.shared.consoleOutput("Fuel: " + message);
        }

        #region Commands

        [Command]
        public void SetFuel(Client sender, double fuelLevel)
        {
            if (!sender.isInVehicle)
            {
                sender.sendChatMessage("Not in vehicle.");
                return;
            }

            var vehicle = API.getPlayerVehicle(sender);
            var currentFuelStatus = this.GetVehicleFuelStatus(vehicle);
            currentFuelStatus.CurrentFuel = (float)fuelLevel;

            SetVehicleFuelStatus(vehicle, currentFuelStatus);
        }

        [Command]
        public void SetCapacity(Client sender, double capacity)
        {
            if (!sender.isInVehicle)
            {
                sender.sendChatMessage("Not in vehicle.");
                return;
            }

            var fuelSpecification = _config.GetFuelSpecificationForVehicleHash((VehicleHash)API.getEntityModel(API.getPlayerVehicle(sender)));
            fuelSpecification.FuelCapacity = capacity;
            SetVehicleFuelStatus(API.getPlayerVehicle(sender), fuelSpecification.GetFuelStatusBasedOnSpecification());

            sender.sendChatMessage("Set fuel capacity to: " + capacity);

            _config.Save();
        }

        [Command]
        public void SetType(Client sender, int fuelType)
        {
            if (!sender.isInVehicle)
            {
                sender.sendChatMessage("Not in vehicle.");
                return;
            }

            var fuelSpecification = _config.GetFuelSpecificationForVehicleHash((VehicleHash)API.getEntityModel(API.getPlayerVehicle(sender)));
            fuelSpecification.FuelType = (FuelTypes)fuelType;
            SetVehicleFuelStatus(API.getPlayerVehicle(sender), fuelSpecification.GetFuelStatusBasedOnSpecification());

            sender.sendChatMessage("Set fuel type to: " + Enum.GetName(typeof(FuelTypes), (FuelTypes)fuelType));

            _config.Save();
        }

        [Command]
        public void SetMult(Client sender, double multiplier)
        {
            if (!sender.isInVehicle)
            {
                sender.sendChatMessage("Not in vehicle.");
                return;
            }

            var fuelSpecification = _config.GetFuelSpecificationForVehicleHash((VehicleHash)API.getEntityModel(API.getPlayerVehicle(sender)));
            fuelSpecification.ConsumptionMultiplier = multiplier;
            SetVehicleFuelStatus(API.getPlayerVehicle(sender), fuelSpecification.GetFuelStatusBasedOnSpecification());

            sender.sendChatMessage("Set consumption multiplier to: " + multiplier);

            _config.Save();
        }

        #endregion
    }
}
