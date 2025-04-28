using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

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
        private IStorage _memoryStorage;

        public Bot(ITelegramBotClient telegramClient, IStorage memoryStorage)
        {
            _telegramClient = telegramClient;
            _memoryStorage = memoryStorage;
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
            if (update.Type == UpdateType.CallbackQuery)
            {
                await HandleCallbackQuery(botClient, update.CallbackQuery, cancellationToken);
                return;
            }

            // Обрабатываем входящие сообщения из Telegram Bot API: https://core.telegram.org/bots/api#message
            if (update.Type == UpdateType.Message)
            {
                await HandleMessage(botClient, update.Message, cancellationToken);
                return;
            }
        }

        //// Обрабатываем входящие сообщения из Telegram Bot API: https://core.telegram.org/bots/api#message
        //async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        //{
        //    //  Обрабатываем нажатия на кнопки  из Telegram Bot API: https://core.telegram.org/bots/api#callbackquery
        //    if (update.Type == UpdateType.CallbackQuery)
        //    {
        //        await _telegramClient.SendTextMessageAsync(update.CallbackQuery.From.Id, $"Данный тип сообщений не поддерживается. Пожалуйста отправьте текст.", cancellationToken: cancellationToken);
        //        return;
        //    }

        //    // Обрабатываем входящие сообщения из Telegram Bot API: https://core.telegram.org/bots/api#message
        //    if (update.Type == UpdateType.Message)
        //    {
        //        switch (update.Message!.Type)
        //        {
        //            case MessageType.Text:
        //                //считаем количество символов в отправленном текстовом сообщении
        //                Console.WriteLine($"Получено сообщение {update.Message.Text}");
        //                Console.WriteLine($"Длина сообщения: {update.Message.Text.Length} знаков");
        //                await _telegramClient.SendTextMessageAsync(update.Message.From.Id, $"Длина сообщения: {update.Message.Text.Length} знаков", cancellationToken: cancellationToken);
        //                return;
        //            default: // unsupported message
        //                Console.WriteLine($"Данный тип сообщений не поддерживается. Пожалуйста отправьте текст.");
        //                await _telegramClient.SendTextMessageAsync(update.Message.From.Id, $"Данный тип сообщений не поддерживается. Пожалуйста отправьте текст.", cancellationToken: cancellationToken);
        //                return;
        //        }
        //    }
        //}

        private async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Data == null)
                return;

            _memoryStorage.GetSession(callbackQuery.From.Id).Functionality = callbackQuery.Data;

            string text = callbackQuery.Data switch
            {
                "count" => "Отправьте текст для подсчета символов",
                "sum" => "Отправьте числа, разделенные пробелами, для вычисления суммы",
                _ => "Выберите функцию:"
            };

            await botClient.SendTextMessageAsync(
                callbackQuery.From.Id,
                text,
                cancellationToken: cancellationToken);
        }

        //// Задаем сообщение об ошибке в зависимости от того, какая именно ошибка произошла
        //Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        //{
        //    var errorMessage = exception switch
        //    {
        //        ApiRequestException apiRequestException
        //            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        //        _ => exception.ToString()
        //    };

        //    // Выводим в консоль информацию об ошибке
        //    Console.WriteLine(errorMessage);
        //    Console.WriteLine("Waiting 10 seconds before retry");
        //    Thread.Sleep(10000);
        //    return Task.CompletedTask;
        //}

        private async Task HandleMessage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if (message.Text == null)
                return;

            if (message.Text == "/start")
            {
                var buttons = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Подсчет символов", "count"),
                        InlineKeyboardButton.WithCallbackData("Сумма чисел", "sum")
                    }
                });

                await botClient.SendTextMessageAsync(
                    message.From.Id,
                    "Выберите функцию:",
                    replyMarkup: buttons,
                    cancellationToken: cancellationToken);
                return;
            }

            var session = _memoryStorage.GetSession(message.From.Id);
            string result = string.Empty;

            switch (session.Functionality)
            {
                case "count":
                    result = $"В вашем сообщении {message.Text.Length} символов";
                    break;
                case "sum":
                    result = CalculateSum(message.Text);
                    break;
                default:
                    result = "Сначала выберите функцию в главном меню (/start)";
                    break;
            }

            await botClient.SendTextMessageAsync(
                message.From.Id,
                result,
                cancellationToken: cancellationToken);
        }

        private string CalculateSum(string input)
        {
            string[] numbers = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int sum = 0;

            foreach (string number in numbers)
            {
                if (int.TryParse(number, out int num))
                {
                    sum += num;
                }
                else
                {
                    return $"Ошибка: '{number}' не является числом";
                }
            }

            return $"Сумма чисел: {sum}";
        }

        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
            Console.WriteLine("Waiting 10 seconds before retry");
            Thread.Sleep(10000);
            return Task.CompletedTask;
        }

    }
    public interface IStorage
    {
        Session GetSession(long chatId);
    }

    public class MemoryStorage : IStorage
    {
        private readonly Dictionary<long, Session> _sessions = new Dictionary<long, Session>();

        public Session GetSession(long chatId)
        {
            if (!_sessions.ContainsKey(chatId))
            {
                _sessions[chatId] = new Session() { Functionality = "" };
            }

            return _sessions[chatId];
        }
    }

    public class Session
    {
        public string Functionality { get; set; }
    }
}