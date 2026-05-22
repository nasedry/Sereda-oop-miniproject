using System;
using System.Linq;
using Xunit;
using CarRental.Domain;
using CarRental.Application;
using CarRental.Infrastructure;

namespace CarRental.Tests
{
    public class RentalTests
    {
        [Fact]
        public void CreateUser_WithEmptyName_ShouldThrowArgumentException()
        {
            var license = new DriverLicense("12345", "B");
            Assert.Throws<ArgumentException>(() => new User(Guid.NewGuid(), "", license));
        }

        [Fact]
        public void CreateTruck_WithInvalidLoadCapacity_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Truck(Guid.NewGuid(), "Volvo", "BC1111AA", 2000, 0));
        }

        [Fact]
        public void PassengerCar_CalculateCost_ShouldReturnBasePriceMultipliedByDays()
        {
            var car = new PassengerCar(Guid.NewGuid(), "Audi", "AA1111AA", 1000, 5);
            decimal cost = car.CalculateActualRentCost(3);
            Assert.Equal(3000, cost);
        }

        [Fact]
        public void Truck_CalculateCost_ShouldIncludeTonnageSurcharge()
        {
            var truck = new Truck(Guid.NewGuid(), "MAN", "BC2222BB", 2000, 2.0);
            decimal expectedCost = (2000 + (decimal)(2.0 * 150)) * 2; 
            decimal actualCost = truck.CalculateActualRentCost(2);
            Assert.Equal(expectedCost, actualCost);
        }

        [Fact]
        public void Vehicle_MarkAsRented_ShouldChangeIsAvailableToFalse()
        {
            var car = new PassengerCar(Guid.NewGuid(), "Ford", "AA3333AA", 800, 5);
            Assert.True(car.IsAvailable);
            car.MarkAsRented();
            Assert.False(car.IsAvailable);
        }

        [Fact]
        public void RentalOrderService_CreateOrder_ShouldSuccess_WhenVehicleIsAvailable()
        {
            var repo = new InMemoryRentalRepository();
            var car = new PassengerCar(Guid.NewGuid(), "Honda", "AA4444AA", 1000, 5);
            repo.AddVehicle(car);
            var service = new RentalOrderService(repo);

            var result = service.CreateRentalOrder("John", "Lic123", "AA4444AA", 5);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.False(car.IsAvailable);
        }

        [Fact]
        public void RentalOrderService_CreateOrder_ShouldFail_WhenVehicleNotFound()
        {
            var repo = new InMemoryRentalRepository();
            var service = new RentalOrderService(repo);

            var result = service.CreateRentalOrder("John", "Lic123", "NONEXIST", 5);

            Assert.False(result.IsSuccess);
            Assert.Equal("Автомобіль з таким номером не знайдено.", result.ErrorMessage);
        }

        [Fact]
        public void RentalOrderService_CreateOrder_ShouldFail_WhenVehicleAlreadyRented()
        {
            var repo = new InMemoryRentalRepository();
            var car = new PassengerCar(Guid.NewGuid(), "Honda", "AA5555AA", 1000, 5);
            car.MarkAsRented();
            repo.AddVehicle(car);
            var service = new RentalOrderService(repo);

            var result = service.CreateRentalOrder("John", "Lic123", "AA5555AA", 3);

            Assert.False(result.IsSuccess);
            Assert.Equal("Цей автомобіль вже заброньовано.", result.ErrorMessage);
        }
    }
}