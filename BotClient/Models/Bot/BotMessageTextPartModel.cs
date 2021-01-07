using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.Bot
{
    public class BotMessageTextPartModel
    {
        public string Text { get; set; }
        public bool hasCaps { get; set; } = false;
        public bool hasMissClickError { get; set; } = false;
        public string BotMessageCorrectTexts { get; set; } = null;
    }
}
