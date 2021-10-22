using BotClient.Models.Bot;
using BotDataModels.Enumerators;
using BotDataModels.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces
{
    public interface ITextService
    {
        Task<BotMessageText> RandOriginalMessage(string message, string? Name = null, EnumGender? Gender = 0);
        bool IsIncludePatttern(string Pattern, string Text);
        Task<string> GetApologies(BotMessageTextPartModel BotMessageTextPart);
        Task<string> GetApologies();
        Task<string> GetCapsApologies();
        Task<string> InsertText(string Message, string InsertableText = "");
        Task<string> AudioReaction();
        Task<string> GetRememberMessage(int MissionNodeId, List<MissionNodeModel> MissionNodes);
        Task<string> RandMessage(string message);
    }
}
