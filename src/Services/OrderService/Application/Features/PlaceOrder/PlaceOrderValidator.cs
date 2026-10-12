
using FluentValidation;
namespace OrderService.Application.Features.PlaceOrder;

public sealed class PlaceOrderValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderValidator()
    {
        RuleFor(x => x.Symbol).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Side).InclusiveBetween(0, 1);
        RuleFor(x => x.Type).InclusiveBetween(0, 2);
        RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(1_000_000);
        RuleFor(x => x.LimitPrice).GreaterThan(0).When(x => x.LimitPrice.HasValue);
    }
}