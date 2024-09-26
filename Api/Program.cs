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

app.MapGet("/test", (IPublishEndpoint endpoint) =>
    {
        endpoint.Publish(new TestMessage(Guid.NewGuid()));
    })
    .WithName("Test")
    .WithOpenApi();

app.Run();

