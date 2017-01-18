using Fuel2.Enums;
using GTANetworkShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Fuel2.Models
{
    [Serializable]
    public class Config
    {
        private const string ConfigPath = "resources/fuel/Config/VehicleFuelSpecifications.xml";
        private const double DefaultFuelCapacity = 40;
        private const double DefaultConsumptionMultiplier = 1;
        private const FuelTypes DefaultFuelType = FuelTypes.Gasoline;

        public List<VehicleFuelSpecification> VehicleFuelSpecifications { get; set; }

        private Dictionary<VehicleHash, VehicleFuelSpecification> _fuelSpecificationCache = new Dictionary<VehicleHash, VehicleFuelSpecification>();

        public Config()
        {
            VehicleFuelSpecifications = new List<VehicleFuelSpecification>();
        }

        public VehicleFuelSpecification GetFuelSpecificationForVehicleHash(VehicleHash vehicleHash)
        {
            if (_fuelSpecificationCache.ContainsKey(vehicleHash)) return _fuelSpecificationCache[vehicleHash];

            var modelName = Enum.GetName(typeof(VehicleHash), vehicleHash).ToLowerInvariant();

            var specification = VehicleFuelSpecifications.FirstOrDefault(s => string.Equals(s.ModelName, modelName, StringComparison.OrdinalIgnoreCase));

            if (specification == null)
            {
                FuelMain.Log("No fuel specification found for vehicle model: " + modelName);
            }
            else
            {
                _fuelSpecificationCache.Add(vehicleHash, specification);
                return specification;
            }

            VehicleFuelSpecifications.Add(new VehicleFuelSpecification { FuelCapacity = DefaultFuelCapacity, ConsumptionMultiplier = DefaultConsumptionMultiplier, FuelType = DefaultFuelType, ModelName = modelName });

            this.Save();

            return GetFuelSpecificationForVehicleHash(vehicleHash);
        }

        private static XmlSerializer GetSerializer()
        {
            return new XmlSerializer(typeof(Config));
        }

        public void Save()
        {
            try
            {
                using (var writer = File.CreateText(ConfigPath))
                {
                    GetSerializer().Serialize(writer, this);
                    writer.Flush();
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                FuelMain.Log("Failed to save config: " + ex);
            }
        }

        private static Config CreateNewConfig()
        {
            var emptyConfig = new Config();

            emptyConfig.VehicleFuelSpecifications.Add(new VehicleFuelSpecification { FuelCapacity = 40, FuelType = FuelTypes.Gasoline, ConsumptionMultiplier = 2, ModelName = "t20" });

            emptyConfig.Save();

            return emptyConfig;
        }

        public static Config LoadConfig()
        {
            if (!File.Exists(ConfigPath)) return CreateNewConfig();

            using (var reader = File.OpenRead(ConfigPath))
            {
                try
                {
                    return (Config)GetSerializer().Deserialize(reader) ?? CreateNewConfig();
                }
                catch (Exception ex)
                {
                    FuelMain.Log("Failed to load config: " + ex);
                }
            }

            return CreateNewConfig();
        }
    }
}
