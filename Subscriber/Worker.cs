using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Subscriber.Utility;
using Subscriber.Models;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace Subscriber
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
            _channel.ExchangeDeclare(exchange: _options.DeadLetterExchange, type: ExchangeType.Fanout);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                #region consumer
                _channel.QueueDeclare(
                    queue: _options.QueueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: true,
                    arguments: 
                        new Dictionary<string, object>
                        {
                        { "x-dead-letter-exchange", _options.DeadLetterExchange},
                        { "x-message-ttl", 1000 }
                        }
                        );
                _channel.QueueBind(queue: _options.QueueName, exchange: _options.Exchange, routingKey: _options.RoutingKey);

                EventingBasicConsumer consumer = new(_channel);
                consumer.Received += (model, ea) =>
                {
                    byte[] body = ea.Body.ToArray();
                    string message = Encoding.UTF8.GetString(body);
                    
                    if(ea.DeliveryTag % 10 == 0)
                    {
                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: true);
                    }

                    _logger.LogInformation($"Received new message: {message}");
                };

                _channel.BasicConsume(queue: _options.QueueName, consumer: consumer);
                await Task.Delay(1000, stoppingToken);
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                #endregion

                #region deadletterConsumer
                _channel.QueueDeclare(
                    queue: _options.DeadLetterQueue,
                    durable: false,
                    exclusive: false,
                    autoDelete: true,
                    arguments: null);
                _channel.QueueBind(queue: _options.DeadLetterQueue, exchange: _options.DeadLetterExchange, routingKey: string.Empty);

                EventingBasicConsumer deadletterConsumer = new(_channel);
                deadletterConsumer.Received += (model, ea) =>
                {
                    byte[] body = ea.Body.ToArray();
                    string message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation($"Deadletter received new message: {message}");
                };

                _channel.BasicConsume(queue: _options.DeadLetterQueue, autoAck: true, consumer: deadletterConsumer);
                #endregion
            }

            //return Task.CompletedTask;
        }
    }
}
