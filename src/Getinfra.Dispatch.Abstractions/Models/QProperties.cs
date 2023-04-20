namespace Getinfra.Dispatch.Abstractions.models
{
    public class QProperties
    {
        public QProperties()
        {
            Persistent = true;
        }

        public string UserId { get; set; }
        public string ReplyTo { get; set; }
        public byte Priority { get; set; }
        public string MessageId { get; set; }
        public string Expiration { get; set; }
        public byte DeliveryMode { get; set; }
        public string CorrelationId { get; set; }
        public string ContentType { get; set; }
        public string ContentEncoding { get; set; }
        public string ClusterId { get; set; }
        public string AppId { get; set; }
        public string Type { get; set; }
        public bool Persistent { get; set; }
    }
}
