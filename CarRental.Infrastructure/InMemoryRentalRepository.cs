using System.Collections.Generic;
using CarRental.Domain;

namespace CarRental.Infrastructure
{
    public class InMemoryRentalRepository : IRentalRepository
    {
        private readonly List<Vehicle> _vehicles = new();
        private readonly List<RentalOrder> _orders = new();

        public void AddVehicle(Vehicle vehicle)
        {
            _vehicles.Add(vehicle);
        }

        public IEnumerable<Vehicle> GetAllVehicles()
        {
            return _vehicles;
        }

        public void AddOrder(RentalOrder order)
        {
            _orders.Add(order);
        }

        // Новий метод для Ітерації 2, який виправляє помилку CS1061 у сервісі
        public IEnumerable<RentalOrder> GetAllOrders()
        {
            return _orders;
        }
    }
}