using System;

namespace OrderService.Domain.Orders;

public enum OrderType
{
    Market = 0,
    Limit = 1,
    Stop = 2
}