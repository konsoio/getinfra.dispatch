using Getinfra.Dispatch.Abstractions.Models;

namespace Getinfra.Dispatch.Abstractions
{
    public interface ITopicConsumer : IBaseService
    {
        event Action<object, QMessage> MessageRecieved;

        void Subscribe(CancellationToken token);

        QMessage Consume<T>();

        QMessage Consume<T>(bool ack = false);
    }
}
