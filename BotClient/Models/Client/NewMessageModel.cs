using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.Client
{
    public class NewMessageModel
    {
        public string Text { get; set; }
        public string AttachedText { get; set; }
        public string ReceiptMessageDatePlatformFormat { get; set; }
        public bool hasAudio { get; set; } = false;
    }
}
