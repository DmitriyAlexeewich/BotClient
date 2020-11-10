using BotClient.Models.Enumerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.WebReports
{
    public class DriverStartReport
    {
        public bool IsSuccess { get; set; }
        public int BrowserCount { get; set; }
        public EnumSocialPlatform SocialPlatform { get; set; }
    }
}
