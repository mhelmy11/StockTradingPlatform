using System;
using Contracts.Orders.Commands;
using ExecutionService.Domain.Executions;
using ExecutionService.Infrastrcture.Persistence;
using ExecutionService.Infrastrcture.Persistence.Inbox;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace ExecutionService.Infrastrcture.Messaging.Consumers;
public sealed class ExecuteOrderConsumer : IConsumer<ExecuteOrder>
{
    private const string ConsumerName = nameof(ExecuteOrderConsumer);

    private readonly ExecutionDbContext _db;
    private readonly ILogger<ExecuteOrderConsumer> _logger;

    public ExecuteOrderConsumer(ExecutionDbContext db, ILogger<ExecuteOrderConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ExecuteOrder> context)
    {
        var messageId = context.MessageId?.ToString() ?? context.Message.OrderId.ToString();
        var command = context.Message;

        _logger.LogInformation(
            "Received ExecuteOrder {MessageId} for order {OrderId} ({Symbol}, {Side}, {Quantity} @ {LimitPrice})",
            messageId, command.OrderId, command.Symbol, command.Side, command.Quantity, command.LimitPrice);

        // IDEMPOTENCY CHECK
        var alreadyProcessed = await _db.ProcessedMessages
            .AnyAsync(p => p.MessageId == messageId && p.ConsumerName == ConsumerName, context.CancellationToken);

        if (alreadyProcessed)
        {
            _logger.LogWarning(
                "Duplicate message {MessageId} — already processed. Skipping.",
                messageId);
            return;
        }

        var execution = Execution.Complete(
            orderId: command.OrderId,
            userId: command.UserId,
            symbol: command.Symbol,
            side: (Side)command.Side,
            quantity: command.Quantity,
            executedPrice: 0);

        await _db.Executions.AddAsync(execution, context.CancellationToken);

        _db.ProcessedMessages.Add(ProcessedMessage.Create(messageId, ConsumerName));

        await _db.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Order {OrderId} executed . ExecutionId: {ExecutionId}.",
            command.OrderId, execution.Id.Value);
    }

}