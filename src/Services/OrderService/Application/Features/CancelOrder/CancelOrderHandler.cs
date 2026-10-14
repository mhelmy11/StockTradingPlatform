using System;


using MediatR;
using OrderService.Application.Abstraction;
using OrderService.Domain.Orders;
using OrderService.Domain.ValueObjects;
using Shared.Domain;
using Shared.Results;
using Shared.Services.CurrentUserService;

namespace OrderService.Application.Features.CancelOrder;

public sealed class CancelOrderHandler
    : IRequestHandler<CancelOrderCommand, Result<CancelOrderResponse>>
{
    private readonly IOrderRepository _orders;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CancelOrderHandler(
        IOrderRepository orders,
        IUnitOfWork uow,
        [FromKeyedServices("Headers")] ICurrentUserService currentUser)
    {
        _orders = orders;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<CancelOrderResponse>> Handle(
        CancelOrderCommand request,
        CancellationToken ct)
    {
        var orderId = OrderId.From(request.OrderId);
        var order = await _orders.GetByIdAsync(orderId, ct)
            ?? throw new DomainException($"Order {request.OrderId} not found.");

        if (order.UserId != UserId.From(Guid.Parse(_currentUser.GetCurrentUserId())))
            throw new DomainException("You can only cancel your own orders.");

        order.Cancel();
        await _uow.SaveChangesAsync(ct);

        return Result.Success(new CancelOrderResponse(
            OrderId: order.Id.Value,
            Status: order.Status.ToString()));
    }
}