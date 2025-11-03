using Confluent.Kafka;
using MessagePack;

namespace MFAWebApplication.Kafka;

public class KafkaProducerService
{
    private readonly IProducer<Null, byte[]> _producer;
    private readonly string _topic;

    public KafkaProducerService(
        IConfiguration config)
    {
        var producerConfig = new ProducerConfig { 
            BootstrapServers = config["Kafka:BootstrapServers"],
            LingerMs = 2,
            BatchNumMessages = 10,
            Acks = Acks.Leader
        };
        _topic = config["Kafka:Topic"];
        _producer = new ProducerBuilder<Null, byte[]>(producerConfig).Build();
    }

    public async Task ProduceAsync<TEvent>(TEvent @event)
    {
        var payload = MessagePackSerializer.Serialize(@event);
        var envelope = new KafkaEnvelope(typeof(TEvent).Name, payload);

        var bytes = MessagePackSerializer.Serialize(envelope);

        await _producer.ProduceAsync(_topic, new Message<Null, byte[]> { Value = bytes });
    }
}
