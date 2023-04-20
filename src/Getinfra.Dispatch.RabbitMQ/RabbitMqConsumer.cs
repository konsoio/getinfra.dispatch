using Getinfra.Dispatch.Abstractions;
using Getinfra.Dispatch.Abstractions.models;
using Getinfra.Dispatch.Abstractions.Models;
using Getinfra.Dispatch.Abstractions.Serializers;
using Getinfra.Dispatch.RabbitMQ.Models;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Text;

namespace Getinfra.Dispatch.RabbitMQ
{
    public class RabbitMqConsumer : BaseRabbitMq, IQueueConsumer
    {
        private IJsonSerializer _serializer;
        private readonly RabbitMqConfig _consumerConfig;
        private IConnection _consumerConn;
        private IModel _consumerChannel;
        private EventingBasicConsumer _consumer;
        private object _lockSubscriber = new object();

        public string Name { get; set; }

        public RabbitMqConsumer(ILogger<RabbitMqConsumer> logger, RabbitMqConfig config, IJsonSerializer serializer)
        {
            _serializer = serializer;
            _logger = logger;
            _consumerConfig = config;
            if (string.IsNullOrEmpty(_consumerConfig.Queue))
            {
                _consumerConfig.GenerateNewQueueName(false);
                _consumerConfig.GeneratedQueueName = true;
                _consumerConfig.Exclusive = true;
            }


        }

        public event Action<object, QMessage> MessageRecieved;

        public QMessage Dequeue<T>()
        {
            return Dequeue<T>(true);
        }

        public void EnsureConnection()
        {
            if (_consumerConn == null || !_consumerConn.IsOpen)
            {
                _consumerConn = GetConnection(_consumerConfig);
                _consumerChannel = Initialize(_consumerConn, _consumerConfig);
                _consumerConn.ConnectionShutdown += ConsumerConnectionShutdown;
            }
        }

        public QMessage Dequeue<T>(bool ack = false)
        {
            QMessage msg = null;
            BasicGetResult result;
            try
            {
                EnsureConnection();
                //IBasicProperties basicProperties = ConsumerChannel.CreateBasicProperties();
                //basicProperties.Persistent = true;
                lock (_lockSubscriber)
                {
                    bool noAck = false;
                    // get message from queue
                    result = _consumerChannel.BasicGet(_consumerConfig.Queue, noAck);
                    if (result == null)
                    {
                        // No message available at this time.
                    }
                    else
                    {
                        IBasicProperties props = result.BasicProperties;

                        // get body
                        byte[] body = result.Body.ToArray();

                        var json = Encoding.UTF8.GetString(body);


                        msg = _serializer.Deserialize<QMessage>(json);
                        msg.DeliveryTag = result.DeliveryTag;
                        msg.Properties = new QProperties()
                        {
                            AppId = props.AppId,
                            ClusterId = props.ClusterId,
                            ContentEncoding = props.ContentEncoding,
                            ContentType = props.ContentType,
                            CorrelationId = props.CorrelationId,
                            DeliveryMode = props.DeliveryMode,
                            Expiration = props.Expiration,
                            MessageId = props.MessageId,
                            Priority = props.Priority,
                            ReplyTo = props.ReplyTo,
                            Type = props.Type,
                            UserId = props.UserId
                        };

                        if (ack)
                        {
                            _consumerChannel.BasicAck(result.DeliveryTag, false);
                        }

                    }
                }
            }
            catch (OperationInterruptedException ex)
            {
                _logger.LogCritical($"Dequeue Error {ex.Message},Inner Exception:{ex.InnerException}, Stack: {ex.StackTrace}");
                throw;
            }

            return msg;
        }

        public void Subscribe()
        {
            _logger.LogInformation("Subscribe: Starting...");

            EnsureConnection();

            Connect();
        }

        public void Unsubscribe()
        {
            if (_consumer != null)
                _consumer.Received -= (model, ea) => { };

            if (_consumerConn != null)
                _consumerConn.ConnectionShutdown -= (sender, e) => { };
        }

        private void ConsumerConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            if (e.Initiator == ShutdownInitiator.Application) return;

            _logger.LogInformation("OnShutDown: Consumer Connection broke!");

            CleanupConnection((IConnection)sender);

            var mres = new ManualResetEventSlim(false); // state is initially false

            while (!mres.Wait(3000)) // loop until state is true, checking every 3s
            {
                try
                {
                    _consumerChannel = null;
                    if (_consumerConfig.GeneratedQueueName)
                        _consumerConfig.GenerateNewQueueName(false);
                    _consumerConn = GetConnection(_consumerConfig);
                    if (_consumerConn == null) throw new Exception("Consumer connection is null");
                    _consumerChannel = Initialize(_consumerConn, _consumerConfig);

                    Subscribe();

                    _consumerConn.ConnectionShutdown += ConsumerConnectionShutdown;
                    _logger.LogInformation("Consumer Reconnected!");
                    mres.Set(); // state set to true - breaks out of loop
                }
                catch (Exception ex)
                {
                    _logger.LogError("Consumer reconnect failed!, Error: {0}", ex.Message);
                }
            }
        }

        private void CleanupConnection(IConnection conn)
        {
            if (conn != null && conn.IsOpen)
            {
                conn.Close();
            }
        }


        private void Connect()
        {
            var logString = $"hostname: {_consumerConfig.Host}, username: {_consumerConfig.Username}, password: {_consumerConfig.Password}, exchangeName: {_consumerConfig.Exchange}, " +
                $"queueName: {_consumerConfig.Queue}, isDurable: {_consumerConfig.IsDurable}, isAutodelete: {_consumerConfig.AutoDelete}, routingKey: {_consumerConfig.RoutingKey}";


            _logger.LogInformation("Connect: Connecting for {0}", logString);

            _consumer = new EventingBasicConsumer(_consumerChannel);

            _consumer.Received += (model, ea) =>
            {
                try
                {
                    _logger.LogInformation("Consume: Calling handler for {0}", logString);

                    IBasicProperties props = ea.BasicProperties;

                    // get body
                    byte[] body = ea.Body.ToArray();

                    var json = Encoding.UTF8.GetString(body);

                    var msg = _serializer.Deserialize<QMessage>(json);

                    msg.Properties = new QProperties()
                    {
                        AppId = props.AppId,
                        ClusterId = props.ClusterId,
                        ContentEncoding = props.ContentEncoding,
                        ContentType = props.ContentType,
                        CorrelationId = props.CorrelationId,
                        DeliveryMode = props.DeliveryMode,
                        Expiration = props.Expiration,
                        MessageId = props.MessageId,
                        Priority = props.Priority,
                        ReplyTo = props.ReplyTo,
                        Type = props.Type,
                        UserId = props.UserId
                    };

                    if (MessageRecieved != null)
                        MessageRecieved(model, msg);

                    //callBack(model, msg); // return byte array
                    _consumerChannel.BasicAck(ea.DeliveryTag, false);
                    _logger.LogInformation("Consume: Acknowledged for {0}", logString);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Consume: Error {0}, Stack: {1}", ex.Message, ex.StackTrace);
                }

            };


            _consumerChannel.BasicConsume(queue: _consumerConfig.Queue, autoAck: false,
                consumer: _consumer);

            _logger.LogInformation("Consume: Connected for {0}", logString);
        }
    }
}