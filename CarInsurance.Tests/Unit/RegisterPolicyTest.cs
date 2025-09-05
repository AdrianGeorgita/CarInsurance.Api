using CarInsurance.Api.Dtos;
using CarInsurance.Api.Services;

namespace CarInsurance.Tests.Unit
{
    public class RegisterPolicyTest
    {
        [Fact]
        public async Task CanRegisterPolicyAsync_ValidDate()
        {
            using var context = TestContext.Create();
            var service = new CarService(context);

            var carId = 2;
            var insurancePolicyDto = new InsurancePolicyDto
            (
                Id: 2,
                StartDate: "2025-10-1",
                EndDate: "2026-4-1",
                Provider: "Groupama"
            );

            var result = await service.RegisterInsurancePolicy(carId, insurancePolicyDto);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task CanRegisterPolicyAsync_InvalidDate()
        {
            using var context = TestContext.Create();
            var service = new CarService(context);

            var carId = 2;
            var insurancePolicyDto = new InsurancePolicyDto
            (
                Id: 3,
                StartDate: "20ha-12-25",
                EndDate: "2024-12-25",
            Provider: "Groupama"
            );

            await Assert.ThrowsAsync<ArgumentException>(() => service.RegisterInsurancePolicy(carId, insurancePolicyDto));
        }

        [Fact]
        public async Task CanRegisterPolicyAsync_OverlappingDate()
        {
            using var context = TestContext.Create();
            var service = new CarService(context);

            var carId = 2;
            var insurancePolicyDto = new InsurancePolicyDto
            (
                Id: 3,
                StartDate: "2025-04-1",
                EndDate: "2025-05-01",
                Provider: "Groupama"
            );

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterInsurancePolicy(carId, insurancePolicyDto));
        }

        [Fact]
        public async Task CanRegisterPolicyAsync_InvalidCar()
        {
            using var context = TestContext.Create();
            var service = new CarService(context);

            var carId = -1;
            var insurancePolicyDto = new InsurancePolicyDto
            (
                Id: 3,
                StartDate: "2025-10-1",
                EndDate: "2026-4-1",
                Provider: "Groupama"
            );

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.RegisterInsurancePolicy(carId, insurancePolicyDto));
        }
    }
}