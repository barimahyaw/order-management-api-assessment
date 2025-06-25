using FluentValidation;

namespace order_management_api_assessment.Features.Orders.Command.Create;

public class CreateOrderItemsValidator : AbstractValidator<OrderItemDto>
{
    public CreateOrderItemsValidator()
    {
        RuleFor(i => i.ProductId)
            .NotEmpty()
            .WithMessage("Product id is required");

        RuleFor(i => i.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0");

        RuleFor(i => i.UnitPrice)
            .GreaterThan(0)
            .WithMessage("Unit price must be greater 0");
    }
}
