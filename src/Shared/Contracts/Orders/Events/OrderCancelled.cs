using System;

namespace Contracts.Orders.Events;

public sealed record OrderCancelled(
    Guid OrderId,
    Guid UserId,
    string Symbol,
    DateTimeOffset CancelledAt);