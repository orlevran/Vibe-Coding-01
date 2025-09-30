using Confluent.Kafka;

namespace ServiceA.Messaging;

public interface IKafkaProducerFactory
{
    IProducer<string, string> CreateProducer();
}

