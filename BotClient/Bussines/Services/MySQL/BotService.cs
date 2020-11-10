using BotClient.Bussines.Interfaces;
using BotClient.Models;
using BotClient.Models.Bot;
using BotClient.Models.Bot.Enumerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services
{
    public class BotService : IBotService
    {
        private readonly IMySQLService mySQLService;

        public BotService(IMySQLService MySQLService)
        {
            mySQLService = MySQLService;
        }

        public async Task<BotModel> GetById(int Id)
        {
            var filters = new List<QueryFilter>();
            filters.Add(new QueryFilter()
            {
                Table = "Bot",
                Column = "Id",
                Filter = Id.ToString()
            });
            var botDataTable = await Select(filters).ConfigureAwait(false);
            if (botDataTable.Count > 0)
            {
                return botDataTable[0];
            }
            return null;
        }

        public async Task<BotModel> GetByVkId(string VkId)
        {
            var filters = new List<QueryFilter>();
            filters.Add(new QueryFilter()
            {
                Table = "Bot",
                Column = "VkId",
                Filter = VkId
            });
            var botDataTable = await Select(filters).ConfigureAwait(false);
            if (botDataTable.Count > 0)
            {
                return botDataTable[0];
            }
            return null;
        }

        public async Task<List<BotModel>> GetAll(int? Id, string? VkId)
        {
            var filters = new List<QueryFilter>();
            if (Id != null)
            {
                filters.Add(new QueryFilter()
                {
                    Table = "Bot",
                    Column = "Id",
                    Filter = Id.ToString()
                });
            }
            if (VkId != null)
            {
                filters.Add(new QueryFilter()
                {
                    Table = "Bot",
                    Column = "VkId",
                    Filter = VkId
                });
            }
            var botDataTable = await Select(filters.Count > 0 ? filters : null).ConfigureAwait(false);
            if (botDataTable.Count > 0)
            {
                return botDataTable;
            }
            return null;
        }

        public async Task<bool> Update(BotModel BotData)
        {
            var query = $"UPDATE `Bot` SET " +
                $"`Login`='{BotData.Login}'," +
                $"`Password`='{BotData.Password}'," +
                $"`IsMale`='{BotData.isMale}'," +
                $"`Gender`='{(int)BotData.Gender}'," +
                $"`Age`='{BotData.Age}'," +
                $"`VkId`='{BotData.VkId}'," +
                $"`FullName`='{BotData.FullName}'," +
                $"`OnlineDate`='{BotData.OnlineDate}'," +
                $"`IsDead`='{(BotData.isDead == true ? 1 : 0)}'," +
                $"`IsPrintBlock`='{(BotData.isPrintBlock == true ? 1 : 0)}'," +
                $"`IsLogin`='{(BotData.isLogin == true ? 1 : 0)}'," +
                $"`IsUpdatedCusomizeInfo`='{(BotData.isUpdatedCustomizeInfo == true ? 1 : 0)}'" +
                $"WHERE `Id`='{BotData.Id}'";
            return await mySQLService.ExecuteNonQuery(query).ConfigureAwait(false);
        }

        public async Task<bool> SetIsDead(int Id, bool isDead)
        {
            var query = $"UPDATE `Bot` SET " +
                        $"`IsDead`='{(isDead == true ? 1 : 0)}'" +
                        $"WHERE `Id`='{Id}'";
            return await mySQLService.ExecuteNonQuery(query).ConfigureAwait(false);
        }

        public async Task<bool> SetIsPrintBlock(int Id, bool isPrintBlock)
        {
            var query = $"UPDATE `Bot` SET " +
                        $"`IsPrintBlock`='{(isPrintBlock == true ? 1 : 0)}'" +
                        $"WHERE `Id`='{Id}'";
            return await mySQLService.ExecuteNonQuery(query).ConfigureAwait(false);
        }

        public async Task<bool> SetIsLogin(int Id, bool isLogin)
        {
            var query = $"UPDATE `Bot` SET " +
                        $"`IsLogin`='{(isLogin == true ? 1 : 0)}'" +
                        $"WHERE `Id`='{Id}'";
            return await mySQLService.ExecuteNonQuery(query).ConfigureAwait(false);
        }

        public async Task<bool> SetIsUpdatedCustomizeInfo(int Id, bool isUpdatedCustomizeInfo)
        {
            var query = $"UPDATE `Bot` SET " +
                        $"`IsUpdatedCusomizeInfo`='{(isUpdatedCustomizeInfo == true ? 1 : 0)}'" +
                        $"WHERE `Id`='{Id}'";
            return await mySQLService.ExecuteNonQuery(query).ConfigureAwait(false);
        }

        private async Task<List<BotModel>> Select(List<QueryFilter>? Filters)
        {
            var query = "SELECT * FROM `Bot` ";
            if (Filters != null)
            {
                query += "WHERE ";
                for (int i = 0; i < Filters.Count; i++)
                    query += $"`{Filters[i].Table}`.`{Filters[i].Column}`='{Filters[i].Filter}' AND";
                query = query.Remove(query.Length - 4);
            }
            var dataTable = await mySQLService.Select(query).ConfigureAwait(false);
            var bots = new List<BotModel>();
            for (int i = 0; i < dataTable.Count; i++)
            {
                bots.Add(new BotModel()
                {
                    Id = int.Parse(dataTable[i][0]),
                    CreateDate = DateTime.Parse(dataTable[i][1]),
                    UpdateDate = DateTime.Parse(dataTable[i][2]),
                    Login = dataTable[i][3],
                    Password = dataTable[i][4],
                    isMale = dataTable[i][5] == "True" ? true : false,
                    Gender = (EnumGender)int.Parse(dataTable[i][6]),
                    Age = int.Parse(dataTable[i][7]),
                    VkId = dataTable[i][8],
                    FullName = dataTable[i][9],
                    OnlineDate = DateTime.Parse(dataTable[i][10]),
                    RoleId = int.Parse(dataTable[i][11]),
                    isDead = dataTable[i][12] == "True" ? true : false,
                    isPrintBlock = dataTable[i][13] == "True" ? true : false,
                    isLogin = dataTable[i][14] == "True" ? true : false,
                    isUpdatedCustomizeInfo = dataTable[i][15] == "True" ? true : false,
                });
            }
            return bots;
        }
    }
}
