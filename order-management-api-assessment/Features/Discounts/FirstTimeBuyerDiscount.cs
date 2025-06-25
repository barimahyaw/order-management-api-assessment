using order_management_api_assessment.Features.Customers;
using order_management_api_assessment.Features.Orders;

namespace order_management_api_assessment.Features.Discounts
{
    public class FirstTimeBuyerDiscount : IDiscountRule
    {
        public string Name => "First Time Buyer";

        public bool IsApplicable(Customer customer, List<OrderItem> items) => customer.IsFirstTime;

        public decimal Calculate(Customer customer, List<OrderItem> items, decimal totalAmount) => totalAmount * 0.10m; // 10% dicount

    }
}
