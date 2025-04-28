
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using UtilityBot.Storage;  // Added reference to our storage namespace

namespace UtilityBot
{
    class Bot : BackgroundService
    {
        private ITelegramBotClient _telegramClient;
        private IStorage _memoryStorage;

        public Bot(ITelegramBotClient telegramClient, IStorage memoryStorage)
        {
            _telegramClient = telegramClient;
            _memoryStorage = memoryStorage;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _telegramClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                new ReceiverOptions() { AllowedUpdates = { } },
                cancellationToken: stoppingToken);

            Console.WriteLine("Bot started");
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.CallbackQuery)
            {
                await HandleCallbackQuery(botClient, update.CallbackQuery, cancellationToken);
                return;
            }

            if (update.Type == UpdateType.Message)
            {
                await HandleMessage(botClient, update.Message, cancellationToken);
                return;
            }
        }

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
                    Console.WriteLine($"В вашем сообщении {message.Text.Length} символов");
                    break;
                case "sum":
                    result = CalculateSum(message.Text);
                    Console.WriteLine(result);
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
}