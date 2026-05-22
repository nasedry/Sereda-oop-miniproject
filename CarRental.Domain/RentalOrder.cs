// Файл: D:\CarRentalProject\CarRental.Domain\RentalOrder.cs
using System;

namespace CarRental.Domain // <- Має бути ідентичним до namespace у User.cs
{
    public class RentalOrder
    {
        public Guid Id { get; private set; }
        public User Customer { get; private set; } // Тепер компілятор знайде тип User
        public Vehicle RentedVehicle { get; private set; }
        public RentalPeriod Period { get; private set; }
        public decimal TotalCost { get; private set; }

        public RentalOrder(Guid id, User customer, Vehicle rentedVehicle, RentalPeriod period)
        {
            Id = id;
            Customer = customer ?? throw new ArgumentNullException(nameof(customer));
            RentedVehicle = rentedVehicle ?? throw new ArgumentNullException(nameof(rentedVehicle));
            Period = period ?? throw new ArgumentNullException(nameof(period));
            
            TotalCost = rentedVehicle.CalculateActualRentCost(period.TotalDays);
            rentedVehicle.MarkAsRented();
        }
    }
}