// Файл: D:\CarRentalProject\CarRental.Domain\User.cs
using System;

namespace CarRental.Domain // <- ПЕРЕВІРТЕ ЦЕЙ РЯДОК
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public DriverLicense License { get; private set; }

        public User(Guid id, string name, DriverLicense license)
        {
            Id = id;
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Ім'я не може бути порожнім") : name;
            License = license ?? throw new ArgumentNullException(nameof(license));
        }
    }
}