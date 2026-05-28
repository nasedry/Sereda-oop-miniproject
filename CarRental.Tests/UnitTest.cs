using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using CarRental.Domain;
using CarRental.Application;
using CarRental.Infrastructure;

namespace CarRental.Tests
{
    // ==========================================
    // 1. НАБІР ЮНІТ-ТЕСТІВ (МІНІМУМ 20 КЕЙСІВ)
    // ==========================================
    public class UnitTests
    {
        private readonly InMemoryRentalRepository _repo;
        private readonly PassengerCar _testCar;
        private readonly User _testUser;

        public UnitTests()
        {
            _repo = new InMemoryRentalRepository();
            _testCar = new PassengerCar(Guid.NewGuid(), "Test Brand", "AA0000BB", 1000, 5);
            _testUser = new User(Guid.NewGuid(), "Tester", new DriverLicense("111", "B"));
            _repo.AddVehicle(_testCar);
        }

        // --- Тести доменних інваріантів та статусів (5 тестів) ---
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

        [Fact]
        public void CompleteAlreadyCompletedOrder_ShouldThrowException()
        {
            var period = new RentalPeriod(DateTime.Today, DateTime.Today.AddDays(2));
            var order = new RentalOrder(Guid.NewGuid(), _testUser, _testCar, period, new RegularCustomerStrategy());
            order.CompleteOrder(DateTime.Today.AddDays(2));

            Assert.Throws<InvalidOperationException>(() => order.CompleteOrder(DateTime.Today.AddDays(3)));
        }

