using System;
using MediatR;
using Shared.Results;

namespace OrderService.Application.Features.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId)
    : IRequest<Result<CancelOrderResponse>>;