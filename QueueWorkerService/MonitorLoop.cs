namespace QueueWorkerService;

public sealed class MonitorLoop
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly ILogger<MonitorLoop> _logger;
    private readonly CancellationToken _cancellationToken;

    public MonitorLoop(
        IBackgroundTaskQueue taskQueue,
        ILogger<MonitorLoop> logger,
        IHostApplicationLifetime applicationLifetime
    )
    {
        _taskQueue = taskQueue;
        _logger = logger;
        _cancellationToken = applicationLifetime.ApplicationStopping;
    }

    public void StartMonitorLoop()
    {
        _logger.LogInformation($"{nameof(MonitorAsync)} loop is starting.");

        _logger.LogInformation(
            $"{nameof(MonitorLoop)} is running.{Environment.NewLine}"
                + $"{Environment.NewLine}Tap W to add a work item to the "
                + $"background queue.{Environment.NewLine}"
        );

        // Run a console user input loop in a background thread
        Task.Run(async () => await MonitorAsync());
    }

    private async ValueTask MonitorAsync()
    {
        while (!_cancellationToken.IsCancellationRequested)
        {
            if (Console.ReadKey().Key == ConsoleKey.W)
            {
                // Enqueue a background work item
                await _taskQueue.QueueBackgroundWorkItemAsync(BuildWorkItemAsync);

                _logger.LogInformation("Successfully queued work for later.");
            }
        }
    }

    private async ValueTask BuildWorkItemAsync(CancellationToken token)
    {
        // Simulate three 5-second tasks to complete
        // for each enqueued work item

        int delayLoop = 0;
        var guid = Guid.NewGuid();

        _logger.LogInformation("Queued work item {Guid} is starting.", guid);

        while (!token.IsCancellationRequested && delayLoop < 3)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), token);
            }
            catch (OperationCanceledException)
            {
                // Prevent throwing if the Delay is cancelled
            }

            ++delayLoop;

            _logger.LogInformation(
                "Queued work item {Guid} is running. {DelayLoop}/3",
                guid,
                delayLoop
            );
        }

        string format = delayLoop switch
        {
            3 => "Queued Background Task {Guid} is complete.",
            _ => "Queued Background Task {Guid} was cancelled."
        };

        _logger.LogInformation(format, guid);
    }
}
