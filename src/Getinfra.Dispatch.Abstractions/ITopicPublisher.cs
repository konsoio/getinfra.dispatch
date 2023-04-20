using Getinfra.Dispatch.Abstractions.Models;

namespace Getinfra.Dispatch.Abstractions
{
    public interface ITopicPublisher : IBaseService
    {
        Task Produce(QMessage msg);
    }
}
