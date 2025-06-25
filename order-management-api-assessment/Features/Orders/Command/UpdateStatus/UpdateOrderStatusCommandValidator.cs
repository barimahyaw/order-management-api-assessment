using FluentValidation;
using order_management_api_assessment.Shared.Enums;

namespace order_management_api_assessment.Features.Orders.Command.UpdateStatus;

internal class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(o => o.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(o => o.NewStatus)
            .NotEmpty()
            .IsInEnum()
            .WithMessage("Valid order status is required");
    }
}
