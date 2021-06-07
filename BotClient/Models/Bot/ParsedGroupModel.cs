using BotClient.Models.HTMLElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.Bot
{
    public class ParsedGroupModel
    {
        public string GroupVkId { get; set; }
        public string GroupName { get; set; }
        public int SubscribersCount { get; set; }
        public WebHTMLElement GroupElement { get; set; }
    }
}
