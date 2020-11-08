using BotClient.Bussines.Interfaces;
using BotClient.Models;
using BotClient.Models.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services
{
    public class ClientService : IClientService
    {
        private readonly IMySQLService mySQLService;

        public ClientService(IMySQLService MySQLService)
        {
            mySQLService = MySQLService;
        }

        public async Task<ClientModel> GetById(int Id)
        {
            var filters = new List<QueryFilter>();
            filters.Add(new QueryFilter()
            {
                Table = "Client",
                Column = "Id",
                Filter = Id.ToString()
            });
            var clientDataTable = await Select(filters).ConfigureAwait(false);
            if (clientDataTable.Count > 0)
            {
                return clientDataTable[0];
            }
            return null;
        }

        public async Task<ClientModel> GetByVkId(string VkId)
        {
            var filters = new List<QueryFilter>();
            filters.Add(new QueryFilter()
            {
                Table = "Client",
                Column = "VkId",
                Filter = VkId.ToString()
            });
            var clientDataTable = await Select(filters).ConfigureAwait(false);
            if (clientDataTable.Count > 0)
            {
                return clientDataTable[0];
            }
            return null;
        }

        public async Task<bool> Update(ClientModel ClientData)
        {
            var query = "UPDATE `Client` SET " +
                        $"`VkId`='{ClientData.VkId}', " +
                        $"`FullName`='{ClientData.FullName}' " +
                        $"WHERE `Id`='{ClientData.Id}'";
            return await mySQLService.ExecuteNonQuery(query).ConfigureAwait(false);
        }

        private async Task<List<ClientModel>> Select(List<QueryFilter>? Filters)
        {
            var query = "SELECT * FROM `Client` ";
            if (Filters != null)
            {
                query += "WHERE ";
                for (int i = 0; i < Filters.Count; i++)
                    query += $"`{Filters[i].Table}`.`{Filters[i].Column}`='{Filters[i].Filter}' AND";
                query = query.Remove(query.Length - 4);
            }
            var dataTable = await mySQLService.Select(query).ConfigureAwait(false);
            var clients = new List<ClientModel>();
            for (int i = 0; i < dataTable.Count; i++)
            {
                clients.Add(new ClientModel()
                {
                    Id = int.Parse(dataTable[i][0]),
                    CreateDate = DateTime.Parse(dataTable[i][1]),
                    UpdateDate = DateTime.Parse(dataTable[i][2]),
                    VkId = dataTable[i][3],
                    FullName = dataTable[i][4]
                });
            }
            return clients;
        }
    }
}
