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
    public class NodePatternService : INodePatternService
    {
        private readonly IMySQLService mySQLService;

        public NodePatternService(IMySQLService MySQLService)
        {
            mySQLService = MySQLService;
        }

        public async Task<NodePatternModel> GetByPatternId(int PatternId)
        {
            var filters = new List<QueryFilter>();
            filters.Add(new QueryFilter()
            {
                Table = "NodePattern",
                Column = "PatternId",
                Filter = PatternId.ToString()
            });
            var nodesDataTable = await Select(filters).ConfigureAwait(false);
            if (nodesDataTable.Count > 0)
            {
                return nodesDataTable[0];
            }
            return null;
        }

        public async Task<List<NodePatternModel>> GetAll(int? Id, int? MissionId, int? PatternId, int? NodeId)
        {
            var filters = new List<QueryFilter>();
            if (Id != null)
            {
                filters.Add(new QueryFilter()
                {
                    Table = "NodePattern",
                    Column = "Id",
                    Filter = Id.ToString()
                });
            }
            if (MissionId != null)
            {
                filters.Add(new QueryFilter()
                {
                    Table = "NodePattern",
                    Column = "MissionId",
                    Filter = MissionId.ToString()
                });
            }
            if (PatternId != null)
            {
                filters.Add(new QueryFilter()
                {
                    Table = "NodePattern",
                    Column = "PatternId",
                    Filter = PatternId.ToString()
                });
            }
            if (NodeId != null)
            {
                filters.Add(new QueryFilter()
                {
                    Table = "NodePattern",
                    Column = "NodeId",
                    Filter = NodeId.ToString()
                });
            }
            var nodesDataTable = await Select(filters).ConfigureAwait(false);
            if (nodesDataTable.Count > 0)
            {
                return nodesDataTable;
            }
            return null;
        }

        private async Task<List<NodePatternModel>> Select(List<QueryFilter>? Filters)
        {
            var query = "SELECT * FROM `NodePattern` ";
            if (Filters != null)
            {
                query += "WHERE ";
                for (int i = 0; i < Filters.Count; i++)
                    query += $"`{Filters[i].Table}`.`{Filters[i].Column}`='{Filters[i].Filter}' AND";
                query = query.Remove(query.Length - 4);
            }
            var dataTable = await mySQLService.Select(query).ConfigureAwait(false);
            var nodePatterns = new List<NodePatternModel>();
            for (int i = 0; i < dataTable.Count; i++)
            {
                nodePatterns.Add(new NodePatternModel()
                {
                    Id = int.Parse(dataTable[i][0]),
                    CreateDate = DateTime.Parse(dataTable[i][1]),
                    UpdateDate = DateTime.Parse(dataTable[i][2]),
                    MissionId = int.Parse(dataTable[i][3]),
                    PatternId = int.Parse(dataTable[i][4]),
                    NodeId = int.Parse(dataTable[i][5]),
                    Title = dataTable[i][6],
                    PatternText = dataTable[i][7],
                    isInclude = dataTable[i][8] == "True" ? true : false,
                    isRegex = dataTable[i][9] == "True" ? true : false
                });
            }
            return nodePatterns;
        }
    }
}
