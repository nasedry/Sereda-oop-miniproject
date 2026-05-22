using System;
using System.Text;
using System.Threading.Tasks;
using CarRental.Application;
using CarRental.Domain;
using CarRental.Infrastructure;

namespace CarRental.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            
            var repo = new InMemoryRentalRepository();
            var vehicleStore = new JsonDataStore<Vehicle>("vehicles.json");
            var orderStore = new JsonDataStore<RentalOrder>("orders.json");
            var service = new RentalOrderService(repo, vehicleStore, orderStore);

            try
            {
                await service.LoadDataAsync();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Помилка завантаження сховища: {ex.Message}");
                Console.ResetColor();
            }

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== СИСТЕМА ОРЕНДИ АВТО (ІТЕРАЦІЯ 2) ===");
                Console.WriteLine("1. Переглянути доступний автопарк");
                Console.WriteLine("2. Оформити нове замовлення");
                Console.WriteLine("3. Скасувати замовлення");
                Console.WriteLine("4. Завершити оренду (Повернути авто)");
                Console.WriteLine("5. Блок аналітики та LINQ-запитів");
                Console.WriteLine("6. Зберегти стан у файл та вийти");
                Console.Write("\nОберіть дію: ");

                string choice = Console.ReadLine() ?? "";
                switch (choice)
                {
                    case "1":
                        ShowVehicles(service);
                        break;
                    case "2":
                        CreateOrder(service);
                        break;
                    case "3":
                        CancelOrder(service);
                        break;
                    case "4":
                        CompleteOrder(service);
                        break;
                    case "5":
                        ShowAnalytics(service);
                        break;
                    case "6":
                        Console.WriteLine("\nЗбереження даних...");
                        await service.SaveDataAsync();
                        Console.WriteLine("Дані успішно збережено. Бувай!");
                        return;
                    default:
                        Console.WriteLine("Невірний вибір. Натисніть Enter...");
                        Console.ReadLine();
                        break;
                }
            }
        }

        static void ShowVehicles(RentalOrderService service)
        {
            Console.Clear();
            Console.WriteLine("--- Доступні автомобілі (відсортовані за ціною) ---");
            foreach (var v in service.GetVehiclesSortedByPrice())
            {
                string type = v is Truck ? "Вантажівка" : "Легкова";
                string status = v.IsAvailable ? "Вільна" : "Зайнята";
                Console.WriteLine($"- [{type}] {v.Brand} | Номер: {v.LicensePlate} | Ціна: {v.BasePricePerDay} грн/день | Статус: {status}");
            }
            Console.WriteLine("\nНатисніть будь-яку клавішу...");
            Console.ReadKey();
        }

        static void CreateOrder(RentalOrderService service)
        {
            Console.Clear();
            Console.WriteLine("--- Оформлення нового замовлення ---");
            Console.Write("Введіть ваше ім'я: "); string name = Console.ReadLine() ?? "";
            Console.Write("Номер посвідчення водія: "); string license = Console.ReadLine() ?? "";
            Console.Write("Номерний знак обраного авто: "); string plate = Console.ReadLine() ?? "";
            Console.Write("Кількість днів оренди: "); string daysInput = Console.ReadLine() ?? "1";
            if (!int.TryParse(daysInput, out int days)) days = 1;

            Console.WriteLine("\nОберіть тарифний план (Патерн Strategy):");
            Console.WriteLine("1. Стандартний (без знижок)");
            Console.WriteLine("2. Довгостроковий (15% знижки від 5 днів)");
            Console.WriteLine("3. Вікенд (5% знижки на короткі подорожі)");
            Console.Write("Ваш вибір: ");
            string stratChoice = Console.ReadLine() ?? "1";

            IDiscountStrategy strategy = stratChoice switch
            {
                "2" => new LongTermRentStrategy(),
                "3" => new WeekendStrategy(),
                _ => new RegularCustomerStrategy()
            };

            var result = service.CreateRentalOrder(name, license, plate, days, strategy);
            if (result.IsSuccess && result.Value != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nУспіх! Замовлення створено.");
                Console.WriteLine($"ID замовлення: {result.Value.Id}");
                Console.WriteLine($"Базова ціна: {result.Value.BaseCost} грн | Знижка: {result.Value.DiscountAmount} грн");
                Console.WriteLine($"Разом до сплати: {result.Value.TotalCost} грн.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Помилка: {result.ErrorMessage}");
            }
            Console.ResetColor();
            Console.ReadLine();
        }

        static void CancelOrder(RentalOrderService service)
        {
            Console.Clear();
            Console.WriteLine("--- Скасування активного замовлення ---");
            Console.Write("Введіть ID замовлення: ");
            if (!Guid.TryParse(Console.ReadLine(), out Guid id))
            {
                Console.WriteLine("Некоректний формат ID.");
                Console.ReadLine();
                return;
            }

            var result = service.CancelOrder(id);
            if (result.IsSuccess)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Замовлення успішно скасовано. Статус авто змінено на 'Вільна'.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Помилка скасування: {result.ErrorMessage}");
            }
            Console.ResetColor();
            Console.ReadLine();
        }

        static void CompleteOrder(RentalOrderService service)
        {
            Console.Clear();
            Console.WriteLine("--- Завершення оренди та повернення авто ---");
            Console.Write("Введіть ID замовлення: ");
            if (!Guid.TryParse(Console.ReadLine(), out Guid id))
            {
                Console.WriteLine("Некоректний формат ID.");
                Console.ReadLine();
                return;
            }

            Console.Write("Скільки днів авто фактично було в оренді від старту: ");
            if (!int.TryParse(Console.ReadLine(), out int actualDays)) actualDays = 1;

            var result = service.CompleteOrder(id, actualDays);
            if (result.IsSuccess && result.Value != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nАвтомобіль успішно повернуто в автопарк!");
                if (result.Value.PenaltyAmount > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Увага! Нараховано штраф за прострочення терміну: {result.Value.PenaltyAmount} грн.");
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Фінальна вартість до сплати: {result.Value.TotalCost} грн.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Помилка завершення оренди: {result.ErrorMessage}");
            }
            Console.ResetColor();
            Console.ReadLine();
        }

        static void ShowAnalytics(RentalOrderService service)
        {
            Console.Clear();
            Console.WriteLine("=== БЛОК ЛІНІЙНОЇ АНАЛІТИКИ (LINQ) ===");
            Console.WriteLine($"1. Загальний фінансовий обіг системи: {service.GetTotalRevenue()} грн.");
            
            Console.WriteLine("\n2. Кількість активних замовлень на цей момент:");
            var active = service.GetActiveOrders();
            Console.WriteLine($"Всього активних оренд: {active.Count()}");
            foreach (var o in active)
            {
                Console.WriteLine($"   - Замовлення {o.Id} | Клієнт: {o.Customer.Name} | Авто: {o.RentedVehicle.Brand}");
            }

            Console.WriteLine("\n3. Пошук замовлень за іменем клієнта:");
            Console.Write("Введіть ім'я для фільтру: ");
            string filterName = Console.ReadLine() ?? "";
            var clientOrders = service.GetOrdersByClient(filterName);
            Console.WriteLine($"Знайдено замовлень ({clientOrders.Count()}):");
            foreach (var o in clientOrders)
            {
                Console.WriteLine($"   - ID: {o.Id} | Статус: {o.Status} | Сума: {o.TotalCost} грн");
            }

            Console.WriteLine("\nНатисніть будь-яку клавішу...");
            Console.ReadKey();
        }
    }
}