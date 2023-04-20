using Getinfra.Dispatch.Abstractions.Models;

namespace Getinfra.Dispatch.Abstractions
{
    public interface IQueueConsumer : IBaseService
    {
        event Action<object, QMessage> MessageRecieved;

        void Subscribe();

        void Unsubscribe();

        QMessage Dequeue<T>();

        QMessage Dequeue<T>(bool ack = false);
    }
}
