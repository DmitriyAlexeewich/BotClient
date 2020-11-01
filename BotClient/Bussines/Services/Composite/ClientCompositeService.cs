using BotClient.Bussines.Interfaces;
using BotClient.Bussines.Interfaces.Composite;
using BotClient.Bussines.Interfaces.MySQL;
using BotClient.Models.Client;
using BotClient.Models.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services.Composite
{
    public class ClientCompositeService : IClientCompositeService
    {
        private readonly IClientService clientService;
        private readonly IMessagesService messageService;
        private readonly IBotClientRoleConnectorService botClientRoleConnector;
        private readonly IRoleMissionConnectorService roleMissionConnectorService;

        public ClientCompositeService(IClientService ClientService, 
                                      IMessagesService MessagesService, 
                                      IBotClientRoleConnectorService BotClientRoleConnector,
                                      IRoleMissionConnectorService RoleMissionConnectorService)
        {
            clientService = ClientService;
            messageService = MessagesService;
            botClientRoleConnector = BotClientRoleConnector;
            roleMissionConnectorService = RoleMissionConnectorService;
        }

        public async Task<List<BotClientRoleConnectorModel>> GetBotClientRoleConnection(int BotId, int RoleId, bool? isComplete = false)
        {
            var connections = await botClientRoleConnector.GetAll(BotId, null, RoleId, isComplete).ConfigureAwait(false);
            return connections;
        }

        public async Task<BotClientRoleConnectorModel> GetBotClientRoleConnection(int BotId, string ClientVkId, bool? isComplete = false)
        {
            var connections = await botClientRoleConnector.GetByClientVkId(BotId, ClientVkId, isComplete.Value).ConfigureAwait(false);
            return connections;
        }

        public async Task<ClientModel> GetClientById(int ClientId)
        {
            var client = await clientService.GetById(ClientId).ConfigureAwait(false);
            return client;
        }

        public async Task<List<MessageModel>> GetMessagesByConnectionId(int BotClientConnectionId)
        {
            var messages = await messageService.GetAll(BotClientConnectionId,null).ConfigureAwait(false);
            return messages;
        }

        public async Task<List<RoleMissionConnectorModel>> GetRoleMissionConnections(int RoleId, bool? isActive)
        {
            var missions = await roleMissionConnectorService.GetByRoleId(RoleId).ConfigureAwait(false);
            if (isActive != null)
                missions = missions.Where(item => item.isActive == isActive).ToList();
            return missions;
        }

        public async Task<bool> UpdateClientData(ClientModel ClientData)
        {
            var result = await clientService.Update(ClientData).ConfigureAwait(false);
            return result;
        }

        public async Task<bool> CreateMessage(int BotClientConnectionId, string Text, bool? isBotMessage = true)
        {
            var result = await messageService.CreateMessage(BotClientConnectionId, Text, isBotMessage.Value).ConfigureAwait(false);
            return result;
        }

        public async Task<bool> SetClientComplete(int BotClientConnectionId)
        {
            var result = await botClientRoleConnector.SetComplete(BotClientConnectionId).ConfigureAwait(false);
            return result;
        }

        public async Task<bool> SetClientComplete(int BotId, int ClientId, int RoleId)
        {
            var result = await botClientRoleConnector.SetComplete(BotId, ClientId, RoleId).ConfigureAwait(false);
            return result;
        }

        public async Task<bool> SetSuccess(int BotClientConnectionId, bool isSuccess)
        {
            var result = await botClientRoleConnector.SetSuccess(BotClientConnectionId, isSuccess).ConfigureAwait(false);
            return result;
        }

        public async Task<bool> SetSuccess(int BotId, int ClientId, int RoleId, bool isSuccess)
        {
            var result = await botClientRoleConnector.SetSuccess(BotId, ClientId, RoleId, isSuccess).ConfigureAwait(false);
            return result;
        }

        public async Task<bool> SetClientMissionPath(int BotClientConnectionId, string MissionPath)
        {
            var result = await botClientRoleConnector.SetMissionPath(BotClientConnectionId, MissionPath).ConfigureAwait(false);
            return result;
        }

        public async Task<bool> SetClientMissionPath(int BotId, int ClientId, int RoleId, string MissionPath)
        {
            var result = await botClientRoleConnector.SetMissionPath(BotId, ClientId, RoleId, MissionPath).ConfigureAwait(false);
            return result;
        }

        public async Task<bool> SetMissionId(int Id, int MissionId)
        {
            var result = await botClientRoleConnector.SetMissionId(Id, MissionId).ConfigureAwait(false);
            return result;
        }

        public async Task<bool> SetHasNewMessage(int Id, bool? hasNewMessage = true)
        {
            var result = await botClientRoleConnector.SetHasNewMessage(Id, hasNewMessage.Value).ConfigureAwait(false);
            return result;
        }
    }
}
