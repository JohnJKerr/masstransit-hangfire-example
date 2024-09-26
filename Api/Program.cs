using System.Data;
using System.Security.Authentication;
using Api;
using Hangfire;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHangfire(cfg => cfg.UseInMemoryStorage());
builder.Services.AddHangfireServer();

builder.Services.AddMassTransit(mt =>
{
    mt.AddPublishMessageScheduler();
    mt.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("amqps://localhost:5672", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.Message<TestMessage>(c => c.SetEntityName("TestMessage"));
        cfg.ConfigureEndpoints(context);
    });
    mt.AddConsumer<TestMessageConsumer>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHangfireDashboard();

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/test", (IPublishEndpoint endpoint) =>
    {
        endpoint.Publish(new TestMessage(Guid.NewGuid()));
    })
    .WithName("Test")
    .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
