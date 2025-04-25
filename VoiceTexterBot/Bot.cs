using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using VoiceTexterBot.Controllers;

// 4/8   11.3. Telegram-бот: внутренняя архитектура

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

        // Контроллеры различных видов сообщений
        private InlineKeyboardController _inlineKeyboardController;
        private TextMessageController _textMessageController;
        private VoiceMessageController _voiceMessageController;
        private DefaultMessageController _defaultMessageController;

        public Bot(
            ITelegramBotClient telegramClient,
            InlineKeyboardController inlineKeyboardController,
            TextMessageController textMessageController,
            VoiceMessageController voiceMessageController,
            DefaultMessageController defaultMessageController)
        {
            _telegramClient = telegramClient;
            _inlineKeyboardController = inlineKeyboardController;
            _textMessageController = textMessageController;
            _voiceMessageController = voiceMessageController;
            _defaultMessageController = defaultMessageController;
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

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            //  Обрабатываем нажатия на кнопки  из Telegram Bot API: https://core.telegram.org/bots/api#callbackquery
            if (update.Type == UpdateType.CallbackQuery)
            {
                await _inlineKeyboardController.Handle(update.CallbackQuery, cancellationToken);
                return;
            }

            // Обрабатываем входящие сообщения из Telegram Bot API: https://core.telegram.org/bots/api#message
            if (update.Type == UpdateType.Message)
            {
                switch (update.Message!.Type)
                {
                    case MessageType.Voice:
                        await _voiceMessageController.Handle(update.Message, cancellationToken);
                        return;
                    case MessageType.Text:
                        await _textMessageController.Handle(update.Message, cancellationToken);
                        return;
                    default:
                        await _defaultMessageController.Handle(update.Message, cancellationToken);
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

            Console.WriteLine(errorMessage);
            Console.WriteLine("Ожидаем 10 секунд перед повторным подключением.");
            Thread.Sleep(10000);

            return Task.CompletedTask;
        }
    }
}