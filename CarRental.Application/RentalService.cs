// Файл: CarRental.Application/RentalService.cs
using CarRental.Domain;

namespace CarRental.Application
{
    public class RentalService
    {
        private readonly IVehicleRepository _vehicleRepository;

        public RentalService(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        public Vehicle RegisterNewVehicle(string brand, string licensePlate, decimal price)
        {
            var vehicle = new Vehicle(Guid.NewGuid(), brand, licensePlate, price);
            _vehicleRepository.Add(vehicle);
            return vehicle;
        }

        public IEnumerable<Vehicle> GetAvailableCars()
        {
            return _vehicleRepository.GetAll();
        }
    }
}