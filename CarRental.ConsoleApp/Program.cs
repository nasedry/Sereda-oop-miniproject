using System;
using System.Text;
using CarRental.Application;
using CarRental.Infrastructure;

namespace CarRental.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            
            // Налаштовуємо залежності (DI вручну)
            var repository = new InMemoryVehicleRepository();
            var service = new RentalService(repository);

            Console.WriteLine("=== СИСТЕМА БРОНЮВАННЯ АВТО (Лаб 34) ===");
            
            // Створюємо перше авто через сервіс (Вертикальний зріз)
            var car = service.RegisterNewVehicle("Toyota Camry", "AA1111BB", 1200);
            Console.WriteLine($"Успішно додано автомобіль: {car.Brand} ({car.LicensePlate}) - {car.PricePerDay} грн/день.");

            Console.WriteLine("\nСписок доступних автомобілів в системі:");
            foreach (var v in service.GetAvailableCars())
            {
                Console.WriteLine($"- {v.Brand} | Номер: {v.LicensePlate} | Доступність: {v.IsAvailable}");
            }
            
            Console.WriteLine("\nНатисніть будь-яку клавішу для виходу...");
            Console.ReadKey();
        }
    }
}
