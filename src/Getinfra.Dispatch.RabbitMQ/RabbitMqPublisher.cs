using Getinfra.Dispatch.Abstractions;
using Getinfra.Dispatch.Abstractions.Models;
using Getinfra.Dispatch.Abstractions.Serializers;
using Getinfra.Dispatch.RabbitMQ.Models;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Xml;

namespace Getinfra.Dispatch.RabbitMQ
{
    public class RabbitMqPublisher: BaseRabbitMq, IQueuePublisher
    {
        private IJsonSerializer _serializer;
        private readonly RabbitMqConfig _publisherConfig;
        private IConnection _publishConn;
        private IModel _publishChannel;
        private object _lockPublisher = new object();

        public string Name { get; set; }

        public RabbitMqPublisher(ILogger<RabbitMqPublisher> logger, RabbitMqConfig config, IJsonSerializer serializer)
        {
            _serializer = serializer;
            _logger = logger;

            _publisherConfig = config;
            if (string.IsNullOrEmpty(_publisherConfig.Queue))
            {
                _publisherConfig.GenerateNewQueueName(true);
                _publisherConfig.GeneratedQueueName = true;
                _publisherConfig.Exclusive = true;
            }
        }


        public void EnsureConnection()
        {
            if (_publishConn == null || !_publishConn.IsOpen)
            {
                _publishConn = GetConnection(_publisherConfig);
                _publishChannel = Initialize(_publishConn, _publisherConfig);

                _publishConn.ConnectionShutdown += PublisherConnectionShutdown;
            }
        }
        public async Task Enqueue(QMessage msg)
        {


            try
            {
                EnsureConnection();

                IBasicProperties basicProperties = _publishChannel.CreateBasicProperties();

                if (msg.Properties != null)
                {
                    basicProperties.Persistent = msg.Properties.Persistent;
                    if (!string.IsNullOrEmpty(msg.Properties.AppId))
                        basicProperties.AppId = msg.Properties.AppId;
                    if (!string.IsNullOrEmpty(msg.Properties.ClusterId))
                        basicProperties.ClusterId = msg.Properties.ClusterId;
                    if (!string.IsNullOrEmpty(msg.Properties.ContentEncoding))
                        basicProperties.ContentEncoding = msg.Properties.ContentEncoding;
                    if (!string.IsNullOrEmpty(msg.Properties.ContentType))
                        basicProperties.ContentType = msg.Properties.ContentType;
                    if (!string.IsNullOrEmpty(msg.Properties.CorrelationId))
                        basicProperties.CorrelationId = msg.Properties.CorrelationId;
                    if (msg.Properties.DeliveryMode != 0)
                        basicProperties.DeliveryMode = msg.Properties.DeliveryMode;
                    if (!string.IsNullOrEmpty(msg.Properties.Expiration))
                        basicProperties.Expiration = msg.Properties.Expiration;
                    if (!string.IsNullOrEmpty(msg.Properties.MessageId))
                        basicProperties.MessageId = msg.Properties.MessageId;
                    if (msg.Properties.Priority != 0)
                        basicProperties.Priority = msg.Properties.Priority;
                    if (!string.IsNullOrEmpty(msg.Properties.ReplyTo))
                        basicProperties.ReplyTo = msg.Properties.ReplyTo;
                    if (!string.IsNullOrEmpty(msg.Properties.Type))
                        basicProperties.Type = msg.Properties.Type;
                    if (!string.IsNullOrEmpty(msg.Properties.UserId))
                        basicProperties.UserId = msg.Properties.UserId;
                }

                // dead letters support
                //if (_settings.RetryDelay > 0)
                //{
                //    basicProperties.Headers.Add("x-delay", _settings.RetryDelay);
                //}

                lock (_lockPublisher)
                {
                    var jsonified = _serializer.Serialize(msg);
                    var messageBuffer = Encoding.UTF8.GetBytes(jsonified);

                    _publishChannel.BasicPublish(_publisherConfig.Exchange, _publisherConfig.RoutingKey, basicProperties, messageBuffer);

                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Enqueue Error" + ex.Message + "Inner Exception:" + ex.InnerException);
                throw;
            }
        }



        private void PublisherConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            if (e.Initiator == ShutdownInitiator.Application) return;

            _logger.LogInformation("Publisher connection broke!");

            var mres = new ManualResetEventSlim(false); // state is initially false

            while (!mres.Wait(3000)) // loop until state is true, checking every 3s
            {
                try
                {
                    _publishChannel = null;
                    if (_publisherConfig.GeneratedQueueName)
                        _publisherConfig.GenerateNewQueueName(true);
                    _publishConn = GetConnection(_publisherConfig);
                    if (_publishConn == null) throw new Exception("Publisher connection is null");
                    _publishChannel = Initialize(_publishConn, _publisherConfig);
                    _publishConn.ConnectionShutdown += PublisherConnectionShutdown;

                    _logger.LogInformation("Publisher reconnected!");
                    mres.Set(); // state set to true - breaks out of loop
                }
                catch (Exception ex)
                {
                    _logger.LogError("Publisher reconnect failed!, Error: {0}", ex.Message);
                }
            }
        }
    }
}
