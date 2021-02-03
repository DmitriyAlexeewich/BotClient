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
        public int ErrorChanceInWords { get; set; }
        public int CapsChancePerThousandWords { get; set; }
        public int NumberChancePerHundredWords { get; set; }

        public int MinSpaceCountToSplit { get; set; }
        public int MaxSpaceCountToSplit { get; set; }
        public int PlotCommaSplitChance { get; set; }
        public int MinAtteptCountToRandMessage { get; set; }
        public int MaxAtteptCountToRandMessage { get; set; }
        public int UseDateTimeHelloPhraseChance { get; set; }
        public int UseContactPhraseChance { get; set; }
        public int UseNameContactChance { get; set; }
        public int MinSubscribeCount { get; set; }
        public int MaxSubscribeCount { get; set; }
        public int RepostChancePerDay { get; set; }
        public int MinRoleActionCountPerDay { get; set; }
        public int MaxRoleActionCountPerDay { get; set; }
        public int LoginWaitingTime { get; set; }
        public int MinRoleActionCountPerSession { get; set; }
        public int MaxRoleActionCountPerSession { get; set; }
        public int MinNightSecondActionCountPerSession { get; set; }
        public int MaxNightSecondActionCountPerSession { get; set; }


        public int MusicWaitingTime { get; set; }
        public int MusicWaitingDeltaTime { get; set; }
        public int MusicLoadingWaitingTime { get; set; }
        public int VideoWaitingTime { get; set; }
        public int VideoWaitingDeltaTime { get; set; }
        public int VideoLoadingWaitingTime { get; set; }
    }
}
