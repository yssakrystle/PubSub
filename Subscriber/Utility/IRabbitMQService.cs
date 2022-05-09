using RabbitMQ.Client;

namespace Subscriber.Utility
{
    public interface IRabbitMQService
    {
        bool IsConnected { get; }
        IModel CreateChannel();
    }
}
