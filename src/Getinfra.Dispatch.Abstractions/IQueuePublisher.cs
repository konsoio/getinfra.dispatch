using Getinfra.Dispatch.Abstractions.Models;

namespace Getinfra.Dispatch.Abstractions
{
    public interface IQueuePublisher : IBaseService
    {
        
        Task Enqueue(QMessage msg);
    }
}
