using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Subscriber.Models;
using Subscriber.Utility;

namespace Subscriber
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    ConfigurationBuilder builder = new();
                    builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    IConfiguration config = builder.Build();

                    services.Configure<RabbitMQConfiguration>(config.GetSection("RabbitMq"));

                    services.AddHostedService<Worker>();
                    services.AddSingleton<IRabbitMQService, RabbitMQService>();
                });
    }
}
