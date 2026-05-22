// Файл: CarRental.Application/RentalService.cs
using System;
using System.Collections.Generic;
using CarRental.Domain;

namespace CarRental.Application
{
    public class RentalService
    {
        private readonly IRentalRepository _repo;
        public RentalService(IRentalRepository repo) => _repo = repo;

        public Vehicle RegisterNewPassengerCar(string brand, string plate, decimal price, int capacity)
        {
            // ЗАМЕНЕНО: вместо абстрактного Vehicle вызываем конкретный PassengerCar
            var car = new PassengerCar(Guid.NewGuid(), brand, plate, price, capacity);
            _repo.AddVehicle(car);
            return car;
        }
    }
}