// Файл: CarRental.Application/IVehicleRepository.cs
using CarRental.Domain;

namespace CarRental.Application
{
    public interface IVehicleRepository
    {
        void Add(Vehicle vehicle);
        IEnumerable<Vehicle> GetAll();
        Vehicle GetById(Guid id);
    }
}