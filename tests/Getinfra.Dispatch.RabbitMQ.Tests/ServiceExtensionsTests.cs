using FluentAssertions;
using Getinfra.Dispatch.RabbitMQ.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Getinfra.Dispatch.RabbitMQ.Tests
{
    public class ServiceExtensionsTests
    {
        private IConfiguration _configuration { get; set; }
        public void Init()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("testsettings.json")
                .Build();
        }


        public ServiceExtensionsTests()
        { 
            Init();
        }

        [Theory]
        [InlineData("Publisher")]
        public void GetPublisherSettings(string configName)
        { 
            var services = new ServiceCollection();

            var exception = Record.Exception(() => services.ConfigureRabbitMqPublisher(new LoggerFactory(), _configuration, configName));

            exception.Should().BeNull();
        }

        [Theory]
        [InlineData("MissingPublisher")]
        public void GetPublisherMissingSettings(string configName)
        {
            var services = new ServiceCollection();

            var exception = Record.Exception(() => services.ConfigureRabbitMqPublisher(new LoggerFactory(), _configuration, configName));

            exception.Should().NotBeNull();
        }


        [Theory]
        [InlineData("Consumer")]
        public void GetConsumerSettings(string configName)
        {
            var services = new ServiceCollection();

            var exception = Record.Exception(() => services.ConfigureRabbitMqConsumer(new LoggerFactory(), _configuration, configName));

            exception.Should().BeNull();
        }

        [Theory]
        [InlineData("MissingConsumer")]
        public void GetConsumerMissingSettings(string configName)
        {
            var services = new ServiceCollection();

            var exception = Record.Exception(() => services.ConfigureRabbitMqConsumer(new LoggerFactory(), _configuration, configName));

            exception.Should().NotBeNull();
        }

    }
}
