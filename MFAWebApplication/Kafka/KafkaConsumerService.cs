using AutoMapper;
using Confluent.Kafka;
using MessagePack;
using MFAWebApplication.Kafka;
using MFAWebApplication.Projections;

public class KafkaConsumerService : BackgroundService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _topic;
    private readonly IConsumer<Null, byte[]> _consumer;
    private readonly IDictionary<string, Type> _projectorTypes;
    private bool _appStarted = false;

    public KafkaConsumerService(
        IHostApplicationLifetime appLifetime,
        IServiceProvider serviceProvider,
        IConfiguration config,
        IDictionary<string, Type> projectorTypes)
    {
        _appLifetime = appLifetime;
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
        _topic = config["Kafka:Topic"];

        _consumer = new ConsumerBuilder<Null, byte[]>(consumerConfig).Build();
        _projectorTypes = projectorTypes;

        _appLifetime.ApplicationStarted.Register(() =>
        {
            _appStarted = true;
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine($"KafkaConsumerService started. Listening to topic: {_topic}");
        _consumer.Subscribe(_topic);

        while (!_appStarted && !stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(100, stoppingToken);
        }

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
                    Console.WriteLine($"Processed event type: {envelope.Type}");
                }
                //catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                       
                    Console.WriteLine(ex);
                }
            }
        }
        finally
        {
            _consumer.Close();
        }

    }

}