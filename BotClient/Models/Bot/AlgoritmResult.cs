using BotClient.Models.Bot.Work.Enumerators;
using BotClient.Models.HTMLElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.Bot.Work
{
    public class AlgoritmResult
    {
        public WebHTMLElementModel WebElement { get; set; }
        public EnumActionResult ActionResultMessage { get; set; }
        public bool hasError { get; set; }
    }
}
