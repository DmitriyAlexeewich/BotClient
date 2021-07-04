using BotClient.Models.Bot.Enumerators;
using BotClient.Models.Enumerators;
using BotDataModels.Bot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.Bot
{
    public class BotWorkStatusModel
    {
        public BotModel BotData { get; set; }
        public Guid WebDriverId { get; set; }
        public List<BotWorkMissionStatus> BotWorkMissionsStatus { get; set; } = new List<BotWorkMissionStatus>();
        public int SubscribeCount { get; set; }
        public int CompletedMissionInitializations { get; set; }
        public DateTime NextDayOnline { get; set; }
    }
}
