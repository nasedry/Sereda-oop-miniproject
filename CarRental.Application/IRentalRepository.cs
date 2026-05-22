// Файл: CarRental.Application/IRentalRepository.cs
using System.Collections.Generic;
using CarRental.Domain;

namespace CarRental.Application
{
    public interface IRentalRepository
    {
        void AddVehicle(Vehicle vehicle);
        IEnumerable<Vehicle> GetAllVehicles();
        void AddOrder(RentalOrder order);
    }
}