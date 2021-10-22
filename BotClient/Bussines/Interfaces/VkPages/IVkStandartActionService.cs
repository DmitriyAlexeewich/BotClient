using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces.VkPages
{
    public interface IVkStandartActionService
    {
        Task<bool> CloseModalWindow(Guid WebDriverId);
        Task<bool> CloseMessageBlockWindow(Guid WebDriverId);
    }
}
