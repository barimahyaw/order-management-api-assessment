using order_management_api_assessment.Features.Customers;
using order_management_api_assessment.Features.Orders;

namespace order_management_api_assessment.Features.Discounts;

public class VipCustomerDiscount : IDiscountRule
{
    public string Name => "VIP Customer";

    public decimal Calculate(Customer customer, List<OrderItem> items, decimal totalAmount)
        => totalAmount * 0.20m; // 20% discount for VIP customers

    public bool IsApplicable(Customer customer, List<OrderItem> items) => customer.Segment == Shared.Enums.CustomerSegment.VIP;
}
