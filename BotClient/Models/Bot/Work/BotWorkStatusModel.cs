using BotClient.Models.Bot.Enumerators;
using BotClient.Models.Enumerators;
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
        public int RoleId { get; set; }
        public EnumBotWorkStatus WorkStatus { get; set; }
        public EnumAlgoritmName LastAlgoritm { get; set; }
        public Guid WebDriverId { get; set; }
    }
}
