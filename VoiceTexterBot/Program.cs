﻿
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using VoiceTexterBot.Configuration;
using VoiceTexterBot.Controllers;
using VoiceTexterBot.Services;

namespace VoiceTexterBot
{
    public class Program
    {

        public static async Task Main()
        {
            Console.OutputEncoding = Encoding.Unicode;

            // Объект, отвечающий за постоянный жизненный цикл приложения
            var host = new HostBuilder()
                .ConfigureServices((hostContext, services) => ConfigureServices(services)) // Задаем конфигурацию
                .UseConsoleLifetime() // Позволяет поддерживать приложение активным в консоли
                .Build(); // Собираем

            Console.WriteLine("Сервис запущен");
            // Запускаем сервис
            await host.RunAsync();
            Console.WriteLine("Сервис остановлен");
        }

        static AppSettings BuildAppSettings()
        {
            return new AppSettings()
            {
                BotToken = "7034725879:AAHJ_kx6Wix-Z10DRScj1vipTxMUKYWBglA"
            };
        }

        //с помощью специального контейнера с зависимостями (IServiceCollection) мы получаем возможность добавить нужные нам компоненты
        //для последующего использования в любых частях приложения. 
        static void ConfigureServices(IServiceCollection services)
        {
            AppSettings appSettings = BuildAppSettings();
            services.AddSingleton(appSettings);

            //хранилище сессий 
            services.AddSingleton<IStorage, MemoryStorage>();

            // Подключаем контроллеры сообщений и кнопок
            services.AddTransient<DefaultMessageController>();
            services.AddTransient<VoiceMessageController>();
            services.AddTransient<TextMessageController>();
            services.AddTransient<InlineKeyboardController>();

            services.AddSingleton<ITelegramBotClient>(provider => new TelegramBotClient("7034725879:AAHJ_kx6Wix-Z10DRScj1vipTxMUKYWBglA"));
            services.AddHostedService<Bot>();

            static AppSettings BuildAppSettings()
            {
                return new AppSettings()
                {
                    DownloadsFolder = "C:\\Users\\evmor\\Downloads",
                    BotToken = "7034725879:AAHJ_kx6Wix-Z10DRScj1vipTxMUKYWBglA",
                    AudioFileName = "audio",
                    InputAudioFormat = "ogg",
                };
            }
        }
    }
}


