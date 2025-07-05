namespace order_management_api_assessment.Models;

public class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public CustomerSegment Segment { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsFirstTime { get; set; }
}