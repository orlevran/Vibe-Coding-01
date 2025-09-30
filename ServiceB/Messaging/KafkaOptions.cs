namespace ServiceB.Messaging;

public class KafkaOptions
{
    public string BootstrapServers { get; set; } = "localhost:9092";

    public string RequestTopic { get; set; } = "city-requests";

    public string ResponseTopic { get; set; } = "city-responses";

    public string ConsumerGroupId { get; set; } = "service-b-requests";

    public int PollIntervalMs { get; set; } = 250;
}

