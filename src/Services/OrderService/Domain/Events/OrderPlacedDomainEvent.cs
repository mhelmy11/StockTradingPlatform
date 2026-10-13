using System;
using Shared.Domain;
using OrderService.Domain.Orders;
using OrderService.Domain.ValueObjects;

namespace OrderService.Domain.Events;

public sealed record OrderPlacedDomainEvent(
    OrderId OrderId,
    UserId UserId,
    Symbol Symbol,
    Side Side,
    OrderType Type,
    Quantity Quantity,
    Price? LimitPrice)
    : DomainEvent;