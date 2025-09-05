using CarInsurance.Api.Dtos;
using CarInsurance.Api.Services;

namespace CarInsurance.Tests.Unit
{
    public class RegisterClaimTest
    {
        [Fact]
        public async Task CanRegisterClaimAsync_ValidDate()
        {
            using var context = TestContext.Create();
            var service = new CarService(context);

            var carId = 2;
            var insuranceClaimDto = new InsuranceClaimDto
            (
                Id: 1,
                ClaimDate: "2025-10-1",
                Description: "Claim Description",
                Amount: 500
            );

            var result = await service.RegisterInsuranceClaim(carId, insuranceClaimDto);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task CanRegisterClaimAsync_InvalidDate()
        {
            using var context = TestContext.Create();
            var service = new CarService(context);

            var carId = 2;
            var insuranceClaimDto = new InsuranceClaimDto
            (
                Id: 1,
                ClaimDate: "202ha-10-1",
                Description: "Claim Description",
                Amount: 500
            );

            await Assert.ThrowsAsync<ArgumentException>(() => service.RegisterInsuranceClaim(carId, insuranceClaimDto));
        }

        [Fact]
        public async Task CanRegisterClaimAsync_InvalidCar()
        {
            using var context = TestContext.Create();
            var service = new CarService(context);

            var carId = -1;
            var insuranceClaimDto = new InsuranceClaimDto
            (
                Id: 1,
                ClaimDate: "2025-10-1",
                Description: "Claim Description",
                Amount: 500
            );

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.RegisterInsuranceClaim(carId, insuranceClaimDto));
        }
    }
}