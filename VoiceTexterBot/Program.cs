using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace UtilityBot
{
    static class Program
    {
        public static async Task Main()
        {
            Console.OutputEncoding = Encoding.Unicode;

            var host = new HostBuilder()
                .ConfigureServices((hostContext, services) => ConfigureServices(services))
                .UseConsoleLifetime()
                .Build();

            Console.WriteLine("Starting Service");
            await host.RunAsync();
            Console.WriteLine("Service stopped");
        }

        static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<UtilityBot.Storage.IStorage, UtilityBot.Storage.MemoryStorage>();
            services.AddSingleton<ITelegramBotClient>(provider => new TelegramBotClient("8110263485:AAEjK6WqRL5yTl0Jn_Yxy4JnzkRT54TTy-M"));
            services.AddHostedService<Bot>();
        }
    }
}