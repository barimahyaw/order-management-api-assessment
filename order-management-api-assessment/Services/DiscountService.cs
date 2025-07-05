using order_management_api_assessment.Models;

namespace order_management_api_assessment.Services;

public class DiscountService
{
    public DiscountResult CalculateBestDiscount(Customer customer, List<Order.OrderItem> orderItems)
    {
        if (orderItems.Count == 0)
            return new DiscountResult(0, "");

        var totalAmount = orderItems.Sum(item => item.Quantity * item.Price);
        var discounts = new List<DiscountResult>();

        // First-time buyer discount: 10%
        if (customer.IsFirstTime)
        {
            discounts.Add(new DiscountResult(totalAmount * 0.10m, "10% discount applied (First-time buyer)"));
        }

        // Bulk order discount: 15% for 10+ items
        var totalQuantity = orderItems.Sum(item => item.Quantity);
        if (totalQuantity >= 10)
        {
            discounts.Add(new DiscountResult(totalAmount * 0.15m, "15% discount applied (Bulk order)"));
        }

        // VIP customer discount: 20%
        if (customer.Segment == CustomerSegment.VIP)
        {
            discounts.Add(new DiscountResult(totalAmount * 0.20m, "20% discount applied (VIP customer)"));
        }

        // Return the best (highest) discount
        return discounts.OrderByDescending(d => d.Amount).FirstOrDefault() ?? new DiscountResult(0, "");
    }
}

public record DiscountResult(decimal Amount, string DiscountType);