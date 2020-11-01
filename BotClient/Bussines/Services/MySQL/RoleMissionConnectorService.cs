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
    public class RoleMissionConnectorService : IRoleMissionConnectorService
    {
        private readonly IMySQLService mySQLService;

        public RoleMissionConnectorService(IMySQLService MySQLService)
        {
            mySQLService = MySQLService;
        }

        public async Task<List<RoleMissionConnectorModel>> GetByRoleId(int RoleId)
        {
            var filters = new List<QueryFilter>();
            filters.Add(new QueryFilter()
            {
                Table = "RoleMissionConnector",
                Column = "RoleId",
                Filter = RoleId.ToString()
            });
            var roleMissionConnectorsDataTable = await Select(filters).ConfigureAwait(false);
            if (roleMissionConnectorsDataTable.Count > 0)
            {
                return roleMissionConnectorsDataTable;
            }
            return null;
        }

        public async Task<List<RoleMissionConnectorModel>> GetByMissionId(int MissionId)
        {
            var filters = new List<QueryFilter>();
            filters.Add(new QueryFilter()
            {
                Table = "RoleMissionConnector",
                Column = "MissionId",
                Filter = MissionId.ToString()
            });
            var roleMissionConnectorsDataTable = await Select(filters).ConfigureAwait(false);
            if (roleMissionConnectorsDataTable.Count > 0)
            {
                return roleMissionConnectorsDataTable;
            }
            return null;
        }

        public async Task<List<RoleMissionConnectorModel>> GetAll(int? RoleId, int? MissionId, bool? isActive)
        {
            var filters = new List<QueryFilter>();
            if (RoleId != null)
            {
                filters.Add(new QueryFilter()
                {
                    Table = "RoleMissionConnector",
                    Column = "RoleId",
                    Filter = RoleId.ToString()
                });
            }
            if (MissionId != null)
            {
                filters.Add(new QueryFilter()
                {
                    Table = "RoleMissionConnector",
                    Column = "MissionId",
                    Filter = MissionId.ToString()
                });
            }
            if (isActive != null)
            {
                filters.Add(new QueryFilter()
                {
                    Table = "RoleMissionConnector",
                    Column = "isActive",
                    Filter = isActive == true ? "1" : "0"
                });
            }
            var roleMissionConnectorsDataTable = await Select(filters.Count > 0 ? filters : null).ConfigureAwait(false);
            if (roleMissionConnectorsDataTable.Count > 0)
            {
                return roleMissionConnectorsDataTable;
            }
            return null;
        }

        private async Task<List<RoleMissionConnectorModel>> Select(List<QueryFilter>? Filters)
        {
            var query = "SELECT * FROM `RoleMissionConnector` ";
            if (Filters != null)
            {
                query += "WHERE ";
                for (int i = 0; i < Filters.Count; i++)
                    query += $"`{Filters[i].Table}`.`{Filters[i].Column}`='{Filters[i].Filter}' AND";
                query = query.Remove(query.Length - 4);
            }
            var dataTable = await mySQLService.Select(query).ConfigureAwait(false);
            var roleMissionConnectors = new List<RoleMissionConnectorModel>();
            for (int i = 0; i < dataTable.Count; i++)
            {
                roleMissionConnectors.Add(new RoleMissionConnectorModel()
                {
                    Id = int.Parse(dataTable[i][0]),
                    CreateDate = DateTime.Parse(dataTable[i][1]),
                    UpdateDate = DateTime.Parse(dataTable[i][2]),
                    RoleId = int.Parse(dataTable[i][3]),
                    MissionId = int.Parse(dataTable[i][4]),
                    isActive = dataTable[i][5] == "True" ? true : false
                });
            }
            return roleMissionConnectors;
        }
    }
}
