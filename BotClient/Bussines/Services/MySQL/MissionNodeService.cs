using BotClient.Bussines.Interfaces;
using BotClient.Models;
using BotClient.Models.Role;
using BotClient.Models.Role.Enumerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services
{
    public class MissionNodeService : IMissionNodeService
    {
        private readonly IMySQLService mySQLService;

        public MissionNodeService(IMySQLService MySQLService)
        {
            mySQLService = MySQLService;
        }

        public async Task<List<MissionNodeModel>> GetByMissionId(int MissionId)
        {
            var filters = new List<QueryFilter>();
            filters.Add(new QueryFilter()
            {
                Table = "MissionNode",
                Column = "MissionId",
                Filter = MissionId.ToString()
            });
            var missionDataTable = await Select(filters).ConfigureAwait(false);
            if (missionDataTable.Count > 0)
            {
                return missionDataTable;
            }
            return null;
        }

        public async Task<List<MissionNodeModel>> GetAll(int? MissionId, int? NodeId, int? PatternId, string? Path)
        {
            var filters = new List<QueryFilter>();
            if (MissionId != null)
            {
                filters.Add(new QueryFilter()
                {
                    Table = "MissionNode",
                    Column = "MissionId",
                    Filter = MissionId.ToString()
                });
            }
            if (NodeId != null)
            {
                filters.Add(new QueryFilter()
                {
                    Table = "MissionNode",
                    Column = "NodeId",
                    Filter = NodeId.ToString()
                });
            }
            if (PatternId != null)
            {
                filters.Add(new QueryFilter()
                {
                    Table = "MissionNode",
                    Column = "PatternId",
                    Filter = PatternId.ToString()
                });
            }
            if (Path != null)
            {
                filters.Add(new QueryFilter()
                {
                    Table = "MissionNode",
                    Column = "Path",
                    Filter = Path
                });
            }
            var missionNodesDataTable = await Select(filters.Count > 0 ? filters : null).ConfigureAwait(false);
            if (missionNodesDataTable.Count > 0)
            {
                return missionNodesDataTable;
            }
            return null;
        }

        private async Task<List<MissionNodeModel>> Select(List<QueryFilter>? Filters)
        {
            var query = "SELECT * FROM `MissionNode` ";
            if (Filters != null)
            {
                query += "WHERE ";
                for (int i = 0; i < Filters.Count; i++)
                    query += $"`{Filters[i].Table}`.`{Filters[i].Column}`='{Filters[i].Filter}' AND";
                query = query.Remove(query.Length - 4);
            }
            var dataTable = await mySQLService.Select(query).ConfigureAwait(false);
            var missionNodes = new List<MissionNodeModel>();
            for (int i = 0; i < dataTable.Count; i++)
            {
                missionNodes.Add(new MissionNodeModel()
                {
                    Id = int.Parse(dataTable[i][0]),
                    CreateDate = DateTime.Parse(dataTable[i][1]),
                    UpdateDate = DateTime.Parse(dataTable[i][2]),
                    MissionId = int.Parse(dataTable[i][3]),
                    NodeId = int.Parse(dataTable[i][4]),
                    PatternId = int.Parse(dataTable[i][5]),
                    Path = dataTable[i][6],
                    Title = dataTable[i][7],
                    Type = (EnumMissionActionType)int.Parse(dataTable[i][8]),
                    Text = dataTable[i][9],
                    isRequired = dataTable[i][10] == "True" ? true : false
                });
            }
            return missionNodes;
        }
    }
}
