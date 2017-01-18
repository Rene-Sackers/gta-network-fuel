using Fuel2.Enums;
using System;
using System.Xml.Serialization;

namespace Fuel2.Models
{
    [Serializable]
    public class VehicleFuelSpecification
    {
        [XmlAttribute]
        public string ModelName { get; set; }

        [XmlAttribute]
        public double FuelCapacity { get; set; }

        [XmlAttribute]
        public double ConsumptionMultiplier { get; set; }

        [XmlAttribute]
        public FuelTypes FuelType { get; set; }

        public VehicleFuelSpecification()
        {
            FuelCapacity = 100;
            ConsumptionMultiplier = 1;
        }

        public VehicleFuelSpecification Clone()
        {
            return new VehicleFuelSpecification
            {
                ConsumptionMultiplier = this.ConsumptionMultiplier,
                FuelCapacity = this.FuelCapacity,
                FuelType = this.FuelType,
                ModelName = this.ModelName
            };
        }

        public VehicleFuelStatus GetFuelStatusBasedOnSpecification()
        {
            return new VehicleFuelStatus
            {
                ConsumptionMultiplier = this.ConsumptionMultiplier,
                CurrentFuel = (float)this.FuelCapacity,
                FuelCapacity = this.FuelCapacity,
                FuelType = this.FuelType
            };
        }
    }
}
