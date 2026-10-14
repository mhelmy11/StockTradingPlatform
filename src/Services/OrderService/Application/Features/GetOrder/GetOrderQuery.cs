using System;
using MediatR;
using Shared.Results;

namespace OrderService.Application.Features.GetOrder;

public sealed record GetOrderQuery(Guid OrderId)
    : IRequest<Result<GetOrderResponse>>;



