using BotClient.Models.Client;
using BotClient.Models.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces.Composite
{
    public interface IClientCompositeService
    {
        Task<List<BotClientRoleConnectorModel>> GetBotClientRoleConnection(int BotId, int? RoleId = null, bool? isComplete = false, bool? hasNewMessage = null, bool? hasNewBotMessage = null);
        Task<BotClientRoleConnectorModel> GetBotClientRoleConnection(int BotId, string ClientVkId, bool? isComplete = false);
        Task<ClientModel> GetClientById(int ClientId);
        Task<List<MessageModel>> GetMessagesByConnectionId(int BotClientConnectionId);
        Task<List<RoleMissionConnectorModel>> GetRoleMissionConnections(int RoleId, bool? isActive);
        Task<bool> UpdateClientData(ClientModel ClientData);
        Task<bool> CreateMessage(int BotClientConnectionId, string Text, bool? isBotMessage = true);
        Task<bool> SetClientComplete(int BotClientConnectionId);
        Task<bool> SetClientComplete(int BotId, int ClientId, int RoleId);
        Task<bool> SetSuccess(int BotClientConnectionId, bool isSuccess);
        Task<bool> SetSuccess(int BotId, int ClientId, int RoleId, bool isSuccess);
        Task<bool> SetClientMissionPath(int BotClientConnectionId, string MissionPath);
        Task<bool> SetClientMissionPath(int BotId, int ClientId, int RoleId, string MissionPath);
        Task<bool> SetMissionId(int Id, int MissionId);
        Task<bool> SetHasNewMessage(int Id, bool? hasNewMessage = true);
        Task<bool> SetHasNewBotMessages(int Id, bool? hasNewBotMessages = true);
    }
}
