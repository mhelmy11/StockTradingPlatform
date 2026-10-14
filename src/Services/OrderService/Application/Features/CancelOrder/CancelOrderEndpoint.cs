using System;
using Carter;
using MediatR;

namespace OrderService.Application.Features.CancelOrder;

public class CancelOrderEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders/{orderId:guid}/cancel", async (
           Guid orderId,
           IMediator mediator,
           CancellationToken ct) =>
       {
           var command = new CancelOrderCommand(orderId);
           var response = await mediator.Send(command, ct);
           return response;
       })
       .WithName("CancelOrder")
       .WithTags("Orders");
    }
}
