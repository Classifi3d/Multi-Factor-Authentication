using AutoMapper;
using Confluent.Kafka;
using MessagePack;
using MFAWebApplication.Kafka;
using MFAWebApplication.Projections;

public class KafkaConsumerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsumer<Null, byte[]> _consumer;
    private readonly IDictionary<string, Type> _projectorTypes;
    private readonly string _topic;

    public KafkaConsumerService(
        IServiceProvider serviceProvider,
        IConfiguration config)
    {
        _serviceProvider = serviceProvider;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            GroupId = "read-db-consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            StatisticsIntervalMs = 5000,
            FetchWaitMaxMs = 5,
            FetchMinBytes = 1,
            SessionTimeoutMs = 10000
        };

        _consumer = new ConsumerBuilder<Null, byte[]>(consumerConfig).Build();
        _topic = config["Kafka:Topic"];
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(stoppingToken);
                    if (result?.Message?.Value == null) continue;

                    var envelope = MessagePackSerializer.Deserialize<KafkaEnvelope>(result.Message.Value);

                    if (!_projectorTypes.TryGetValue(envelope.Type, out var projType))
                    {
                        // unknown event - skip, optionally log or push to DLQ
                        _consumer.Commit(result); // commit to avoid redelivery if you don't want to process
                        continue;
                    }

                    using var scope = _serviceProvider.CreateScope();
                    var projector = (IEventProjector)scope.ServiceProvider.GetRequiredService(projType);

                    await projector.ProjectAsync(envelope.Payload, stoppingToken);

                    // commit offset only after successful projection
                    _consumer.Commit(result);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    // log ex, do not commit to allow retry; consider moving message to DLQ after repeated failures
                }
            }
        }
        finally
        {
            _consumer.Close();
        }
    }

}