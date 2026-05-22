using CarRental.Application;
using CarRental.Domain;

namespace CarRental.Infrastructure
{
    public class InMemoryVehicleRepository : IVehicleRepository
    {
        private readonly List<Vehicle> _vehicles = new();

        public void Add(Vehicle vehicle)
        {
            _vehicles.Add(vehicle);
        }

        public IEnumerable<Vehicle> GetAll()
        {
            return _vehicles;
        }

        public Vehicle GetById(Guid id)
        {
            return _vehicles.FirstOrDefault(v => v.Id == id) 
                   ?? throw new KeyNotFoundException("Автомобіль не знайдено.");
        }
    }
}