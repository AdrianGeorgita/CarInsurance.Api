using CarInsurance.Api.Dtos;
using CarInsurance.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CarInsurance.Api.Controllers;

[ApiController]
[Route("api")]
public class CarsController(CarService service) : ControllerBase
{
    private readonly CarService _service = service;

    [HttpGet("cars")]
    public async Task<ActionResult<List<CarDto>>> GetCars()
        => Ok(await _service.ListCarsAsync());

    [HttpGet("cars/{carId:long}/insurance-valid")]
    public async Task<ActionResult<InsuranceValidityResponse>> IsInsuranceValid(long carId, [FromQuery] string date)
    {
        if (carId < 1)
            return UnprocessableEntity("Invalid car Id. Ids must be positive values!");

        if (!DateOnly.TryParse(date, out var parsed))
            return BadRequest("Invalid date or format. Use YYYY-MM-DD.");

        if ((parsed > DateOnly.FromDateTime(DateTime.Now.AddYears(1)) || (parsed < DateOnly.MinValue)))
            return BadRequest("Invalid date. You can only check validity up to 1 year in the future.");

        try
        {
            var valid = await _service.IsInsuranceValidAsync(carId, parsed);
            return Ok(new InsuranceValidityResponse(carId, parsed.ToString("yyyy-MM-dd"), valid));
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Car {carId} not found");
        }
    }

    [HttpPost("cars/{carId:long}/claims")]
    public async Task<ActionResult> RegisterInsuranceClaim(long carId, [FromBody] InsuranceClaimDto claimDto)
    {
        if (carId < 1)
            return UnprocessableEntity("Invalid car Id. Ids must be positive values!");

        if (!DateOnly.TryParse(claimDto.ClaimDate, out var parsed))
            return BadRequest("Invalid date or format. Use YYYY-MM-DD.");

        if(claimDto.Description.Length > 500)
            return UnprocessableEntity("Description maximum length exceeded.");

        if(claimDto.Amount < 1)
            return UnprocessableEntity("Invalid Claim Amount. Amounts must be positive values!");

        try
        {
            var claim = await _service.RegisterInsuranceClaim(carId, claimDto);
            return CreatedAtAction(nameof(RegisterInsuranceClaim), new { carId = carId, claimId = claim.Id}, claim);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Car {carId} not found");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("cars/{carId:long}/history")]
    public async Task<ActionResult<CarHistoryResponse>> GetCarHistory(long carId)
    {
        if (carId < 1)
            return UnprocessableEntity("Invalid car Id. Ids must be positive values!");

        try
        {
            return Ok(await _service.GetCarHistory(carId));
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Car {carId} not found");
        }
    }

    [HttpPatch("cars/{carId:long}")]
    public async Task<ActionResult> UpdateCarOwner(long carId, [FromBody] UpdateCarOwnerDto dto)
    {
        if (carId < 1)
            return UnprocessableEntity("Invalid car Id. Ids must be positive values!");

        if (dto.NewOwnerId < 1)
            return UnprocessableEntity("Invalid owner Id. Ids must be positive values!");

        try
        {
            await _service.UpdateCarOwner(carId, dto.NewOwnerId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Car {carId} not found");
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("cars/{carId:long}/policies")]
    public async Task<ActionResult> RegisterInsurancePolicy(long carId, [FromBody] InsurancePolicyDto dto)
    {
        if (carId < 1)
            return UnprocessableEntity("Invalid car Id. Ids must be positive values!");

        if (!DateOnly.TryParse(dto.StartDate, out var parsedStartDate) || !DateOnly.TryParse(dto.EndDate, out var parsedEndDate))
            return BadRequest("Invalid date or format. Use YYYY-MM-DD.");

        if(parsedStartDate > parsedEndDate)
            return UnprocessableEntity("Invalid dates! The start date must be before the end date.");

        if (dto.Provider?.Length > 64)
            return UnprocessableEntity("Provider name length exceeded.");

        try
        {
            var policy = await _service.RegisterInsurancePolicy(carId, dto);
            return CreatedAtAction(nameof(RegisterInsurancePolicy), new { carId = carId, policyId = policy.Id }, policy);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Car {carId} not found");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}
