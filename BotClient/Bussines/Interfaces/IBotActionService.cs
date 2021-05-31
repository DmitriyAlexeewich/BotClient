using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces
{
    public interface IBotActionService
    {
        Task ListenMisuc(Guid WebDriverId, int BotId);
        Task WatchVideo(Guid WebDriverId, int BotId);
    }
}
