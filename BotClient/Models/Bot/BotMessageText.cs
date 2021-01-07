using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.Bot
{
    public class BotMessageText
    {
        public string Text { get; set; } = null;
        public bool hasMultiplyMissClickError { get; set; } = false;
        public List<BotMessageTextPartModel> TextParts { get; set; } = new List<BotMessageTextPartModel>();
    }
}
