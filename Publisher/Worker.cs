using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Publisher.Models;
using Publisher.Utility;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Publisher
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IRabbitMQService _connection;
        private readonly RabbitMQConfiguration _options;
        private IModel _channel;
      
        public Worker(ILogger<Worker> logger, IRabbitMQService connection, IOptions<RabbitMQConfiguration> options)
        {
            _logger = logger;
            _options = options.Value;

            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _channel = _connection.CreateChannel();
            _channel.ExchangeDeclare(exchange: _options.Exchange, type: ExchangeType.Direct);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Worker running at: {DateTimeOffset.Now}");
                
                int messageId = 1;
                while (true)
                {
                    string message = $"Sent Message Id: {messageId}";
                    byte[] body = Encoding.UTF8.GetBytes(message);

                    _channel.BasicPublish(
                           exchange: _options.Exchange,
                           routingKey: _options.RoutingKey,
                           mandatory: true,
                           body: body);

                    _logger.LogInformation($"{message}");
                    messageId++;
                }
            }

            return Task.CompletedTask;
        }
    }
}
