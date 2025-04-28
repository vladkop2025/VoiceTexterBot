using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityBot.Storage
{
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
