using System;
using FluentValidation;

namespace OrderService.Application.Features.GetOrder;

public class GetOrderValidator : AbstractValidator<GetOrderQuery>
{
    public GetOrderValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required.");
    }

}
