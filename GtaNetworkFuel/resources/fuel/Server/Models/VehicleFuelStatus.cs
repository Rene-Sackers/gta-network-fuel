using Fuel2.Enums;

namespace Fuel2.Models
{
    public class VehicleFuelStatus
    {
        public float CurrentFuel { get; set; }

        public double FuelCapacity { get; set; }

        public double ConsumptionMultiplier { get; set; }

        public FuelTypes FuelType { get; set; }

        public VehicleFuelStatus()
        {
            CurrentFuel = 100f;
            FuelCapacity = 100;
            ConsumptionMultiplier = 1;
        }
    }
}
