using MediatR;
using OrderService.Application.Abstraction;
using OrderService.Domain.Orders;
using OrderService.Domain.ValueObjects;
using Shared.Helpers;
using Shared.Results;
using Shared.Services.CurrentUserService;

namespace OrderService.Application.Features.PlaceOrder;

public sealed class PlaceOrderHandler
    : IRequestHandler<PlaceOrderCommand, Result<PlaceOrderResponse>>
{
    private readonly IOrderRepository _orders;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService currentUserService;
    private readonly ILogger<PlaceOrderHandler> logger;

    public PlaceOrderHandler(
        IOrderRepository orders,
        IUnitOfWork uow,
        [FromKeyedServices("Headers")] ICurrentUserService currentUserService,
        ILogger<PlaceOrderHandler> logger
        )
    {
        _orders = orders;
        _uow = uow;
        this.currentUserService = currentUserService;
        this.logger = logger;
    }

    public async Task<Result<PlaceOrderResponse>> Handle(
        PlaceOrderCommand request,
        CancellationToken ct)
    {

        logger.LogInformation("Placing order for user {UserId} with symbol {Symbol}, side {Side}, type {Type}, quantity {Quantity}, limit price {LimitPrice}",
            currentUserService.GetCurrentUserId(),
            request.Symbol,
            request.Side,
            request.Type,
            request.Quantity,
            request.LimitPrice);


        var symbol = Symbol.From(request.Symbol);
        var side = (Side)request.Side;
        var type = (OrderType)request.Type;
        var quantity = Quantity.From(request.Quantity);
        var limitPrice = request.LimitPrice is null
            ? null
            : Price.From(request.LimitPrice.Value);

        var order = Order.Place(
            userId: UserId.From(Guid.Parse(currentUserService.GetCurrentUserId())),
            symbol: symbol,
            side: side,
            type: type,
            quantity: quantity,
            limitPrice: limitPrice);

        await _orders.AddAsync(order, ct);
        await _uow.SaveChangesAsync(ct);

        return new PlaceOrderResponse(
            OrderId: order.Id.Value,
            Status: order.Status.ToString(),
            Symbol: order.Symbol.Value,
            Side: (int)order.Side,
            Quantity: order.Quantity.Value);
    }
}