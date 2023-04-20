using Getinfra.Dispatch.Abstractions;
using Getinfra.Dispatch.Abstractions.Serializers;
using Getinfra.Dispatch.RabbitMQ.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Getinfra.Dispatch.RabbitMQ.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureRabbitMqConsumer(this IServiceCollection services, ILoggerFactory loggerFactory, IConfiguration configuration, string configName)
        {
            var config = new RabbitMqConfig();
            var serializer = new DefaultJsonSerializer();

            configuration.Bind($"Getinfra.Dispatch:RabbitMq:{configName}", config);

            if (string.IsNullOrEmpty(config.Host) || string.IsNullOrEmpty(config.Exchange) || string.IsNullOrEmpty(config.Username) || string.IsNullOrEmpty(config.Password))
                throw new Exception("Missing config");

            services.AddSingleton<IQueueConsumer>(s => new RabbitMqConsumer(loggerFactory.CreateLogger<RabbitMqConsumer>(), config, serializer));
        }

        public static void ConfigureRabbitMqPublisher(this IServiceCollection services, ILoggerFactory loggerFactory, IConfiguration configuration, string configName)
        {
            var config = new RabbitMqConfig();
            var serializer = new DefaultJsonSerializer();

            configuration.Bind($"Getinfra.Dispatch:RabbitMq:{configName}", config);

            if (string.IsNullOrEmpty(config.Host) || string.IsNullOrEmpty(config.Exchange) || string.IsNullOrEmpty(config.Username) || string.IsNullOrEmpty(config.Password))
                throw new Exception("Missing config");

            services.AddSingleton<IQueuePublisher>(s => new RabbitMqPublisher(loggerFactory.CreateLogger<RabbitMqPublisher>(), config, serializer));
        }
    }
}
