namespace ServiceA.Messaging;

public class KafkaOptions
{
    public string BootstrapServers { get; set; } = "localhost:9092";

    public string CityRequestTopic { get; set; } = "city-requests";

    public string CityResponseTopic { get; set; } = "city-responses";

    public string ResponseConsumerGroupId { get; set; } = "service-a-responses";

    public int ResponsePollingDelayMs { get; set; } = 250;
}

