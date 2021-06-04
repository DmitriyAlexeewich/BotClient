using BotClient.Models.HTMLElements;
using BotDataModels.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.Bot
{
    public class ParsedAudioModel
    {
        public WebHTMLElement AudioElement { get; set; }
        public AudioCreateModel AudioCreate { get; set; }
    }
}
