using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VoiceTexterBot.Services
{
    public interface IFileHandler
    {
        Task Download(string fileId, CancellationToken ct);
        string Process(string param);
    }
}

//Метод Download(...) будет отвечать за первичное скачивание файла. Скачивание — это длительная асинхронная операция,
//поэтому он будет возвращать объект Task, а принимать идентификатор fileId и уже знакомый вам токен отмены CancellationToken

//Второй метод интерфейса — Process(...) — будет обрабатывать файл (конвертировать и распознавать)

