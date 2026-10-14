using System;

namespace OrderService.Infrastrcture.Persistence.Messaging;

public sealed class OutboxSignal
{
    private readonly SemaphoreSlim _semaphore = new(initialCount: 0, maxCount: 1);

    public void Signal()
    {
        if (_semaphore.CurrentCount == 0)
        {
            try
            {
                _semaphore.Release();
            }
            catch (SemaphoreFullException)
            {
            }
        }
    }

    public async Task<bool> WaitAsync(TimeSpan timeout, CancellationToken ct)
      => await _semaphore.WaitAsync(timeout, ct);
}