using Confluent.Kafka;
using MessagePack;
using MessagePack.Resolvers;
using MFAWebApplication.Outbox;

namespace MFAWebApplication.Kafka;

public class KafkaProducerService
{
    private readonly IProducer<Null, byte[]> _producer;
    private readonly string _topic;

    public KafkaProducerService(
        IConfiguration config)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            LingerMs = 2,
            BatchNumMessages = 10,
            Acks = Acks.Leader
        };
        _topic = config["Kafka:Topic"];
        _producer = new ProducerBuilder<Null, byte[]>(producerConfig).Build();
    }

    public async Task ProduceAsync(OutboxMessage message)
    {
        var options = MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance);
        var bytes = MessagePackSerializer.Serialize(message, options);  
        await _producer.ProduceAsync(
            _topic,
            new Message<Null, byte[]> { Value = bytes }
            );
        _producer.Flush(TimeSpan.FromSeconds(1));

    }
}
