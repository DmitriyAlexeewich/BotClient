using BotClient.Models.Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces
{
    public interface IBotCustomizeService
    {
        Task<BotCustomizeModel> GetById(int Id);
        Task<BotCustomizeModel> GetByBotId(int BotId);
    }
}
