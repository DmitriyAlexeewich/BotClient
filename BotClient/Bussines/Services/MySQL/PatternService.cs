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
    public class PatternService : IPatternService
    {
        private readonly IMySQLService mySQLService;

        public PatternService(IMySQLService MySQLService)
        {
            mySQLService = MySQLService;
        }

        public async Task<List<PatternModel>> GetByRoleId(int RoleId)
        {
            var filters = new List<QueryFilter>();
            filters.Add(new QueryFilter()
            {
                Table = "Pattern",
                Column = "RoleId",
                Filter = RoleId.ToString()
            });
            var patternsDataTable = await Select(filters).ConfigureAwait(false);
            if (patternsDataTable.Count > 0)
            {
                return patternsDataTable;
            }
            return null;
        }

        public async Task<List<PatternModel>> GetAllNoneRole()
        {
            var filters = new List<QueryFilter>();
            filters.Add(new QueryFilter()
            {
                Table = "Pattern",
                Column = "IsRolePattern",
                Filter = "0"
            });
            var patternsDataTable = await Select(filters).ConfigureAwait(false);
            if (patternsDataTable.Count > 0)
            {
                return patternsDataTable;
            }
            return null;
        }

        public async Task<List<PatternModel>> GetAll(int? Id, int? RoleId, bool? isInclude, bool? isRegex, bool? isRolePattern)
        {
            var filters = new List<QueryFilter>();
            if (Id != null)
            {
                filters.Add(new QueryFilter()
                {
                    Table = "Pattern",
                    Column = "Id",
                    Filter = Id.ToString()
                });
            }
            if (RoleId != null)
            {
                filters.Add(new QueryFilter()
                {
                    Table = "Pattern",
                    Column = "RoleId",
                    Filter = RoleId.ToString()
                });
            }
            if (isInclude != null)
            {
                filters.Add(new QueryFilter()
                {
                    Table = "Pattern",
                    Column = "isInclude",
                    Filter = isInclude == true ? "1" : "0"
                });
            }
            if (isRegex != null)
            {
                filters.Add(new QueryFilter()
                {
                    Table = "Pattern",
                    Column = "isRegex",
                    Filter = isRegex == true ? "1" : "0"
                });
            }
            if (isRolePattern != null)
            {
                filters.Add(new QueryFilter()
                {
                    Table = "Pattern",
                    Column = "isRegex",
                    Filter = isRolePattern == true ? "1" : "0"
                });
            }
            var patternsDataTable = await Select(filters.Count > 0 ? filters : null).ConfigureAwait(false);
            if (patternsDataTable.Count > 0)
            {
                return patternsDataTable;
            }
            return null;
        }

        private async Task<List<PatternModel>> Select(List<QueryFilter>? Filters)
        {
            var query = "SELECT * FROM `Pattern` ";
            if (Filters != null)
            {
                query += "WHERE ";
                for (int i = 0; i < Filters.Count; i++)
                    query += $"`{Filters[i].Table}`.`{Filters[i].Column}`='{Filters[i].Filter}' AND";
                query = query.Remove(query.Length - 4);
            }
            var dataTable = await mySQLService.Select(query).ConfigureAwait(false);
            var patterns = new List<PatternModel>();
            for (int i = 0; i < dataTable.Count; i++)
            {
                patterns.Add(new PatternModel()
                {
                    Id = int.Parse(dataTable[i][0]),
                    CreateDate = DateTime.Parse(dataTable[i][1]),
                    UpdateDate = DateTime.Parse(dataTable[i][2]),
                    RoleId = int.Parse(dataTable[i][3]),
                    Title = dataTable[i][4],
                    Text = dataTable[i][5],
                    AnswerText = dataTable[i][6],
                    isInclude = dataTable[i][6] == "True" ? true : false,
                    isRegex = dataTable[i][7] == "True" ? true : false,
                    isRolePattern = dataTable[i][9] == "True" ? true : false
                });
            }
            return patterns;
        }
    }
}
