
using MFAWebApplication.Context;
using MFAWebApplication.Kafka;
using System.Threading;

namespace MFAWebApplication.Outbox;
public class OutboxProcessorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly KafkaProducerService _kafka;

    public OutboxProcessorService(IServiceProvider serviceProvider, KafkaProducerService kafka)
    {
        _serviceProvider = serviceProvider;
        _kafka = kafka;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var database = scope.ServiceProvider.GetRequiredService<WriteDbContext>();

            var outboxMessages = database.OutboxMessages
                .Where(m => !m.Processed)
                .OrderBy(m => m.CreatedAt)
                .Take(10)
                .ToList();

            foreach (var message in outboxMessages)
            {
                try
                {
                    await _kafka.ProduceAsync(message);
                    message.Processed = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Outbox Error: {e.Message}");
                }
            }

            if (outboxMessages.Any())
            {
                await database.SaveChangesAsync(cancellationToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        }
    }
}
