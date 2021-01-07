using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.Bot
{
    public class BotMessageText
    {
        public string Text { get; set; } = null;
        public List<string> TextParts { get; set; } = new List<string>();
        public bool isHasKeyboardError { get; set; } = false;
        public bool isHasCaps { get; set; } = false;
        public List<BotMessageErrorText> BotMessageErrorTexts { get; set; }
    }
}
