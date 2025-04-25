using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceTexterBot.Models
{
    public class Session
    {
        //Этот язык пользователь будет выбирать перед отправкой аудиосообщения, чтобы бот понимал, на каком языке предстоит расшифровать аудио.
        public string LanguageCode { get; set; }
    }
}
