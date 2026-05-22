using System;

namespace CarRental.Domain
{
    public enum OrderStatus { Active, Cancelled, Completed }

    public class RentalOrder
    {
        public Guid Id { get; private set; }
        public User Customer { get; private set; }
        public Vehicle RentedVehicle { get; private set; }
        public RentalPeriod Period { get; private set; }
        public decimal BaseCost { get; private set; }
        public decimal DiscountAmount { get; private set; }
        public decimal PenaltyAmount { get; private set; }
        public decimal TotalCost => BaseCost - DiscountAmount + PenaltyAmount;
        public OrderStatus Status { get; private set; }

        public RentalOrder(Guid id, User customer, Vehicle rentedVehicle, RentalPeriod period, IDiscountStrategy discountStrategy)
        {
            Id = id;
            Customer = customer ?? throw new ArgumentNullException(nameof(customer));
            RentedVehicle = rentedVehicle ?? throw new ArgumentNullException(nameof(rentedVehicle));
            Period = period ?? throw new ArgumentNullException(nameof(period));
            Status = OrderStatus.Active;

            BaseCost = RentedVehicle.CalculateActualRentCost(Period.TotalDays);
            DiscountAmount = discountStrategy.CalculateDiscount(BaseCost, Period.TotalDays);
            PenaltyAmount = 0;

            RentedVehicle.MarkAsRented();
        }

        // Для десеріалізації з JSON
        public RentalOrder(Guid id, User customer, Vehicle rentedVehicle, RentalPeriod period, decimal baseCost, decimal discountAmount, decimal penaltyAmount, OrderStatus status)
        {
            Id = id;
            Customer = customer;
            RentedVehicle = rentedVehicle;
            Period = period;
            BaseCost = baseCost;
            DiscountAmount = discountAmount;
            PenaltyAmount = penaltyAmount;
            Status = status;
        }

        public void CancelOrder()
        {
            if (Status != OrderStatus.Active)
                throw new InvalidOperationException("Можна скасувати тільки активне замовлення.");

            Status = OrderStatus.Cancelled;
            RentedVehicle.Release();
        }

        public void CompleteOrder(DateTime actualReturnDate)
        {
            if (Status != OrderStatus.Active)
                throw new InvalidOperationException("Тільки активне замовлення можна завершити.");

            Status = OrderStatus.Completed;
            RentedVehicle.Release();

            if (actualReturnDate > Period.EndDate)
            {
                int overdueDays = (actualReturnDate - Period.EndDate).Days;
                PenaltyAmount = overdueDays * (RentedVehicle.BasePricePerDay * 1.5m); // Штраф 150% за день прострочення
            }
        }
    }
}