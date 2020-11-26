using BotClient.Models.Bot;
using BotDataModels.Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.WebReports
{
    public class BotStartReport
    {
        public int BotCount { get; set; }
        public int ErrorBotCount { get; set; }
        public List<BotModel> Bots { get; set; } = new List<BotModel>();
        public List<BotModel> ErrorBots { get; set; } = new List<BotModel>();
        public string ExceptionMessage { get; set; }
    }
}
