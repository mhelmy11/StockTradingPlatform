using System;


using MediatR;
using OrderService.Domain.Orders;
using Shared.Domain;
using Shared.Results;

namespace OrderService.Application.Features.GetOrder;

public sealed class GetOrderHandler
    : IRequestHandler<GetOrderQuery, Result<GetOrderResponse>>
{
    private readonly IOrderRepository _orders;

    public GetOrderHandler(IOrderRepository orders)
    {
        _orders = orders;
    }

    public async Task<Result<GetOrderResponse>> Handle(GetOrderQuery request, CancellationToken ct)
    {
        var orderId = OrderId.From(request.OrderId);
        var order = await _orders.GetByIdAsync(orderId, ct)
            ?? throw new DomainException($"Order {request.OrderId} not found.");

        return new GetOrderResponse(
            OrderId: order.Id.Value,
            UserId: order.UserId.Value,
            Symbol: order.Symbol.Value,
            Side: (int)order.Side,
            Type: (int)order.Type,
            Quantity: order.Quantity.Value,
            FilledQuantity: order.FilledQuantity.Value,
            RemainingQuantity: order.RemainingQuantity.Value,
            LimitPrice: order.LimitPrice?.Amount,
            Status: (int)order.Status,
            StatusText: order.Status.ToString(),
            PlacedAt: order.PlacedAt,
            FilledAt: order.FilledAt,
            CancelledAt: order.CancelledAt);
    }
}