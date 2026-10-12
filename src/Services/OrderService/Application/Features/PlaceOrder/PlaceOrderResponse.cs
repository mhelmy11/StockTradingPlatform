using System;

namespace OrderService.Application.Features.PlaceOrder;

public sealed record PlaceOrderResponse(
    Guid OrderId,
    string Status,
    string Symbol,
    int Side,
    int Quantity);