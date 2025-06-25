using order_management_api_assessment.Features.Customers;
using order_management_api_assessment.Features.Orders;

namespace order_management_api_assessment.Features.Discounts;

public class BulkOrderDiscount : IDiscountRule
{
    public string Name => "Bulk Order";

    public bool IsApplicable(Customer customer, List<OrderItem> items)
    {
        var totalQuantity = items.Sum(x => x.Quantity);
        return totalQuantity >= 10;
    }

    public decimal Calculate(Customer customer, List<OrderItem> items, decimal totalAmount) => totalAmount * 0.15m; // 15% discount for bulk orders
}
