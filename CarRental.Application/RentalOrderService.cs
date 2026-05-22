using System;
using System.Collections.Generic;
using System.Linq;
using CarRental.Domain;

namespace CarRental.Application
{
    public class RentalOrderService
    {
        private readonly IRentalRepository _repository;

        public RentalOrderService(IRentalRepository repository) => _repository = repository;

        public void SeedInitialVehicles()
        {
            if (!_repository.GetAllVehicles().Any())
            {
                _repository.AddVehicle(new PassengerCar(Guid.NewGuid(), "Toyota Camry", "AA1111BB", 1200, 5));
                _repository.AddVehicle(new Truck(Guid.NewGuid(), "Volvo FH16", "BC7777CB", 3000, 10.5));
            }
        }

        public IEnumerable<Vehicle> GetAvailableVehicles() 
            => _repository.GetAllVehicles().Where(v => v.IsAvailable);

        public Result<RentalOrder> CreateRentalOrder(string clientName, string licenseNum, string carPlate, int rentDays)
        {
            var vehicle = _repository.GetAllVehicles().FirstOrDefault(v => v.LicensePlate == carPlate);
            if (vehicle == null) return Result<RentalOrder>.Failure("Автомобіль з таким номером не знайдено.");
            if (!vehicle.IsAvailable) return Result<RentalOrder>.Failure("Цей автомобіль вже заброньовано.");

            try
            {
                var license = new DriverLicense(licenseNum, "B");
                var user = new User(Guid.NewGuid(), clientName, license);
                var period = new RentalPeriod(DateTime.Today, DateTime.Today.AddDays(rentDays));

                var order = new RentalOrder(Guid.NewGuid(), user, vehicle, period);
                _repository.AddOrder(order);

                return Result<RentalOrder>.Success(order);
            }
            catch (Exception ex)
            {
                return Result<RentalOrder>.Failure(ex.Message);
            }
        }
    }
}