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
    private readonly Mapper _mapper;

    public KafkaConsumerService(IServiceProvider serviceProvider, IConfiguration config, Mapper mapper)
    {
        _serviceProvider = serviceProvider;
        _mapper = mapper;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            GroupId = "read-db-consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
        _consumer.Subscribe(config["Kafka:Topic"]);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
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
                    var repo = uow.Repository<User>();

                    var existingUser = await repo.GetByIdAsync(userEvent.Id, stoppingToken);
                    if (existingUser == null)
                    {

                        var user = _mapper.Map<User>(userEvent);
                        await uow.SaveChangesAsync(stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException) { }
        }
    }
}
