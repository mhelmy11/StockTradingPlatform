using System;

namespace OrderService.Application.Features.CancelOrder;

public sealed record CancelOrderResponse(
    Guid OrderId,
    string Status);