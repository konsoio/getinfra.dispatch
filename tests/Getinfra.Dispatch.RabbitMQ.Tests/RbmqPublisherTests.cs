using Getinfra.Dispatch.Abstractions.Models;
using Getinfra.Dispatch.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getinfra.Dispatch.Abstractions.Serializers;
using Getinfra.Dispatch.RabbitMQ.Models;
using FluentAssertions;

namespace Getinfra.Dispatch.RabbitMQ.Tests
{
    public class RbmqPublisherTests
    {
        private readonly LoggerFactory _factory;
        public RbmqPublisherTests()
        {
            _factory = new LoggerFactory();
        }

        [Fact]
        public async Task EnqueueTest()
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
                RoutingKey="key-for-test"
            };

            // define publisher
            IQueuePublisher publisher = new RabbitMqPublisher(_factory.CreateLogger<RabbitMqPublisher>(), config, new DefaultJsonSerializer());

            // enqueue
            var exception = await Record.ExceptionAsync(() => publisher.Enqueue(new QMessage() { Body = new DummyObject() { Id = 1, Name = "test" } }));

            exception.Should().BeNull();

        }
    }
}
