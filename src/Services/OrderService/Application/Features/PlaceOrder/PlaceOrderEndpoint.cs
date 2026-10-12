using System;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace OrderService.Application.Features.PlaceOrder;

public class PlaceOrderEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders/place", async (
           [FromBody] PlaceOrderCommand command,
           IMediator mediator,
           CancellationToken ct) =>
       {
           var result = await mediator.Send(command, ct);
           return result;
       })
       .WithName("PlaceOrder")
       .WithTags("Orders");
    }
}
