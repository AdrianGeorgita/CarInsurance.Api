using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using Microsoft.EntityFrameworkCore;
using CarInsurance.Api.Models;

namespace CarInsurance.Api.Services;

public class CarService(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task<List<CarDto>> ListCarsAsync()
    {
        return await _db.Cars.Include(c => c.Owner)
            .Select(c => new CarDto(c.Id, c.Vin, c.Make, c.Model, c.YearOfManufacture,
                                    c.OwnerId, c.Owner.Name, c.Owner.Email))
            .ToListAsync();
    }

    public async Task<bool> IsInsuranceValidAsync(long carId, DateOnly date)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");

        return await _db.Policies.AnyAsync(p =>
            p.CarId == carId &&
            p.StartDate <= date &&
            p.EndDate >= date
        );
    }

    public async Task<InsuranceClaim> RegisterInsuranceClaim(long carId, InsuranceClaimDto claimDto)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");

        if (!DateOnly.TryParse(claimDto.ClaimDate, out var claimDate))
            throw new ArgumentException("Invalid claim date format.");

        var claim = new InsuranceClaim
        {
            CarId = carId,
            ClaimDate = claimDate,
            Description = claimDto.Description,
            Amount = claimDto.Amount
        };

        _db.Claims.Add(claim);
        await _db.SaveChangesAsync();
        return claim;
    }

    public async Task<List<CarHistoryResponse>> GetCarHistory(long carId)
    {
        var car = await _db.Cars
            .Include(c => c.Policies)
            .Include(c => c.Claims)
            .Include(c => c.OwnershipChanges)
            .FirstOrDefaultAsync(c => c.Id == carId);

        if(car == null)
            throw new KeyNotFoundException($"Car {carId} not found");

        var policyItems = car.Policies
            .Select(p => new CarHistoryResponse(
                Type: "Policy",
                Id: p.Id,
                Date: p.StartDate.ToString("yyyy-MM-dd"),
                EndDate: p.EndDate.ToString("yyyy-MM-dd"),
                Description: $"Policy from {p.Provider}"
            ));

        var claimItems = car.Claims
            .Select(p => new CarHistoryResponse(
                Type: "Claim",
                Id: p.Id,
                Date: p.ClaimDate.ToString("yyyy-MM-dd"),
                Description: p.Description,
                Amount: p.Amount
            ));

        var ownershipItems = car.OwnershipChanges
            .Select(o => new CarHistoryResponse(
                Type: "Ownership",
                Id: o.Id,
                Date: o.ChangeDate.ToString("yyyy-MM-dd"),
                Description: $"Owner changed to {o.NewOwnerId}"
            ));

        var history = policyItems
           .Concat(claimItems)
           .Concat(ownershipItems)
           .OrderBy(item => item.Date)
           .ToList();

        return history ?? new List<CarHistoryResponse> { };
    }

    public async Task UpdateCarOwner(long carId, long newOwnerId)
    {
        var car = await _db.Cars.FindAsync(carId);
        if (car == null)
            throw new KeyNotFoundException($"Car {carId} not found");

        var newOwnerExists = await _db.Owners.AnyAsync(o => o.Id == newOwnerId);
        if (!newOwnerExists)
            throw new KeyNotFoundException($"Owner {newOwnerId} not found");

        var previousOwnerId = car.OwnerId;
        car.OwnerId = newOwnerId;

        _db.OwnershipChanges.Add(new CarOwnershipChange
        {
            CarId = carId,
            PreviousOwnerId = previousOwnerId,
            NewOwnerId = newOwnerId,
            ChangeDate = DateOnly.FromDateTime(DateTime.Now)
        }); ;

        await _db.SaveChangesAsync();
    }

    public async Task<InsurancePolicy> RegisterInsurancePolicy(long carId, InsurancePolicyDto dto)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");

        if (!DateOnly.TryParse(dto.StartDate, out var startDate) || !DateOnly.TryParse(dto.EndDate, out var endDate))
            throw new ArgumentException("Invalid claim date format.");

        bool overlaps = await _db.Policies
        .AnyAsync(p => p.CarId == carId &&
                       p.StartDate <= endDate &&
                       p.EndDate >= startDate);

        if (overlaps)
            throw new InvalidOperationException("Overlapping policy.");

        var policy = new InsurancePolicy
        {
            CarId = carId,
            StartDate = startDate,
            EndDate = endDate,
            Provider = dto.Provider
        };

        _db.Policies.Add(policy);
        await _db.SaveChangesAsync();
        return policy;
    }
}
