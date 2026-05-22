// Файл: CarRental.Domain/Vehicle.cs
using System;

namespace CarRental.Domain
{
    public abstract class Vehicle
    {
        public Guid Id { get; private set; }
        public string Brand { get; private set; }
        public string LicensePlate { get; private set; }
        public decimal BasePricePerDay { get; private set; }
        public bool IsAvailable { get; private set; }

        protected Vehicle(Guid id, string brand, string licensePlate, decimal basePricePerDay)
        {
            if (string.IsNullOrWhiteSpace(brand)) throw new ArgumentException("Марка не може бути порожньою.");
            if (string.IsNullOrWhiteSpace(licensePlate)) throw new ArgumentException("Номерний знак не може бути порожнім.");
            if (basePricePerDay <= 0) throw new ArgumentException("Базова ціна повинна бути більшою за 0.");

            Id = id;
            Brand = brand;
            LicensePlate = licensePlate;
            BasePricePerDay = basePricePerDay;
            IsAvailable = true;
        }

        // Абстрактний метод для поліморфного розрахунку вартості
        public abstract decimal CalculateActualRentCost(int days);

        public void MarkAsRented() => IsAvailable = false;
        public void MarkAsAvailable() => IsAvailable = true;
        public void Release() => IsAvailable = true;
    }
}