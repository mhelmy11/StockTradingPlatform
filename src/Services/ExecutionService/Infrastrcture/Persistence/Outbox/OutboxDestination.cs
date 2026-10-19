using System;

namespace ExecutionService.Infrastrcture.Persistence.Outbox;

public enum OutboxDestination
{
    Kafka = 0,
    RabbitMq = 1
}
