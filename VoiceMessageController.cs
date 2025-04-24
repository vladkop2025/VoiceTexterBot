using Telegram.Bot;
using Telegram.Bot.Types;

namespace VoiceTexterBot.Controllers
{
    public class VoiceMessageController
    {
        private readonly ITelegramBotClient _telegramClient;

        // VoiceMessageController.cs
        public async Task Handle(Message message, CancellationToken ct)
        {
            Console.WriteLine($"Контроллер {GetType().Name} получил сообщение");
            await _telegramClient.SendTextMessageAsync(message.Chat.Id, $"Получено текстовое сообщение", cancellationToken: ct);
        }
    }
}