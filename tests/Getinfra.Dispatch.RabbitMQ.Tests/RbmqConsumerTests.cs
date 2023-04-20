using Getinfra.Dispatch.Abstractions.Models;
using Getinfra.Dispatch.Abstractions;
using Microsoft.Extensions.Logging;
using Getinfra.Dispatch.RabbitMQ.Models;
using Getinfra.Dispatch.Abstractions.Serializers;
using FluentAssertions;

namespace Getinfra.Dispatch.RabbitMQ.Tests
{
    public class RbmqConsumerTests
    {
        private readonly LoggerFactory _factory;
        public RbmqConsumerTests() {
            _factory = new LoggerFactory();
        }

        [Fact]
        public async Task DequeueTest()
        {
            var config = new RabbitMqConfig()
            {
                Host = "localhost",
                Port = 5672,
                Username = "guest",
                Password = "guest",
                Exchange = "exchange.dev.direct",
                ExchangeType = "direct",
                Queue = "queue-for-test",
                RoutingKey = "key-for-test"
            };

            // define publisher
            IQueuePublisher publisher = new RabbitMqPublisher(_factory.CreateLogger<RabbitMqPublisher>(), config, new DefaultJsonSerializer());

            // enqueue
            var exception = await Record.ExceptionAsync(() => publisher.Enqueue(new QMessage() { Body = new DummyObject() { Id = 1, Name = "test" } }));


            exception.Should().BeNull();

            IQueueConsumer consumer = new RabbitMqConsumer(_factory.CreateLogger<RabbitMqConsumer>(), config, new DefaultJsonSerializer());

            var response = consumer.Dequeue<DummyObject>();

            response.Should().NotBeNull();

        }
    }
}