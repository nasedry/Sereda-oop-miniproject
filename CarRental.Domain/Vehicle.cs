// Файл: CarRental.Domain/Vehicle.cs
namespace CarRental.Domain
{
    public class Vehicle
    {
        public Guid Id { get; private set; }
        public string Brand { get; private set; }
        public string LicensePlate { get; private set; }
        public decimal PricePerDay { get; private set; }
        public bool IsAvailable { get; private set; }

        public Vehicle(Guid id, string brand, string licensePlate, decimal pricePerDay)
        {
            if (string.IsNullOrWhiteSpace(brand))
                throw new ArgumentException("Марка машини не може бути порожньою.");
            if (string.IsNullOrWhiteSpace(licensePlate))
                throw new ArgumentException("Номерний знак не може бути порожнім.");
            if (pricePerDay <= 0)
                throw new ArgumentException("Ціна за день має бути більшою за 0.");

            Id = id;
            Brand = brand;
            LicensePlate = licensePlate;
            PricePerDay = pricePerDay;
            IsAvailable = true;
        }

        public void ChangeAvailability(bool available)
        {
            IsAvailable = available;
        }
    }
}