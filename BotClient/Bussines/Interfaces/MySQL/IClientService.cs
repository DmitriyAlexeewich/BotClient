using BotClient.Models.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces
{
    public interface IClientService
    {
        Task<ClientModel> GetById(int Id);
        Task<ClientModel> GetByVkId(string VkId);
        Task<bool> Update(ClientModel ClientData);
    }
}
