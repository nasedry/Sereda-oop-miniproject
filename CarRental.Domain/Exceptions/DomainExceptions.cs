using System;

namespace CarRental.Domain.Exceptions
{
    public class CarRentalDomainException : Exception
    {
        public CarRentalDomainException(string message) : base(message) { }
    }

    public class VehicleAlreadyRentedException : CarRentalDomainException
    {
        public VehicleAlreadyRentedException(string plate) 
            : base($"Транспортний засіб з номером {plate} вже орендований і недоступний.") { }
    }

    public class OrderStateTransitionException : CarRentalDomainException
    {
        public OrderStateTransitionException(string action, string status) 
            : base($"Неможливо виконати дію '{action}' для замовлення у статусі [{status}].") { }
    }
}