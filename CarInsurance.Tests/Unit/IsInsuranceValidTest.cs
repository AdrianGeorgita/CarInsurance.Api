using CarInsurance.Api.Services;

namespace CarInsurance.Tests.Unit
{
    public class IsInsuranceValid
    {
        [Fact]
        public async Task IsInsuranceValidAsync_ValidDate()
        {
            using var context = TestContext.Create();
            var service = new CarService(context);

            var carId = 1;
            var date = new DateOnly(2024, 12, 25);

            var result = await service.IsInsuranceValidAsync(carId, date);

            Assert.True(result);
        }

        [Fact]
        public async Task IsInsuranceValidAsync_BeforeStartDate()
        {
            using var context = TestContext.Create();
            var service = new CarService(context);

            var carId = 1;
            var date = new DateOnly(2022, 12, 25);

            var result = await service.IsInsuranceValidAsync(carId, date);

            Assert.False(result);
        }

        [Fact]
        public async Task IsInsuranceValidAsync_AfterStartDate()
        {
            using var context = TestContext.Create();
            var service = new CarService(context);

            var carId = 1;
            var date = new DateOnly(2027, 12, 25);

            var result = await service.IsInsuranceValidAsync(carId, date);

            Assert.False(result);
        }

        [Fact]
        public async Task IsInsuranceValidAsync_InvalidCar()
        {
            using var context = TestContext.Create();
            var service = new CarService(context);

            var carId = 500;
            var date = new DateOnly(2024, 12, 25);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.IsInsuranceValidAsync(carId, date));
        }
    }
}