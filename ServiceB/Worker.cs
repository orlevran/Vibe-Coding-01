using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ServiceB.Messaging;

namespace ServiceB;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConsumer<string, string> _consumer;
    private readonly IProducer<string, string> _producer;
    private readonly ICityCache _cityCache;
    private readonly KafkaOptions _options;

    public Worker(ILogger<Worker> logger, ICityCache cityCache, IOptions<KafkaOptions> options)
    {
        _logger = logger;
        _cityCache = cityCache;
        _options = options.Value;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            Acks = Acks.Leader,
            EnableIdempotence = true,
            MessageTimeoutMs = 5000
        };

        _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
        _consumer.Subscribe(_options.RequestTopic);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(stoppingToken);
                if (result is null)
                {
                    continue;
                }

                var city = result.Message.Value;
                var correlationId = result.Message.Key;

                _logger.LogInformation("Processing city request {City} ({CorrelationId})", city, correlationId);

                if (!_cityCache.TryGetSisterCities(city, out var sisters))
                {
                    _logger.LogWarning("City {City} not found in cache", city);
                    sisters = new[] { "Unknown" };
                }

                await _producer.ProduceAsync(_options.ResponseTopic, new Message<string, string>
                {
                    Key = correlationId,
                    Value = string.Join('|', sisters)
                });

                _logger.LogInformation("Published sister cities for {City}: {Sisters}", city, string.Join(", ", sisters));
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume error");
            }
            catch (OperationCanceledException)
            {
                // Swallow cancellation
            }
        }
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        _producer.Flush();
        _producer.Dispose();
        base.Dispose();
    }
}
