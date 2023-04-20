namespace Getinfra.Dispatch.RabbitMQ.Models
{
    public class RabbitMqConfig
    {
        public string Host { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }

        public string Exchange { get; set; }

        public string Queue { get; set; }

        public string RoutingKey { get; set; }

        public ushort QoS { get; set; }

        public string ExchangeType { get; set; }

        public bool IsDurable { get; set; }

        public bool AutoDelete { get; set; }

        public ushort MessageLimit { get; set; }

        public string Vhost { get; set; }

        public bool Bind { get; set; }

        public bool DeadLetters { get; set; }

        public int Port { get; set; }

        public string Name { get; set; }

        public bool Exclusive { get; set; }

        public bool GeneratedQueueName { get; set; }

        public void GenerateNewQueueName(bool isPublisher)
        {
            string dir = isPublisher ? "pub" : "con";
            string computername = System.Environment.MachineName;
            Random rnd = new Random(DateTime.Now.Millisecond);
            string newQueueName = $"{this.Exchange}.{computername}.{dir}-{rnd.Next(12345678, 99999999)}";
            this.Queue = newQueueName;
            this.Exclusive = true;
        }
    }
}
