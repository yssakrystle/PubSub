using Microsoft.Extensions.Options;
using Subscriber.Models;
using RabbitMQ.Client;
using System;

namespace Subscriber.Utility
{
    public class RabbitMQService : IRabbitMQService
    {
        private readonly RabbitMQConfiguration _options;
        private IConnection _connection;

        public RabbitMQService(IOptions<RabbitMQConfiguration> options)
        {
            _options = options.Value;
        }

        public bool IsConnected => _connection != null && _connection.IsOpen;

        public IModel CreateChannel()
        {
            TryConnect();

            if (!IsConnected)
                throw new InvalidOperationException("No RabbitMQ connections are available");
            return _connection.CreateModel();
        }

        private void TryConnect()
        {
            try
            {
                if (IsConnected)
                    return;

                ConnectionFactory factory = new()
                {
                    HostName = _options.Hostname,
                    UserName = _options.UserName,
                    Password = _options.Password
                };

                _connection = factory.CreateConnection();
                _connection.ConnectionShutdown += (s, e) => TryConnect();
                _connection.CallbackException += (s, e) => TryConnect();
                _connection.ConnectionBlocked += (s, e) => TryConnect();
            }
            catch
            {
                throw new InvalidOperationException("No RabbitMQ connections are available");
            }
        }
    }
}
