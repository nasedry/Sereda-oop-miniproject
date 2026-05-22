using System;
using System.Text;
using CarRental.Application;
using CarRental.Domain;
using CarRental.Infrastructure;

namespace CarRental.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            var repo = new InMemoryRentalRepository();
            var service = new RentalOrderService(repo);
            service.SeedInitialVehicles();

            Console.WriteLine("=== ВЕРТИКАЛЬНИЙ ЗРІЗ: ОФОРМЛЕННЯ БРОНЮВАННЯ (Лаб 34) ===");
            
            Console.WriteLine("\nДоступний автопарк:");
            foreach (var v in service.GetAvailableVehicles())
            {
                string type = v is Truck ? "Вантажівка" : "Легкова";
                Console.WriteLine($"- [{type}] {v.Brand} | Номер: {v.LicensePlate} | Базова ціна: {v.BasePricePerDay} грн/день");
            }

            Console.WriteLine("\n--- Оформлення нового замовлення ---");
            
            Console.Write("Введіть ваше ім'я: "); 
            string name = Console.ReadLine() ?? ""; 
            
            Console.Write("Введіть номер посвідчення: "); 
            string license = Console.ReadLine() ?? "";
            
            Console.Write("Введіть номерний знак обраного авто: "); 
            string plate = Console.ReadLine() ?? "";
            
            Console.Write("Кількість днів оренди: "); 
            string daysInput = Console.ReadLine() ?? "1";
            
            if (!int.TryParse(daysInput, out int days)) days = 1;

            var result = service.CreateRentalOrder(name, license, plate, days);

            if (result.IsSuccess && result.Value != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nУспіх! Замовлення {result.Value.Id} створено.");
                Console.WriteLine($"Разом до сплати: {result.Value.TotalCost} грн. Статус авто змінено на 'Зайнято'.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nПомилка бронювання: {result.ErrorMessage}");
                Console.ResetColor();
            }

            Console.WriteLine("\nНатисніть будь-яку клавішу для виходу...");
            Console.ReadKey();
        }
    }
}