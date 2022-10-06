using QueueWorkerService;

using IHost host = Host.CreateDefaultBuilder()
    .ConfigureServices(
        (_, services) =>
        {
            services
                .AddTransient<MonitorLoop>()
                .AddHostedService<QueuedHostedService>()
                .AddSingleton<IBackgroundTaskQueue>(
                    _ => new DefaultBackgroundTaskQueue(capacity: 3)
                );
        }
    )
    .Build();

host.Services.GetRequiredService<MonitorLoop>().StartMonitorLoop();

await host.RunAsync();
