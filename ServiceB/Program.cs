using Microsoft.Extensions.Hosting;
using ServiceB;
using ServiceB.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton<CityCache>();
builder.Services.AddSingleton<ICityCache>(sp => sp.GetRequiredService<CityCache>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<CityCache>());
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
