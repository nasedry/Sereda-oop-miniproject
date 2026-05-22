using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarRental.Domain;

namespace CarRental.Application
{
    public class RentalOrderService
    {
        private readonly IRentalRepository _repository;
        private readonly IDataStore<Vehicle> _vehicleStore;
        private readonly IDataStore<RentalOrder> _orderStore;

        public RentalOrderService(IRentalRepository repository, IDataStore<Vehicle> vehicleStore, IDataStore<RentalOrder> orderStore)
        {
            _repository = repository;
            _vehicleStore = vehicleStore;
            _orderStore = orderStore;
        }

        public async Task LoadDataAsync()
        {
            var vehicles = await _vehicleStore.LoadAsync();
            foreach (var v in vehicles) _repository.AddVehicle(v);

            var orders = await _orderStore.LoadAsync();
            foreach (var o in orders) _repository.AddOrder(o);

            if (!vehicles.Any())
            {
                SeedInitialVehicles();
                await SaveDataAsync();
            }
        }

        public async Task SaveDataAsync()
        {
            await _vehicleStore.SaveAsync(_repository.GetAllVehicles().ToList());
            await _orderStore.SaveAsync(_repository.GetAllOrders().ToList());
        }

        private void SeedInitialVehicles()
        {
            _repository.AddVehicle(new PassengerCar(Guid.NewGuid(), "Toyota Camry", "AA1111BB", 1200, 5));
            _repository.AddVehicle(new PassengerCar(Guid.NewGuid(), "Tesla Model 3", "AA2222BB", 2000, 5));
            _repository.AddVehicle(new Truck(Guid.NewGuid(), "Volvo FH16", "BC7777CB", 3000, 12.0));
            _repository.AddVehicle(new Truck(Guid.NewGuid(), "Scania R500", "BC8888CB", 3500, 15.5));
        }

        public IEnumerable<Vehicle> GetAvailableVehicles() 
            => _repository.GetAllVehicles().Where(v => v.IsAvailable);

        // USE CASE 1: Створення замовлення зі стратегією знижки
        public Result<RentalOrder> CreateRentalOrder(string clientName, string licenseNum, string carPlate, int rentDays, IDiscountStrategy discountStrategy)
        {
            var vehicle = _repository.GetAllVehicles().FirstOrDefault(v => v.LicensePlate == carPlate);
            if (vehicle == null) return Result<RentalOrder>.Failure("Автомобіль з таким номером не знайдено.");
            if (!vehicle.IsAvailable) return Result<RentalOrder>.Failure("Цей автомобіль вже заброньовано.");
            if (rentDays <= 0) return Result<RentalOrder>.Failure("Кількість днів має бути більшою за 0.");

            try
            {
                var license = new DriverLicense(licenseNum, "B");
                var user = new User(Guid.NewGuid(), clientName, license);
                var period = new RentalPeriod(DateTime.Today, DateTime.Today.AddDays(rentDays));

                var order = new RentalOrder(Guid.NewGuid(), user, vehicle, period, discountStrategy);
                _repository.AddOrder(order);

                return Result<RentalOrder>.Success(order);
            }
            catch (Exception ex)
            {
                return Result<RentalOrder>.Failure(ex.Message);
            }
        }

        // USE CASE 2: Скасування замовлення
        public Result<bool> CancelOrder(Guid orderId)
        {
            var order = _repository.GetAllOrders().FirstOrDefault(o => o.Id == orderId);
            if (order == null) return Result<bool>.Failure("Замовлення не знайдено.");

            try
            {
                order.CancelOrder();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(ex.Message);
            }
        }

        // USE CASE 3: Повернення авто (Завершення замовлення зі штрафами)
        public Result<RentalOrder> CompleteOrder(Guid orderId, int actualDaysFromStart)
        {
            var order = _repository.GetAllOrders().FirstOrDefault(o => o.Id == orderId);
            if (order == null) return Result<RentalOrder>.Failure("Замовлення не знайдено.");

            try
            {
                DateTime actualDate = order.Period.StartDate.AddDays(actualDaysFromStart);
                order.CompleteOrder(actualDate);
                return Result<RentalOrder>.Success(order);
            }
            catch (Exception ex)
            {
                return Result<RentalOrder>.Failure(ex.Message);
            }
        }

        // --- LINQ АНАЛІТИКА ---

        // 1. Пошук активних замовлень
        public IEnumerable<RentalOrder> GetActiveOrders() 
            => _repository.GetAllOrders().Where(o => o.Status == OrderStatus.Active);

        // 2. Сортування автомобілів за ціною (від дешевих до дорогих)
        public IEnumerable<Vehicle> GetVehiclesSortedByPrice() 
            => _repository.GetAllVehicles().OrderBy(v => v.BasePricePerDay);

        // 3. Фільтрація замовлень конкретного клієнта
        public IEnumerable<RentalOrder> GetOrdersByClient(string name) 
            => _repository.GetAllOrders().Where(o => o.Customer.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        // 4. Агрегація: Загальний прибуток системи
        public decimal GetTotalRevenue() 
            => _repository.GetAllOrders().Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Active).Sum(o => o.TotalCost);
    }
}