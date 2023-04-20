using Getinfra.Dispatch.Abstractions.models;

namespace Getinfra.Dispatch.Abstractions.Models
{
    public class QMessage
    {
        /// <summary>
        /// 
        /// </summary>
        public object Body { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public QProperties Properties { get; set; }

        /// <summary>
        /// TODO: define why do we need it for
        /// </summary>
        public ulong DeliveryTag { get; set; }
    }
}
