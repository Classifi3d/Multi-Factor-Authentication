using Confluent.Kafka;
using MessagePack;
using MFAWebApplication.Abstraction.Repository;
using MFAWebApplication.Context;
using Microsoft.Extensions.DependencyInjection;

namespace MFAWebApplication.Kafka;

public class KafkaProducerService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ProducerConfig _config;
    private readonly IProducer<Null, byte[]> _producer;
    private readonly string _topic;

    public KafkaProducerService(
        IServiceProvider serviceProvider,
        IConfiguration config)
    {
        _serviceProvider = serviceProvider;
        _config = new ProducerConfig { 
            BootstrapServers = config["Kafka:BootstrapServers"],
            LingerMs = 2,
            BatchNumMessages = 10,
            Acks = Acks.Leader
        };
        _topic = config["Kafka:Topic"];
    }

    public async Task ProduceAsync<TEvent>(TEvent @event)
    {
        var payload = MessagePackSerializer.Serialize(@event);
        var envelope = new KafkaEnvelope(typeof(TEvent).Name, payload);

        var bytes = MessagePackSerializer.Serialize(envelope);

        await _producer.ProduceAsync(_topic, new Message<Null, byte[]> { Value = bytes });
    }
}
