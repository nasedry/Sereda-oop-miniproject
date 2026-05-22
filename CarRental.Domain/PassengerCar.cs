namespace CarRental.Domain
{
    public class PassengerCar : Vehicle
    {
        // Перевір, щоб було саме так:
        public int SeatingCapacity { get; private set; }

        public PassengerCar(Guid id, string brand, string licensePlate, decimal basePricePerDay, int seatingCapacity)
            : base(id, brand, licensePlate, basePricePerDay)
        {
            if (seatingCapacity <= 0) 
                throw new ArgumentException("Місткість сидінь має бути більшою за нуль.");
                
            SeatingCapacity = seatingCapacity;
        }

        public override decimal CalculateActualRentCost(int days)
        {
            return BasePricePerDay * days;
        }
    }
}