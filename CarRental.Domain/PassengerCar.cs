// Файл: CarRental.Domain/PassengerCar.cs
namespace CarRental.Domain
{
    public class PassengerCar : Vehicle
    {
        public int PassengerCapacity { get; private set; }

        public PassengerCar(Guid id, string brand, string licensePlate, decimal basePricePerDay, int passengerCapacity)
            : base(id, brand, licensePlate, basePricePerDay)
        {
            if (passengerCapacity <= 0) throw new ArgumentException("Місткість пасажирів має бути більшою за 0.");
            PassengerCapacity = passengerCapacity;
        }

        // Легкові авто мають фіксовану ціну
        public override decimal CalculateActualRentCost(int days) => BasePricePerDay * days;
    }
}