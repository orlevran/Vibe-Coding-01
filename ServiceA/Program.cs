using Confluent.Kafka;
using Microsoft.Extensions.Options;
using ServiceA.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton<IKafkaProducerFactory, KafkaProducerFactory>();
builder.Services.AddSingleton<IResponseCache, InMemoryResponseCache>();
builder.Services.AddHostedService<CityResponseHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/cities/request", async Task<IResult> (CityRequest request, IKafkaProducerFactory factory, IResponseCache cache, IOptions<KafkaOptions> options) =>
{
    var correlationId = string.IsNullOrWhiteSpace(request.CorrelationId) ? Guid.NewGuid().ToString() : request.CorrelationId;
    using var producer = factory.CreateProducer();
    cache.MarkPending(correlationId);
    await producer.ProduceAsync(options.Value.CityRequestTopic, new Message<string, string>
    {
        Key = correlationId,
        Value = request.CityName
    });
    return Results.Accepted(value: new { correlationId, message = "City request submitted" });
})
.WithName("RequestCitySisters")
.WithSummary("Request sister cities for a given city")
.WithDescription("Publishes a Kafka message for the requested city and returns a correlation id to poll results.");

app.MapGet("/cities/responses/{correlationId}", (string correlationId, IResponseCache cache) =>
{
    return cache.TryGet(correlationId, out var sisters)
        ? Results.Ok(new CityResponse(correlationId, sisters))
        : Results.NotFound();
})
.WithName("GetCitySistersResponse")
.WithSummary("Retrieve sister city response by correlation id")
.WithDescription("Returns the cached sister cities result if available; otherwise responds with 404 while waiting for processing.");

app.Run();

record CityRequest(string CityName, string? CorrelationId);

record CityResponse(string CorrelationId, IReadOnlyList<string> SisterCities);
