using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

//Профессия C#-разработчик Язык C# Модуль 11. Telegram-бот
// 3 / 8   11.2.Telegram - бот: создаём проект
namespace UtilityBot
{
    static class Program
    {
        public static async Task Main()
        {
            Console.OutputEncoding = Encoding.Unicode;

            // Объект, отвечающий за постоянный жизненный цикл приложения
            var host = new HostBuilder()
                .ConfigureServices((hostContext, services) => ConfigureServices(services))
                .UseConsoleLifetime() // Позволяет поддерживать приложение активным в консоли
                .Build(); // Собираем

            Console.WriteLine("Starting Service");
            await host.RunAsync();
            Console.WriteLine("Service stopped");
        }

        static void ConfigureServices(IServiceCollection services)
        {
            // Регистрируем объект TelegramBotClient c токеном подключения
            //На этот раз сообщений об ошибках подключения не наблюдается, бот успешно «подцепился» к Telegram Bot API. Остаётся только написать ему в чат!
            services.AddSingleton<ITelegramBotClient>(provider => new TelegramBotClient("8110263485:AAEjK6WqRL5yTl0Jn_Yxy4JnzkRT54TTy-M"));
            // Регистрируем постоянно активный сервис бота
            services.AddHostedService<Bot>();

           //Здесь мы впервые начинаем применять полноценный паттерн построения современных крупных приложений на .NET Core — Dependency Injection
           //(внедрение зависимостей). Суть его состоит в том, что с помощью специального контейнера с зависимостями(IServiceCollection) мы получаем
           //возможность добавить нужные нам компоненты для последующего использования в любых частях приложения. 
        }
    }
}