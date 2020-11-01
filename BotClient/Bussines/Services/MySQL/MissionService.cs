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
    public class MissionService : IMissionService
    {
        private readonly IMySQLService mySQLService;

        public MissionService(IMySQLService MySQLService)
        {
            mySQLService = MySQLService;
        }

        public async Task<MissionModel> GetById(int Id)
        {
            var filters = new List<QueryFilter>();
            filters.Add(new QueryFilter()
            {
                Table = "Mission",
                Column = "Id",
                Filter = Id.ToString()
            });
            var missionssDataTable = await Select(filters).ConfigureAwait(false);
            if (missionssDataTable.Count > 0)
            {
                return missionssDataTable[0];
            }
            return null;
        }

        private async Task<List<MissionModel>> Select(List<QueryFilter>? Filters)
        {
            var query = "SELECT * FROM `Mission` ";
            if (Filters != null)
            {
                query += "WHERE ";
                for (int i = 0; i < Filters.Count; i++)
                    query += $"`{Filters[i].Table}`.`{Filters[i].Column}`='{Filters[i].Filter}' AND";
                query = query.Remove(query.Length - 4);
            }
            var dataTable = await mySQLService.Select(query).ConfigureAwait(false);
            var missions = new List<MissionModel>();
            for (int i = 0; i < dataTable.Count; i++)
            {
                missions.Add(new MissionModel()
                {
                    Id = int.Parse(dataTable[i][0]),
                    CreateDate = DateTime.Parse(dataTable[i][1]),
                    UpdateDate = DateTime.Parse(dataTable[i][2]),
                    Title = dataTable[i][3]
                });
            }
            return missions;
        }
    }
}
