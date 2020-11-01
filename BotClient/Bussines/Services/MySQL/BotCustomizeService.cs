using BotClient.Bussines.Interfaces;
using BotClient.Models;
using BotClient.Models.Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services
{
    public class BotCustomizeService : IBotCustomizeService
    {
        private readonly IMySQLService mySQLService;

        public BotCustomizeService(IMySQLService MySQLService)
        {
            mySQLService = MySQLService;
        }

        public async Task<BotCustomizeModel> GetById(int Id)
        {
            var filters = new List<QueryFilter>();
            filters.Add(new QueryFilter()
            {
                Table = "BotCustomize",
                Column = "Id",
                Filter = Id.ToString()
            });
            var botCustomizeDataTable = await Select(filters).ConfigureAwait(false);
            if (botCustomizeDataTable.Count > 0)
            {
                return botCustomizeDataTable[0];
            }
            return null;
        }

        public async Task<BotCustomizeModel> GetByBotId(int BotId)
        {
            var filters = new List<QueryFilter>();
            filters.Add(new QueryFilter()
            {
                Table = "BotCustomize",
                Column = "BotId",
                Filter = BotId.ToString()
            });
            var botCustomizeDataTable = await Select(filters).ConfigureAwait(false);
            if (botCustomizeDataTable.Count > 0)
            {
                return botCustomizeDataTable[0];
            }
            return null;
        }

        private async Task<List<BotCustomizeModel>> Select(List<QueryFilter>? Filters)
        {
            var query = "SELECT * FROM `BotCustomize` ";
            if (Filters != null)
            {
                query += "WHERE ";
                for (int i = 0; i < Filters.Count; i++)
                    query += $"`{Filters[i].Table}`.`{Filters[i].Column}`='{Filters[i].Filter}' AND";
                query = query.Remove(query.Length - 4);
            }
            var dataTable = await mySQLService.Select(query).ConfigureAwait(false);
            var botsCustomize = new List<BotCustomizeModel>();
            for (int i = 0; i < dataTable.Count; i++)
            {
                botsCustomize.Add(new BotCustomizeModel()
                {
                    Id = int.Parse(dataTable[0][0]),
                    CreateDate = DateTime.Parse(dataTable[0][1]),
                    UpdateDate = DateTime.Parse(dataTable[0][2]),
                    BotId = int.Parse(dataTable[0][3]),
                    City = dataTable[0][4],
                    MobilePhone = dataTable[0][5],
                    HomePhone = dataTable[0][6],
                    Skype = dataTable[0][7],
                    JobSite = dataTable[0][8],
                    Job = dataTable[0][9],
                    Interest = dataTable[0][10],
                    FavoriteMusic = dataTable[0][11],
                    FavoriteFilms = dataTable[0][12],
                    FavoriteTVShows = dataTable[0][13],
                    FavoriteBook = dataTable[0][14],
                    FavoriteGame = dataTable[0][15],
                    FavoriteQuote = dataTable[0][16],
                    PersonalInfo = dataTable[0][17],
                    Grands = dataTable[0][18],
                    Parents = dataTable[0][19],
                    SistersAndBrothers = dataTable[0][20],
                    Childs = dataTable[0][21],
                    GrandChilds = dataTable[0][22],
                });
            }
            return botsCustomize;
        }
    }
}
