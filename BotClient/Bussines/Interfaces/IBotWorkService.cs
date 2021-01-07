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
        Task StartBot(Guid WebDriverId, int ServerId);
        Task<List<BotRoleActionsDaySchedule>> GetBotRoleActions();
        Task<List<string>> GetRandomMessages();
        Task<string> Test(string Text);
    }
}
