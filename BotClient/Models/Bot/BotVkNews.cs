using BotClient.Models.HTMLElements;
using BotDataModels.Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.Bot
{
    public class BotVkNews
    {
        public BotNewsModel BotNews { get; set; }
        public WebHTMLElement NewsElement { get; set; }
    }
}
