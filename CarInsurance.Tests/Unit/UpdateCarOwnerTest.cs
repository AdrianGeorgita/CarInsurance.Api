using CarInsurance.Api.Dtos;
using CarInsurance.Api.Services;

namespace CarInsurance.Tests.Unit
{
    public class UpdateCarOwnerTest
    {
        [Fact]
        public async Task CanUpdateOwnerAsync_ValidOwner()
        {
            using var context = TestContext.Create();
            var service = new CarService(context);

            var carId = 2;
            var newOwnerId = 3;

            var result = await service.UpdateCarOwner(carId, newOwnerId);

            Assert.True(result);
        }

        [Fact]
        public async Task CanUpdateOwnerAsync_InvalidOwner()
        {
            using var context = TestContext.Create();
            var service = new CarService(context);

            var carId = 2;
            var newOwnerId = 10;

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateCarOwner(carId, newOwnerId));
        }

        [Fact]
        public async Task CanUpdateOwnerAsync_SameOwner()
        {
            using var context = TestContext.Create();
            var service = new CarService(context);

            var carId = 2;
            var newOwnerId = 2;

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateCarOwner(carId, newOwnerId));
        }

        [Fact]
        public async Task CanUpdateOwnerAsync_InvalidCar()
        {
            using var context = TestContext.Create();
            var service = new CarService(context);

            var carId = -1;
            var newOwnerId = 1;

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateCarOwner(carId, newOwnerId));
        }
    }
}