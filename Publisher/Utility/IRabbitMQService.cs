using RabbitMQ.Client;

namespace Publisher.Utility
{
    public interface IRabbitMQService
    {
        bool IsConnected { get; }
        IModel CreateChannel();
    }
}
