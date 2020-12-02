using BotClient.Models.Bot;
using BotClient.Models.WebReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces
{
    public interface IBotWorkService
    {
        Task<BotStartReport> StartBot(List<int> BotsId);
        Task<BotStopQueryReport> StopBot(List<int> BotsId);
        Task<List<BotWorkStatusModel>> GetBots();
        Task<List<BotRoleActionsDaySchedule>> GetBotRoleActions();
        Task<List<string>> GetRandomMessages();
    }
}
