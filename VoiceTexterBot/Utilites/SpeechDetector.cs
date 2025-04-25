using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceTexterBot.Extensions;
using Vosk;

namespace VoiceTexterBot.Utilities
{
    public static class SpeechDetector
    {
        public static string DetectSpeech(string audioPath, float inputBitrate, string languageCode)
        {
            //Довольно много места занимают логи Vosk, если вы не хотите их видеть, их можно скрыть, поменяв уровень логирования на -1
            Vosk.Vosk.SetLogLevel(0);
            //Vosk.Vosk.SetLogLevel(-1);
            var modelPath = Path.Combine(DirectoryExtension.GetSolutionRoot(), "Speech-models", $"vosk-model-small-{languageCode.ToLower()}");
            Model model = new(modelPath);
            return GetWords(model, audioPath, inputBitrate);
        }

        /// <summary>
        /// Основной метод для распознавания слов
        /// </summary>
        private static string GetWords(Model model, string audioPath, float inputBitrate)
        {
            // В конструктор для распознавания передаем битрейт, а также используемую языковую модель
            VoskRecognizer rec = new(model, inputBitrate);
            rec.SetMaxAlternatives(0);
            rec.SetWords(true);

            StringBuilder textBuffer = new();

            using (Stream source = File.OpenRead(audioPath))
            {
                byte[] buffer = new byte[4096];
                int bytesRead;

                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // Распознавание отдельных слов
                    if (rec.AcceptWaveform(buffer, bytesRead))
                    {
                        var sentenceJson = rec.Result();
                        // Сохраняем текстовый вывод в JSON-объект и извлекаем данные
                        JObject sentenceObj = JObject.Parse(sentenceJson);
                        string sentence = (string)sentenceObj["text"];
                        textBuffer.Append(StringExtension.UppercaseFirst(sentence) + ". ");
                    }
                }
            }

            // Распознавание предложений
            var finalSentence = rec.FinalResult();
            // Сохраняем текстовый вывод в JSON-объект и извлекаем данные
            JObject finalSentenceObj = JObject.Parse(finalSentence);

            // Собираем итоговый текст
            textBuffer.Append((string)finalSentenceObj["text"]);
            // Возвращаем в виде строки
            return textBuffer.ToString();
        }
    }
}

/*
1. Вся логика распознавания происходит в методе GetWords(...), и на качество распознавания сильно влияют все его входные параметры. На вход здесь 
необходимо передавать модель именно того языка, который используется.

2. Также очень важное значение имеет битрейт распознаваемого аудиофайла (параметр inputBitrate). При выборе этого значения следует руководствоваться 
значением в распознаваемом файле — его можно посмотреть в свойствах: 
*/