using BotClient.Models.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces.MySQL
{
    public interface IBotClientRoleConnectorService
    {
        Task<List<BotClientRoleConnectorModel>> GetAll(int? BotId, int? ClientId, int? RoleId, bool? isComplete, bool? hasNewMessage, bool? hasNewBotMessages);
        Task<BotClientRoleConnectorModel> GetByClientVkId(int BotId, string ClientVkId, bool isComplete);
        Task<bool> SetComplete(int Id);
        Task<bool> SetComplete(int BotId, int ClientId, int RoleId);
        Task<bool> SetSuccess(int Id, bool isSuccess);
        Task<bool> SetSuccess(int BotId, int ClientId, int RoleId, bool isSuccess);
        Task<bool> SetMissionPath(int Id, string MissionPath);
        Task<bool> SetMissionPath(int BotId, int ClientId, int RoleId, string MissionPath);
        Task<bool> SetMissionId(int Id, int MissionId);
        Task<bool> SetHasNewMessage(int Id, bool hasNewMessage);
        Task<bool> SetHasNewBotMessages(int Id, bool hasNewBotMessages);
    }
}
