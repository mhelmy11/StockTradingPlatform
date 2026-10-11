using System;
using BuildingBlocks.Domain;
using OrderService.Domain.Orders;
using OrderService.Domain.ValueObjects;

namespace OrderService.Domain.Events;

public sealed record OrderCancelledDomainEvent(OrderId OrderId, UserId UserId, Symbol Symbol)
    : DomainEvent;