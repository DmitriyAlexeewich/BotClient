using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.Settings
{
    public class WebConnectionSettings
    {
        public Guid ServerId { get; set; }
        public string ParentServerIP { get; set; }
        public List<string> Options { get; set; } = new List<string>();
        public int KeyWaitingTimeMin { get; set; }
        public int KeyWaitingTimeMax { get; set; }
        public int HTMLPageWaitingTime { get; set; }
        public int HTMLElementWaitingTime { get; set; }
        public int ScrollCount { get; set; }
        public int ErrorChancePerTenWords { get; set; }
        public int CapsChancePerThousandWords { get; set; }
        public int NumberChancePerHundredWords { get; set; }
        public int MusicWaitingTime { get; set; }
        public int MusicWaitingDeltaTime { get; set; }
        public int MusicLoadingWaitingTime { get; set; }
        public int VideoWaitingTime { get; set; }
        public int VideoWaitingDeltaTime { get; set; }
        public int VideoLoadingWaitingTime { get; set; }
    }
}
