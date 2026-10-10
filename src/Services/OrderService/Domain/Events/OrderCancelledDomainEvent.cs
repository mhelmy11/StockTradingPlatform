using System;
using BuildingBlocks.Domain;
using OrderService.Domain.Orders;

namespace OrderService.Domain.Events;

public sealed record OrderCancelledDomainEvent(OrderId OrderId)
    : DomainEvent;