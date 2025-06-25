using order_management_api_assessment.Features.Customers;
using order_management_api_assessment.Features.Orders;

namespace order_management_api_assessment.Features.Discounts.Services;

public class DiscountService(IEnumerable<IDiscountRule> discountRules) : IDiscountService
{
    public DiscountResult CalculateBestDiscount(Customer customer, List<OrderItem> items)
    {
        var totalAmount = items.Sum(i => i.Price * i.Quantity);

        var applicableDiscounts = discountRules
            .Where(rule => rule.IsApplicable(customer, items))
            .Select(rule => new {Rule = rule, Amount = rule.Calculate(customer, items, totalAmount)})
            .ToList();

        if (applicableDiscounts.Count == 0)
        {
            return new DiscountResult(0, null);
        }

        var bestDiscount = applicableDiscounts.MaxBy(d => d.Amount)!;
        
        var discountType = bestDiscount.Rule.Name switch
        {
            "VIP Customer" => "20% discount applied",
            "Bulk Order" => "15% discount applied",
            "First Time Buyer" => "10% discount applied",
            _ => $"{bestDiscount.Rule.Name} discount applied"
        };

        return new DiscountResult(bestDiscount.Amount, discountType);
    }
}
