using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

// 3/8   11.2. Telegram-бот: создаём проект

//NuGet-пакет Telegram.Bot версия 22.4.4
//NuGet-пакет Microsoft.Extensions.Hosting версия
//(предоставляет ряд классов и методов, позволяющих превратить обычное приложение в постоянно активный сервис, способный обслуживать
//входящие запросы или самостоятельно выполнять требуемые постоянные действия - решает задачу по превращению обычного консольного приложения в постоянно активный сервис)
namespace UtilityBot 
{

    //Теперь Bot.cs наследуется от BackgroundService, а значит, приобретает признаки постоянно активного сервиса.
    //Поскольку родительский класс BackgroundService реализует интерфейс IHostedService, наш бот также получает метод ExecuteAsync(...), 
    //благодаря которому бот сможет быть постоянно активным.

    class Bot : BackgroundService
    {
        /// <summary>
        /// объект, отвеающий за отправку сообщений клиенту
        /// </summary>
        private ITelegramBotClient _telegramClient;

        public Bot(ITelegramBotClient telegramClient)
        {
            _telegramClient = telegramClient;
        }

        //ExecuteAsync активирует нашего бота, запуская его в постоянно активный режим
        //В качестве первых двух входных параметров метод StartReceiving(...) получает делегаты Func<T, …>, которые указывают на уже добавленные
        //методы HandleUpdateAsync и HandleErrorAsync.Благодаря этому вызову должна происходить первичная активация нашего бота на постоянное получение обновлений.
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _telegramClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                new ReceiverOptions() { AllowedUpdates = { } }, // Здесь выбираем, какие обновления хотим получать. В данном случае разрешены все
                cancellationToken: stoppingToken);

            Console.WriteLine("Bot started");
        }

        // Обрабатываем входящие сообщения из Telegram Bot API: https://core.telegram.org/bots/api#message
        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            //  Обрабатываем нажатия на кнопки  из Telegram Bot API: https://core.telegram.org/bots/api#callbackquery
            if (update.Type == UpdateType.CallbackQuery)
            {
                await _telegramClient.SendTextMessageAsync(update.CallbackQuery.From.Id, $"Данный тип сообщений не поддерживается. Пожалуйста отправьте текст.", cancellationToken: cancellationToken);
                return;
            }

            // Обрабатываем входящие сообщения из Telegram Bot API: https://core.telegram.org/bots/api#message
            if (update.Type == UpdateType.Message)
            {
                switch (update.Message!.Type)
                {
                    case MessageType.Text:
                        //считаем количество символов в отправленном текстовом сообщении
                        Console.WriteLine($"Получено сообщение {update.Message.Text}");
                        Console.WriteLine($"Длина сообщения: {update.Message.Text.Length} знаков");
                        await _telegramClient.SendTextMessageAsync(update.Message.From.Id, $"Длина сообщения: {update.Message.Text.Length} знаков", cancellationToken: cancellationToken);
                        return;
                    default: // unsupported message
                        Console.WriteLine($"Данный тип сообщений не поддерживается. Пожалуйста отправьте текст.");
                        await _telegramClient.SendTextMessageAsync(update.Message.From.Id, $"Данный тип сообщений не поддерживается. Пожалуйста отправьте текст.", cancellationToken: cancellationToken);
                        return;
                }
            }
        }

        // Задаем сообщение об ошибке в зависимости от того, какая именно ошибка произошла
        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            // Выводим в консоль информацию об ошибке
            Console.WriteLine(errorMessage);
            Console.WriteLine("Waiting 10 seconds before retry");
            Thread.Sleep(10000);
            return Task.CompletedTask;
        }
    }
}