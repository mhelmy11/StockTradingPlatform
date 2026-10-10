using System;

namespace OrderService.Domain.Orders;

public enum OrderStatus
{
    Placed = 0,
    PartiallyFilled = 1,
    Filled = 2,
    Cancelled = 3,
    Rejected = 4
}