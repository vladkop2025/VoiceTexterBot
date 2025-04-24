using System.Threading.Tasks;
using System.Threading;

namespace VoiceTexterBot.Services
{
    public interface IFileHandler
    {
        Task Download(string fileId, CancellationToken ct);
        string Process(string param);
    }
}