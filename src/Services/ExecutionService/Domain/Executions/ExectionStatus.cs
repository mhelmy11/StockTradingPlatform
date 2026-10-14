using System;

namespace ExecutionService.Domain.Executions;

public enum ExecutionStatus
{
    Completed = 0,
    PartiallyFilled = 1,
    Rejected = 2,
    Failed = 3
}
