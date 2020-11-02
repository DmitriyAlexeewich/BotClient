using BotClient.Bussines.Interfaces;
using BotClient.Bussines.Interfaces.MySQL;
using BotClient.Models;
using BotClient.Models.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services.MySQL
{
    public class BotClientRoleConnectorService : IBotClientRoleConnectorService
    {
        private readonly IMySQLService mySQLService;

        public BotClientRoleConnectorService(IMySQLService MySQLService)
        {
            mySQLService = MySQLService;
        }

        public async Task<List<BotClientRoleConnectorModel>> GetAll(int? BotId, int? ClientId, int? RoleId, bool? isComplete, bool? hasNewMessage, bool? hasNewBotMessages)
        {
            var filters = new List<QueryFilter>();
            if (BotId != null)
            {
                filters.Add(new QueryFilter()
                {
                    Table = "BotClientRoleConnector",
                    Column = "BotId",
                    Filter = BotId.ToString()
                });
            }
            if (ClientId != null)
            {
                filters.Add(new QueryFilter()
                {
                    Table = "BotClientRoleConnector",
                    Column = "ClientId",
                    Filter = ClientId.ToString()
                });
            }
            if (RoleId != null)
            {
                filters.Add(new QueryFilter()
                {
                    Table = "BotClientRoleConnector",
                    Column = "RoleId",
                    Filter = RoleId.ToString()
                });
            }
            if (isComplete != null)
            {
                filters.Add(new QueryFilter()
                {
                    Table = "BotClientRoleConnector",
                    Column = "RoleId",
                    Filter = isComplete == true ? "1" : "0"
                });
            }
            if (hasNewMessage != null)
            {
                filters.Add(new QueryFilter()
                {
                    Table = "BotClientRoleConnector",
                    Column = "HasNewMessage",
                    Filter = hasNewMessage == true ? "1" : "0"
                });
            }
            if (hasNewBotMessages != null)
            {
                filters.Add(new QueryFilter()
                {
                    Table = "BotClientRoleConnector",
                    Column = "HasNewBotMessages",
                    Filter = hasNewBotMessages == true ? "1" : "0"
                });
            }
            var botClientRoleConnectionsDataTable = await Select(filters.Count > 0 ? filters : null).ConfigureAwait(false);
            if (botClientRoleConnectionsDataTable.Count > 0)
            {
                return botClientRoleConnectionsDataTable;
            }
            return null;
        }

        public async Task<BotClientRoleConnectorModel> GetByClientVkId(int BotId, string ClientVkId, bool isComplete)
        {
            var query = $"SELECT `BotClientRoleConnector`.* FROM `BotClientRoleConnector` "+
                        $"INNER JOIN `Client` "+
                        $"ON `Client`.`Id`=`BotClientRoleConnector`.`ClientId` "+
                        $"WHERE " +
                        $"`BotClientRoleConnector`.`BotId` = '{BotId}' AND " +
                        $"`BotClientRoleConnector`.`IsComplete`= '{ClientVkId}' AND " +
                        $"`Client`.`VkId`= '{(isComplete == true ? 1 : 0)}'";
            var dataTable = await mySQLService.Select(query).ConfigureAwait(false);
            var result = ConstructBotClientRoleConnectorsList(dataTable);
            if (result.Count > 0)
                return result[0];
            return null;
        }

        public async Task<bool> SetComplete(int Id)
        {
            var query = "UPDATE `BotClientRoleConnector` SET " +
                        $"`UpdateDate`='{DateTime.UtcNow}', " +
                        $"`IsComplete`='1' " +
                        "WHERE " +
                        $"`Id`='{Id}'";
            return await mySQLService.ExecuteNonQuery(query);
        }

        public async Task<bool> SetComplete(int BotId, int ClientId, int RoleId)
        {
            var query = "UPDATE `BotClientRoleConnector` SET " +
                    $"`UpdateDate`='{DateTime.UtcNow}', " +
                    $"`IsComplete`='1' " +
                    "WHERE " +
                    $"`BotId`='{BotId}' " +
                    "AND " +
                    $"`ClientId`='{ClientId}' " +
                    "AND " +
                    $"`RoleId`='{RoleId}'";
            return await mySQLService.ExecuteNonQuery(query);
        }

        public async Task<bool> SetSuccess(int Id, bool isSuccess)
        {
            var query = "UPDATE `BotClientRoleConnector` SET " +
                        $"`UpdateDate`='{DateTime.UtcNow}', " +
                        $"`IsSuccess`='" + (isSuccess == true ? "1" : "0") + "' " +
                        "WHERE " +
                        $"`Id`='{Id}'";
            return await mySQLService.ExecuteNonQuery(query);
        }

        public async Task<bool> SetSuccess(int BotId, int ClientId, int RoleId, bool isSuccess)
        {
            var query = "UPDATE `BotClientRoleConnector` SET " +
                    $"`UpdateDate`='{DateTime.UtcNow}', " +
                    $"`IsSuccess`='" + (isSuccess == true ? "1" : "0") + "' " +
                    "WHERE " +
                    $"`BotId`='{BotId}' " +
                    "AND " +
                    $"`ClientId`='{ClientId}' " +
                    "AND " +
                    $"`RoleId`='{RoleId}'";
            return await mySQLService.ExecuteNonQuery(query);
        }

        public async Task<bool> SetMissionPath(int Id, string MissionPath)
        {
            var query = "UPDATE `BotClientRoleConnector` SET " +
                        $"`UpdateDate`='{DateTime.UtcNow}', " +
                        $"`MissionPath`='{MissionPath}' " +
                        "WHERE " +
                        $"`Id`='{Id}'";
            return await mySQLService.ExecuteNonQuery(query);
        }

        public async Task<bool> SetMissionPath(int BotId, int ClientId, int RoleId, string MissionPath)
        {
            var query = "UPDATE `BotClientRoleConnector` SET " +
                        $"`UpdateDate`='{DateTime.UtcNow}', " +
                        $"`MissionPath`='{MissionPath}' " +
                        "WHERE " +
                        $"`BotId`='{BotId}' " +
                        "AND " +
                        $"`ClientId`='{ClientId}' " +
                        "AND " +
                        $"`RoleId`='{RoleId}'";
            return await mySQLService.ExecuteNonQuery(query);
        }

        public async Task<bool> SetMissionId(int Id, int MissionId)
        {
            var query = "UPDATE `BotClientRoleConnector` SET " +
                        $"`UpdateDate`='{DateTime.UtcNow}', " +
                        $"`MissionId`='{MissionId}' " +
                        "WHERE " +
                        $"`Id`='{Id}'";
            return await mySQLService.ExecuteNonQuery(query);
        }

        public async Task<bool> SetHasNewMessage(int Id, bool hasNewMessage)
        {
            var query = "UPDATE `BotClientRoleConnector` SET " +
                           $"`UpdateDate`='{DateTime.UtcNow}', " +
                           $"`HasNewMessage`='{(hasNewMessage == true ? 1 : 0)}' " +
                           "WHERE " +
                           $"`Id`='{Id}'";
            return await mySQLService.ExecuteNonQuery(query);
        }

        public async Task<bool> SetHasNewBotMessages(int Id, bool hasNewBotMessages)
        {
            var query = "UPDATE `BotClientRoleConnector` SET " +
                               $"`UpdateDate`='{DateTime.UtcNow}', " +
                               $"`HasNewBotMessages`='{(hasNewBotMessages == true ? 1 : 0)}' " +
                               "WHERE " +
                               $"`Id`='{Id}'";
            return await mySQLService.ExecuteNonQuery(query);
        }

        private async Task<List<BotClientRoleConnectorModel>> Select(List<QueryFilter>? Filters)
        {
            var query = "SELECT * FROM `BotClientRoleConnector` ";
            if (Filters != null)
            {
                query += "WHERE ";
                for (int i = 0; i < Filters.Count; i++)
                    query += $"`{Filters[i].Table}`.`{Filters[i].Column}`='{Filters[i].Filter}' AND";
                query = query.Remove(query.Length - 4);
            }
            var dataTable = await mySQLService.Select(query).ConfigureAwait(false);
            return ConstructBotClientRoleConnectorsList(dataTable);
        }

        private List<BotClientRoleConnectorModel> ConstructBotClientRoleConnectorsList(List<List<string>> DataTable)
        {
            var botClientRoleConnections = new List<BotClientRoleConnectorModel>();
            for (int i = 0; i < DataTable.Count; i++)
            {
                botClientRoleConnections.Add(new BotClientRoleConnectorModel()
                {
                    Id = int.Parse(DataTable[i][0]),
                    CreateDate = DateTime.Parse(DataTable[i][1]),
                    UpdateDate = DateTime.Parse(DataTable[i][2]),
                    BotId = int.Parse(DataTable[i][3]),
                    ClientId = int.Parse(DataTable[i][4]),
                    RoleId = int.Parse(DataTable[i][5]),
                    MissionId = int.Parse(DataTable[i][6]),
                    MissionPath = DataTable[i][7],
                    isComplete = DataTable[i][8] == "True" ? true : false,
                    isSuccess = DataTable[i][9] == "True" ? true : false,
                    hasNewMessage = DataTable[i][10] == "True" ? true : false,
                    hasNewBotMessages = DataTable[i][10] == "True" ? true : false
                });
            }
            return botClientRoleConnections;
        }
    }
}
