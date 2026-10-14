namespace OrderService.Infrastrcture.Persistence.Outbox;

public enum OutboxDestination
{
    Kafka = 0,
    RabbitMQ = 1

}
