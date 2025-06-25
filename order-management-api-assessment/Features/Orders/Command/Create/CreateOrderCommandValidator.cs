using FluentValidation;

namespace order_management_api_assessment.Features.Orders.Command.Create;

internal class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(o => o.CustomerId)
            .NotEmpty()
            .WithMessage("Customer id is required");

        RuleFor(o => o.OrderItems)
            .NotEmpty()
            .WithMessage("Order must contain at least one item");

        RuleForEach(o => o.OrderItems)
            .SetValidator(new CreateOrderItemsValidator());
    }
}