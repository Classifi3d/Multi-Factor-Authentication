using Confluent.Kafka;

namespace MFAWebApplication.Kafka;

public class KafkaProducerService
{
    private readonly ProducerConfig _config;
    private readonly IProducer<Null, string> _producer;
    private readonly string _topic;

    public KafkaProducerService(IConfiguration config)
    {
        _config = new ProducerConfig { BootstrapServers = config["Kafka:BootstrapServers"] };
        _topic = config["Kafka:Topic"];
    }

    private IProducer<Null, string> Producer => _producer ?? new ProducerBuilder<Null, string>(_config).Build();

    public async Task ProduceAsync(string message)
    {
        await Producer.ProduceAsync(_topic, new Message<Null, string> { Value = message });
    }
}

