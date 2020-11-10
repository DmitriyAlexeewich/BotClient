using BotClient.Models.Enumerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.WebReports
{
    public class DriverRestartReport
    {
        public bool isSuccess { get; set; }

        public EnumSocialPlatform SocialPlatform { get; set; }
    }
}
