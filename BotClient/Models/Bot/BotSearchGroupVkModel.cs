using BotClient.Models.Bot.Enumerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.Bot
{
    public class BotSearchGroupVkModel
    {
        public string Country { get; set; }
        public List<string> City { get; set; }
        public string KeyWord { get; set; }
        public bool FilteredBySubscribersCount { get; set; }
        public bool isSaftySearch { get; set; }
        public EnumSearchGroupType SearchGroupType { get; set; }
        public EnumCityMixType CityMixType { get; set; }
    }
}
