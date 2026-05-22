// Файл: CarRental.Infrastructure/InMemoryRentalRepository.cs
using System.Collections.Generic;
using CarRental.Application;
using CarRental.Domain;

namespace CarRental.Infrastructure
{
    public class InMemoryRentalRepository : IRentalRepository
    {
        // Використовуємо List<T> для простого збереження послідовності сутностей
        private readonly List<Vehicle> _vehicles = new();
        private readonly List<RentalOrder> _orders = new();

        public void AddVehicle(Vehicle vehicle) => _vehicles.Add(vehicle);
        public IEnumerable<Vehicle> GetAllVehicles() => _vehicles;
        public void AddOrder(RentalOrder order) => _orders.Add(order);
    }
}