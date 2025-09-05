namespace CarInsurance.Api.Models;

public class CarOwnershipChange
{
    public long Id { get; set; }
    public long CarId {  get; set; }
    public Car Car { get; set; } = default!;


    public long? PreviousOwnerId {  get; set; }
    public long NewOwnerId { get; set; }

    public DateOnly ChangeDate { get; set; }
}
