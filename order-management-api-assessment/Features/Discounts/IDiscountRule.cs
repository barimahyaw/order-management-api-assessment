using order_management_api_assessment.Features.Customers;
using order_management_api_assessment.Features.Orders;

namespace order_management_api_assessment.Features.Discounts;

public interface IDiscountRule
{
    string Name { get; }
    bool IsApplicable(Customer customer, List<OrderItem> items); 
    decimal Calculate(Customer customer, List<OrderItem> items, decimal totalAmount);
}
