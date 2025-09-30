using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ServiceA.Messaging;

public class CityResponseHandler : BackgroundService
{
    private readonly ILogger<CityResponseHandler> _logger;
    private readonly IResponseCache _cache;
    private readonly IConsumer<string, string> _consumer;
    private readonly KafkaOptions _options;

    public CityResponseHandler(ILogger<CityResponseHandler> logger, IResponseCache cache, IOptions<KafkaOptions> options)
    {
        _logger = logger;
        _cache = cache;
        _options = options.Value;

        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.ResponseConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _consumer.Subscribe(_options.CityResponseTopic);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(TimeSpan.FromMilliseconds(_options.ResponsePollingDelayMs));
                if (consumeResult is not null)
                {
                    var sisterCities = consumeResult.Message.Value.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    _cache.Store(consumeResult.Message.Key, sisterCities);
                    _logger.LogInformation("Received sister cities for {City}: {Sisters}", consumeResult.Message.Key, string.Join(", ", sisterCities));
                }
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume error");
            }

            await Task.Delay(_options.ResponsePollingDelayMs, stoppingToken);
        }
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }
}

