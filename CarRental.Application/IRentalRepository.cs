using System.Collections.Generic;

namespace CarRental.Domain
{
    public interface IRentalRepository
    {
        IEnumerable<Vehicle> GetAllVehicles();
        void AddVehicle(Vehicle vehicle);
        void AddOrder(RentalOrder order);
        
        // ДОДАЙ ЦЕЙ РЯДОК:
        IEnumerable<RentalOrder> GetAllOrders();
    }
}