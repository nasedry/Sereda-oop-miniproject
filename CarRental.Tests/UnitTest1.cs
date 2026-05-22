using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using CarRental.Domain;
using CarRental.Application;
using CarRental.Infrastructure;

namespace CarRental.Tests
{
    public class BusinessAndPersistenceTests
    {
        private readonly InMemoryRentalRepository _repo;
        private readonly PassengerCar _testCar;
        private readonly User _testUser;

        public BusinessAndPersistenceTests()
        {
            _repo = new InMemoryRentalRepository();
            _testCar = new PassengerCar(Guid.NewGuid(), "Test Brand", "AA0000BB", 1000, 5);
            _testUser = new User(Guid.NewGuid(), "Tester", new DriverLicense("111", "B"));
            _repo.AddVehicle(_testCar);
        }

        // --- ТЕСТИ ДОМЕННИХ ІНВАРІАНТІВ ---
        [Fact]
        public void RentalOrder_ShouldInitWithActiveStatus()
        {
            var period = new RentalPeriod(DateTime.Today, DateTime.Today.AddDays(2));
            var order = new RentalOrder(Guid.NewGuid(), _testUser, _testCar, period, new RegularCustomerStrategy());
            Assert.Equal(OrderStatus.Active, order.Status);
        }

        [Fact]
        public void OrderCancel_ShouldReleaseVehicleAndChangeStatus()
        {
            var period = new RentalPeriod(DateTime.Today, DateTime.Today.AddDays(2));
            var order = new RentalOrder(Guid.NewGuid(), _testUser, _testCar, period, new RegularCustomerStrategy());
            
            order.CancelOrder();

            Assert.Equal(OrderStatus.Cancelled, order.Status);
            Assert.True(_testCar.IsAvailable);
        }

        [Fact]
        public void CancelAlreadyCancelledOrder_ShouldThrowException()
        {
            var period = new RentalPeriod(DateTime.Today, DateTime.Today.AddDays(2));
            var order = new RentalOrder(Guid.NewGuid(), _testUser, _testCar, period, new RegularCustomerStrategy());
            order.CancelOrder();

            Assert.Throws<InvalidOperationException>(() => order.CancelOrder());
        }

        // --- ТЕСТИ ПАТЕРНУ STRATEGY (БІЗНЕС-ПРАВИЛА) ---
        [Fact]
        public void LongTermStrategy_ShouldApplyDiscount_WhenDaysRentEqualOrMoreThan5()
        {
            var period = new RentalPeriod(DateTime.Today, DateTime.Today.AddDays(5));
            var order = new RentalOrder(Guid.NewGuid(), _testUser, _testCar, period, new LongTermRentStrategy());

            decimal expectedBase = 1000 * 5;
            decimal expectedDiscount = expectedBase * 0.15m;

            Assert.Equal(expectedDiscount, order.DiscountAmount);
            Assert.Equal(expectedBase - expectedDiscount, order.TotalCost);
        }

        [Fact]
        public void WeekendStrategy_ShouldApplyDiscount_OnShortRent()
        {
            var period = new RentalPeriod(DateTime.Today, DateTime.Today.AddDays(2));
            var order = new RentalOrder(Guid.NewGuid(), _testUser, _testCar, period, new WeekendStrategy());

            Assert.Equal(2000 * 0.05m, order.DiscountAmount);
        }

        [Fact]
        public void CompleteOrder_WithOverdue_ShouldApplyHeavyPenalty()
        {
            var period = new RentalPeriod(DateTime.Today, DateTime.Today.AddDays(2));
            var order = new RentalOrder(Guid.NewGuid(), _testUser, _testCar, period, new RegularCustomerStrategy());

            order.CompleteOrder(DateTime.Today.AddDays(4));

            decimal expectedPenalty = 2 * (1000 * 1.5m);
            Assert.Equal(expectedPenalty, order.PenaltyAmount);
            Assert.Equal(OrderStatus.Completed, order.Status);
        }

        // --- ТЕСТИ СЕРВІСІВ (USE CASES) ---
        [Fact]
        public void Service_CancelOrder_ShouldReturnSuccess()
        {
            var orderStore = new JsonDataStore<RentalOrder>("test_orders.json");
            var vehicleStore = new JsonDataStore<Vehicle>("test_vehicles.json");
            var service = new RentalOrderService(_repo, vehicleStore, orderStore);

            var orderResult = service.CreateRentalOrder("Tester", "111", "AA0000BB", 3, new RegularCustomerStrategy());
            
            Assert.True(orderResult.IsSuccess);
            Assert.NotNull(orderResult.Value);

            var cancelResult = service.CancelOrder(orderResult.Value!.Id);

            Assert.True(cancelResult.IsSuccess);
            Assert.True(_testCar.IsAvailable);
        }

        [Fact]
        public void Service_CompleteOrder_ShouldSuccessAndCalculateFinalRevenue()
        {
            var orderStore = new JsonDataStore<RentalOrder>("test_orders.json");
            var vehicleStore = new JsonDataStore<Vehicle>("test_vehicles.json");
            var service = new RentalOrderService(_repo, vehicleStore, orderStore);

            var orderResult = service.CreateRentalOrder("Tester", "111", "AA0000BB", 2, new RegularCustomerStrategy());
            
            Assert.True(orderResult.IsSuccess);
            Assert.NotNull(orderResult.Value);

            service.CompleteOrder(orderResult.Value!.Id, 2);

            Assert.Equal(2000, service.GetTotalRevenue());
        }

        // --- ТЕСТИ LINQ ЗАПИТІВ ---
        [Fact]
        public void Service_LINQ_GetVehiclesSortedByPrice_ShouldBeCorrect()
        {
            var cheapCar = new PassengerCar(Guid.NewGuid(), "Cheap", "AA7777BB", 500, 4);
            _repo.AddVehicle(cheapCar);
            var service = new RentalOrderService(_repo, null!, null!);

            var sorted = service.GetVehiclesSortedByPrice().ToList();

            Assert.Equal("Cheap", sorted.First().Brand);
            Assert.Equal("Test Brand", sorted.Last().Brand);
        }

        [Fact]
        public void Service_LINQ_GetActiveOrders_ShouldIgnoreCancelledAndCompleted()
        {
            var orderStore = new JsonDataStore<RentalOrder>("test_orders.json");
            var vehicleStore = new JsonDataStore<Vehicle>("test_vehicles.json");
            var service = new RentalOrderService(_repo, vehicleStore, orderStore);

            var res1 = service.CreateRentalOrder("T1", "11", "AA0000BB", 2, new RegularCustomerStrategy());
            var res2 = service.CreateRentalOrder("T2", "22", "AA0000BB", 2, new RegularCustomerStrategy());

            Assert.Single(service.GetActiveOrders());
        }

        // --- ТЕСТИ PERSISTENCE (SHADOW I/O) ---
        [Fact]
        public async Task DataStore_SaveAndLoadEmptyCollection_ShouldNotThrowException()
        {
            var store = new JsonDataStore<Vehicle>("empty_test.json");
            var list = new List<Vehicle>();

            await store.SaveAsync(list);
            var loaded = await store.LoadAsync();

            Assert.Empty(loaded);
        }

        [Fact]
        public async Task DataStore_LoadCorruptedFile_ShouldThrowInvalidOperationException()
        {
            var store = new JsonDataStore<Vehicle>("corrupted_test.json");
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "corrupted_test.json");
            System.IO.File.WriteAllText(path, "{ invalid json... }");

            // Виправлено: тест тепер асинхронно очікує генерацію винятку
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await store.LoadAsync());
        }
    }
}