using BotClient.Models.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces.MySQL
{
    public interface IMessagesService
    {
        Task<List<MessageModel>> GetAll(int BotClientConnectionId, int? Id);

        Task<MessageModel> GetById(int Id);

        Task<bool> CreateMessage(int BotClientConnectionId, string Text, bool isBotMessage);
    }
}
