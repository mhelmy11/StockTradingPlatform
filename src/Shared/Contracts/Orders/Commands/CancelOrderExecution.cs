using System;

namespace Contracts.Orders.Commands;

public sealed record CancelOrderExecution(
    Guid OrderId,
    DateTimeOffset CancelledAt);