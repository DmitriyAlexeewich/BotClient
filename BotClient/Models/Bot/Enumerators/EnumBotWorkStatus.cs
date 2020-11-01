using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.Bot.Enumerators
{
    public enum EnumBotWorkStatus
    {
        Free = 1,
        Run = 2,
        StopQuery = 3,
        Stop = 4,
        Error = 5
    }
}
