using BotClient.Models.HTMLElements;
using BotDataModels.Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.Bot
{
    public class BotVkVideo
    {
        public BotVideoModel BotVideo { get; set; }
        public string VkId { get; set; }
        public WebHTMLElement HTMLElement { get; set; }
    }
}
