using BotClient.Models.Bot;
using BotDataModels.Enumerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces
{
    public interface ITextService
    {
        Task<BotMessageText> RandOriginalMessage(string message, string? Name = null, EnumGender? Gender = 0);
        string TextToRegex(string Text);
        Task<string> GetApologies(BotMessageTextPartModel BotMessageTextPart);
        Task<string> GetApologies();
        Task<string> GetCapsApologies();
    }
}
