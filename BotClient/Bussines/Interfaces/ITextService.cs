using BotClient.Models.Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces
{
    public interface ITextService
    {
        Task<BotMessageText> RandOriginalMessage(string message);
        string TextToRegex(string Text);
        Task<string> GetApologies(BotMessageTextPartModel BotMessageTextPart);
        Task<string> GetApologies();
        Task<string> GetCapsApologies();
    }
}
