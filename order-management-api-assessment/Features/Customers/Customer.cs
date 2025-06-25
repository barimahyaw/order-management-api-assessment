using order_management_api_assessment.Shared.Enums;

namespace order_management_api_assessment.Features.Customers;

public class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email {  get; set; }
    public CustomerSegment Segment { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsFirstTime { get; set; }
}
