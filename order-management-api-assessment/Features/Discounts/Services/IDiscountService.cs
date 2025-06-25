using order_management_api_assessment.Features.Customers;
using order_management_api_assessment.Features.Orders;

namespace order_management_api_assessment.Features.Discounts.Services;

public record DiscountResult(decimal Amount, string? DiscountType);

public interface IDiscountService
{
    DiscountResult CalculateBestDiscount(Customer customer, List<OrderItem> items);
}
