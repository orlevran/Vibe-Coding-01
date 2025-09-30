using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace ServiceA.Messaging;

public class KafkaProducerFactory(IOptions<KafkaOptions> options) : IKafkaProducerFactory
{
    private readonly KafkaOptions _options = options.Value;

    public IProducer<string, string> CreateProducer()
    {
        var config = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            Acks = Acks.Leader,
            EnableIdempotence = true,
            MessageTimeoutMs = 5000
        };

        return new ProducerBuilder<string, string>(config).Build();
    }
}

