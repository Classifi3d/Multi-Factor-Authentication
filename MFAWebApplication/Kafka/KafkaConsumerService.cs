using AuthenticationWebApplication.Enteties;
using AutoMapper;
using Confluent.Kafka;
using MFAWebApplication.Abstraction.UnitOfWork;
using MFAWebApplication.Context;
using MFAWebApplication.Enteties;
using System.Text.Json;

public class KafkaConsumerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsumer<Null, string> _consumer;
    private readonly string _topic;
    private readonly Mapper _mapper;

    public KafkaConsumerService(IServiceProvider serviceProvider, IConfiguration config, Mapper mapper)
    {
        _serviceProvider = serviceProvider;
        _mapper = mapper;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            GroupId = "read-db-consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            StatisticsIntervalMs = 5000,
            FetchWaitMaxMs = 5,        // ↓ reduces fetch latency
            FetchMinBytes = 1,         // deliver small messages immediately
            SessionTimeoutMs = 10000
        };

        _consumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
        _topic = config["Kafka:Topic"];
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        try
        {
            _consumer.Subscribe(_topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(stoppingToken);
                    var userEvent = JsonSerializer.Deserialize<UserCreatedEvent>(result.Message.Value);

                    if (userEvent != null)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var uow = scope.ServiceProvider.GetRequiredService<UnitOfWork<ReadDbContext>>();
                        var repo = uow.Repository<UserReadModel>();

                        var existingUser = await repo.GetByIdAsync(userEvent.Id, stoppingToken);
                        if (existingUser == null)
                        {
                            var userReadModel = _mapper.Map<UserReadModel>(userEvent);
                            await repo.AddAsync(userReadModel, stoppingToken);
                            await uow.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
        finally
        {
            _consumer.Close();
        }
    }
}