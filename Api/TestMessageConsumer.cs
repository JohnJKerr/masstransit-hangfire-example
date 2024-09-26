using MassTransit;

namespace Api;

public class TestMessageConsumer : IConsumer<TestMessage>
{
    public Task Consume(ConsumeContext<TestMessage> context)
    {
        throw new Exception("An error has occurred");
    }
}