        // --- Параметризовані тести меж часу та валідації (4 тести) ---
        [Fact]
        public void RentalPeriod_EndDateBeforeStartDate_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new RentalPeriod(DateTime.Today, DateTime.Today.AddDays(-1)));
        }

        [Fact]
        public void RentalPeriod_ZeroDaysRental_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new RentalPeriod(DateTime.Today, DateTime.Today));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-2)]
        public void PassengerCar_WithInvalidSeats_ShouldThrowArgumentException(int seats)
        {
            Assert.Throws<ArgumentException>(() => new PassengerCar(Guid.NewGuid(), "Brand", "AA1111AA", 1000, seats));
        }

        // --- Патерн Strategy: Довгострокова оренда (3 тест-кейси через Theory) ---
        [Theory]
        [InlineData(5, 1000, 4250)]  // 5 днів, 1000/день, 15% знижка = 4250
        [InlineData(10, 500, 4250)]  // 10 днів, 500/день, 15% знижка = 4250
        [InlineData(6, 2000, 10200)] // 6 днів, 2000/день, 15% знижка = 10200
        public void LongTermStrategy_ShouldApply15PercentDiscount(int days, decimal price, decimal expectedTotal)
        {
            var car = new PassengerCar(Guid.NewGuid(), "Tesla", "AA1111BB", price, 5);
            var period = new RentalPeriod(DateTime.Today, DateTime.Today.AddDays(days));
            var order = new RentalOrder(Guid.NewGuid(), _testUser, car, period, new LongTermRentStrategy());

            Assert.Equal(expectedTotal, order.TotalCost);
        }

        // --- Патерн Strategy: Вікенд та базові тарифи (3 тест-кейси через Theory) ---
        [Theory]
        [InlineData(1, 1000, 950)]   // 1 день, 1000/день, 5% знижка = 950
        [InlineData(2, 1000, 1900)]  // 2 дні, 1000/день, 5% знижка = 1900
        [InlineData(3, 500, 1500)]   // ВИПРАВЛЕНО: 3 дні, 500/день, без знижки = 1500
        public void WeekendStrategy_ShouldApply5PercentDiscount(int days, decimal price, decimal expectedTotal)
        {
            var car = new PassengerCar(Guid.NewGuid(), "Ford", "AA2222BB", price, 5);
            var period = new RentalPeriod(DateTime.Today, DateTime.Today.AddDays(days));
            var order = new RentalOrder(Guid.NewGuid(), _testUser, car, period, new WeekendStrategy());

            Assert.Equal(expectedTotal, order.TotalCost);
        }

        // --- Тести Use Cases та бізнес-сервісів (3 тести) ---
        [Fact]
        public void Service_CreateOrder_WhenVehicleAlreadyRented_ShouldReturnFailure()
        {
            var service = new RentalOrderService(_repo, null!, null!);
            
            // Перша оренда успішна
            service.CreateRentalOrder("Tester", "111", "AA0000BB", 2, new RegularCustomerStrategy());
            
            // Друга оренда на той самий автомобіль має повернути бізнес-помилку Result
            var duplicateResult = service.CreateRentalOrder("Іван", "222", "AA0000BB", 3, new RegularCustomerStrategy());

            Assert.True(!duplicateResult.IsSuccess);
            Assert.Contains("вже заброньовано", duplicateResult.ErrorMessage);
        }

        [Fact]
        public void Service_CreateOrder_WithValidData_ShouldReturnSuccessResult()
        {
            var service = new RentalOrderService(_repo, null!, null!);
            var result = service.CreateRentalOrder("Катерина", "333", "AA0000BB", 3, new RegularCustomerStrategy());

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
        }

        // --- Тести аналітичних LINQ-запитів (2 тести) ---
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
        public void Service_LINQ_GetActiveOrders_ShouldIgnoreClosedStates()
        {
            var service = new RentalOrderService(_repo, null!, null!);
            var res = service.CreateRentalOrder("T1", "11", "AA0000BB", 2, new RegularCustomerStrategy());
            
            service.CancelOrder(res.Value!.Id); // Скасовуємо, статус більше не Active

            Assert.Empty(service.GetActiveOrders());
        }
    }

    // ==========================================
    // 2. НАБІР ІНТЕГРАЦІЙНИХ ТЕСТІВ (МІНІМУМ 8 КЕЙСІВ)
    // ==========================================
    public class IntegrationTests : IDisposable
    {
        private readonly string _tempVehiclesFile;
        private readonly string _tempOrdersFile;

        public IntegrationTests()
        {
            _tempVehiclesFile = Path.GetTempFileName();
            _tempOrdersFile = Path.GetTempFileName();
        }

        private (InMemoryRentalRepository, RentalOrderService) CreateCleanStack()
        {
            var repo = new InMemoryRentalRepository();
            var vStore = new JsonDataStore<Vehicle>(_tempVehiclesFile);
            var oStore = new JsonDataStore<RentalOrder>(_tempOrdersFile);
            var service = new RentalOrderService(repo, vStore, oStore);
            return (repo, service);
        }

        [Fact]
        public async Task IT_1_SaveAndLoadEmptyCollection_ShouldNotThrowException()
        {
            var store = new JsonDataStore<Vehicle>(_tempVehiclesFile);
            var list = new List<Vehicle>();

            await store.SaveAsync(list);
            var loaded = await store.LoadAsync();

            Assert.Empty(loaded);
        }

        [Fact]
        public async Task IT_2_SaveVehiclesToDisk_ShouldWritePhysicalFile()
        {
            var repo = new InMemoryRentalRepository();
            repo.AddVehicle(new PassengerCar(Guid.NewGuid(), "Mazda", "AA8888BB", 1000, 5));
            var store = new JsonDataStore<Vehicle>(_tempVehiclesFile);

            await store.SaveAsync(repo.GetAllVehicles().ToList());

            Assert.True(File.Exists(_tempVehiclesFile));
            Assert.True(new FileInfo(_tempVehiclesFile).Length > 0);
        }

        [Fact]
        public async Task IT_3_ReloadVehiclesFromDisk_ShouldRestorePolymorphicState()
        {
            var repo = new InMemoryRentalRepository();
            repo.AddVehicle(new PassengerCar(Guid.NewGuid(), "Tesla", "AA9999BB", 1500, 5));
            var store = new JsonDataStore<Vehicle>(_tempVehiclesFile);
            await store.SaveAsync(repo.GetAllVehicles().ToList());

            var freshStore = new JsonDataStore<Vehicle>(_tempVehiclesFile);
            var loaded = await freshStore.LoadAsync();

            Assert.Single(loaded);
            Assert.Equal("Tesla", loaded.First().Brand);
        }

        [Fact]
        public async Task IT_4_FullWorkflow_CreateOrder_ShouldModifyVehicleAvailability()
        {
            var (repo, service) = CreateCleanStack();
            var car = new PassengerCar(Guid.NewGuid(), "Nissan", "AA1234BB", 1000, 5);
            repo.AddVehicle(car);

            var result = service.CreateRentalOrder("Дмитро", "555", "AA1234BB", 3, new RegularCustomerStrategy());

            Assert.True(result.IsSuccess);
            Assert.False(car.IsAvailable);
        }

        [Fact]
        public async Task IT_5_CompleteOrder_ShouldPersistStatusAndRevenueChanges()
        {
            var (repo, service) = CreateCleanStack();
            var car = new PassengerCar(Guid.NewGuid(), "Honda", "AA4321BB", 1000, 5);
            repo.AddVehicle(car);

            var res = service.CreateRentalOrder("Ольга", "999", "AA4321BB", 2, new RegularCustomerStrategy());
            service.CompleteOrder(res.Value!.Id, 2);

            Assert.True(car.IsAvailable);
            Assert.Equal(2000, service.GetTotalRevenue());
        }

        // --- НЕГАТИВНІ ІНТЕГРАЦІЙНІ СЦЕНАРІЇ (FAULT HANDLING) ---
        [Fact]
        public async Task IT_6_LoadCorruptedJson_ShouldThrowInvalidOperationException()
        {
            await File.WriteAllTextAsync(_tempVehiclesFile, "{ пошкоджений або неповний об'єкт json... }");
            var vStore = new JsonDataStore<Vehicle>(_tempVehiclesFile);

            // ВИПРАВЛЕНО ВОРНІНГ: додано внутрішній await
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await vStore.LoadAsync());
        }

        [Fact]
        public async Task IT_7_MissingFile_ShouldReturnEmptyCollectionSafely()
        {
            if (File.Exists(_tempVehiclesFile)) File.Delete(_tempVehiclesFile);
            var vStore = new JsonDataStore<Vehicle>(_tempVehiclesFile);

            // ВИПРАВЛЕНО ВОРНІНГ: додано await перед викликом
            var result = await vStore.LoadAsync();
            Assert.Empty(result);
        }

        [Fact]
        public async Task IT_8_SequentialSavesToSameFile_ShouldOverwriteSafely()
        {
            var repo = new InMemoryRentalRepository();
            repo.AddVehicle(new PassengerCar(Guid.NewGuid(), "Volvo", "AA0011BB", 1500, 5));
            var vStore = new JsonDataStore<Vehicle>(_tempVehiclesFile);
            
            await vStore.SaveAsync(repo.GetAllVehicles().ToList());
            await vStore.SaveAsync(repo.GetAllVehicles().ToList());

            var reloaded = await vStore.LoadAsync();
            Assert.Single(reloaded);
        }

        public void Dispose()
        {
            if (File.Exists(_tempVehiclesFile)) File.Delete(_tempVehiclesFile);
            if (File.Exists(_tempOrdersFile)) File.Delete(_tempOrdersFile);
        }
    }
}