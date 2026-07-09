# Thread safety

Keep in mind:

- Process one trigger at a time.
- Queue triggers and process them one by one from the queue.
- How to deal with context.

```cs
using System.Collections.Concurrent;

public class MessageProcessor : IAsyncDisposable
{
    private readonly ConcurrentQueue<string> _queue = new();
    private readonly SemaphoreSlim _signal = new(0);
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _worker;

    public MessageProcessor()
    {
        _worker = Task.Run(ProcessLoopAsync);
    }

    public void Enqueue(string message)
    {
        _queue.Enqueue(message);
        _signal.Release(); // wake the processor
    }

    private async Task ProcessLoopAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            await _signal.WaitAsync(_cts.Token);

            while (_queue.TryDequeue(out var msg))
            {
                await ProcessMessageAsync(msg);
            }
        }
    }

    private Task ProcessMessageAsync(string msg)
    {
        Console.WriteLine($"Processing: {msg}");
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _signal.Release();
        await _worker;
        _cts.Dispose();
        _signal.Dispose();
    }
};
```
