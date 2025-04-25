using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using VoiceTexterBot.Configuration;
using VoiceTexterBot.Utilities;

namespace VoiceTexterBot.Services
{
    public class AudioFileHandler : IFileHandler
    {
        private readonly AppSettings _appSettings;
        private readonly ITelegramBotClient _telegramBotClient;

        public AudioFileHandler(ITelegramBotClient telegramBotClient, AppSettings appSettings)
        {
            _appSettings = appSettings;
            _telegramBotClient = telegramBotClient;
        }

        public async Task Download(string fileId, CancellationToken ct)
        {
            // Генерируем полный путь файла из конфигурации
            string inputAudioFilePath = Path.Combine(_appSettings.DownloadsFolder, $"{_appSettings.AudioFileName}.{_appSettings.InputAudioFormat}");

            using (FileStream destinationStream = File.Create(inputAudioFilePath))
            {
                // Загружаем информацию о файле
                var file = await _telegramBotClient.GetFileAsync(fileId, ct);
                if (file.FilePath == null)
                    return;

                // Скачиваем файл
                //В версии Telegram.Bot v22.4.4 метод DownloadFileAsync был удален, и вместо него теперь нужно использовать
                //TelegramBotClient.DownloadFileStreamAsync или скачивание через HttpClient.
                //await _telegramBotClient.DownloadFileAsync(file.FilePath, destinationStream, ct);
                await _telegramBotClient.DownloadFile(file.FilePath, destinationStream, ct);
            }
        }

        public string Process(string languageCode)
        {
            //// Метод пока не реализован
            //throw new NotImplementedException();

            string inputAudioPath = Path.Combine(_appSettings.DownloadsFolder, $"{_appSettings.AudioFileName}.{_appSettings.InputAudioFormat}");
            string outputAudioPath = Path.Combine(_appSettings.DownloadsFolder, $"{_appSettings.AudioFileName}.{_appSettings.OutputAudioFormat}");

            Console.WriteLine("Начинаем конвертацию...");
            AudioConverter.TryConvert(inputAudioPath, outputAudioPath);
            Console.WriteLine("Файл конвертирован");

            return "Конвертация успешно завершена";

        }
    }
}

/*
Для скачивания файла используется объект FileStream, предоставляющий поток файлового вывода. Этот объект должен быть вам знаком по модулю «Работа с файлами».
Перед скачиванием мы сначала готовим файл с помощью модификатора GetFileAsync(…).
Само скачивание возможно при помощи метода DownloadFileAsync(..) библиотеки Telegram.Bot.
Метод Process(...) пока не содержит функционала, его подключим позже. 
*/