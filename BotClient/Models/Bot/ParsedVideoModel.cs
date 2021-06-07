using BotClient.Models.HTMLElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.Bot
{
    public class ParsedVideoModel
    {
        public string Name { get; set; }
        public WebHTMLElement VideoElement { get; set; }
    }
}
