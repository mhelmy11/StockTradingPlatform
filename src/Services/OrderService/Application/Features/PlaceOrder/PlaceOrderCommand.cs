using System;
using MediatR;
using Shared.Results;

namespace OrderService.Application.Features.PlaceOrder;

public sealed record PlaceOrderCommand(
    string Symbol,
    int Side,
    int Type,
    int Quantity,
    decimal? LimitPrice)
    : IRequest<Result<PlaceOrderResponse>>;