using CarInsurance.Api.Models;

namespace CarInsurance.Api.Dtos;

public record CarDto(long Id, string Vin, string? Make, string? Model, int Year, long OwnerId, string OwnerName, string? OwnerEmail);
public record InsuranceValidityResponse(long CarId, string Date, bool Valid);
public record InsurancePolicyDto(long Id, string? Provider, string StartDate, string EndDate);
public record InsuranceClaimDto(long Id, string ClaimDate, string Description, long Amount);
public record CarHistoryResponse(string Type, long Id, string Date, string? Description, string? EndDate = null, long? Amount = null);
public record UpdateCarOwnerDto(long NewOwnerId);