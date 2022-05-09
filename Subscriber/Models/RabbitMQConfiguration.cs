namespace Subscriber.Models
{
    public class RabbitMQConfiguration
    {
        public string Hostname { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Exchange { get; set; }
        public string QueueName { get; set; }
        public string DeadLetterExchange { get; set; }
        public string DeadLetterQueue { get; set; }
        public string RoutingKey { get; set; }
    }
}
