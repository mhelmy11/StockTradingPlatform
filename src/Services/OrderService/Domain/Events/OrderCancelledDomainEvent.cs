using System;
using OrderService.Domain.Orders;
using OrderService.Domain.ValueObjects;
using Shared.Domain;

namespace OrderService.Domain.Events;

public sealed record OrderCancelledDomainEvent(OrderId OrderId, UserId UserId, Symbol Symbol)
    : DomainEvent;