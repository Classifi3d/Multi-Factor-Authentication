namespace MFAWebApplication.Kafka;
public record KafkaEnvelope(string Type, byte[] Payload);
