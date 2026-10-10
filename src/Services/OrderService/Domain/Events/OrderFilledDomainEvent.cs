using System;
using BuildingBlocks.Domain;
using OrderService.Domain.Orders;
using OrderService.Domain.ValueObjects;

namespace OrderService.Domain.Events;

public sealed record OrderFilledDomainEvent(
    OrderId OrderId,
    Quantity FilledQuantity,
    Price ExecutionPrice,
    OrderStatus NewStatus)
    : DomainEvent;