using System;
using Carter;
using MediatR;

namespace OrderService.Application.Features.GetOrder;

public class GetOrderEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders/{orderId:guid}", async (
           Guid orderId,
           IMediator mediator,
           CancellationToken ct) =>
       {
           var result = await mediator.Send(new GetOrderQuery(orderId), ct);
           return result;
       })
       .WithName("GetOrder")
       .WithTags("Orders")
       .Produces<GetOrderResponse>(StatusCodes.Status200OK);
    }
}
