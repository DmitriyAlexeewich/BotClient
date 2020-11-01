using BotClient.Bussines.Interfaces;
using BotClient.Bussines.Interfaces.MySQL;
using BotClient.Models;
using BotClient.Models.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services.MySQL
{
    public class MessagesService : IMessagesService
    {
        private readonly IMySQLService mySQLService;

        public MessagesService(IMySQLService MySQLService)
        {
            mySQLService = MySQLService;
        }

        public async Task<List<MessageModel>> GetAll(int BotClientConnectionId, int? Id)
        {
            var filters = new List<QueryFilter>();
            filters.Add(new QueryFilter()
            {
                Table = "Messages",
                Column = "BotClientConnectionId ",
                Filter = BotClientConnectionId.ToString()
            });
            if (Id != null)
            {
                filters.Add(new QueryFilter()
                {
                    Table = "Messages",
                    Column = "Id ",
                    Filter = Id.ToString()
                });
            }
            var messagesDataTable = await Select(filters).ConfigureAwait(false);
            if (messagesDataTable.Count > 0)
            {
                return messagesDataTable;
            }
            return null;
        }

        public async Task<MessageModel> GetById(int Id)
        {
            var filters = new List<QueryFilter>();
            filters.Add(new QueryFilter()
            {
                Table = "Messages",
                Column = "Id",
                Filter = Id.ToString()
            });
            var messagesDataTable = await Select(filters).ConfigureAwait(false);
            if (messagesDataTable.Count > 0)
            {
                return messagesDataTable[0];
            }
            return null;
        }
        
        public async Task<bool> CreateMessage(int BotClientConnectionId, string Text, bool isBotMessage)
        {
            var query = $"INSERT INTO `Messages`(`BotClientConnectionId`, `Text`, `IsBotMessage`) VALUES ('{BotClientConnectionId}','{Text}', " +
                        $"'{(isBotMessage == true ? 1 : 0)}')";
            return await mySQLService.ExecuteNonQuery(query).ConfigureAwait(false);
        }

        private async Task<List<MessageModel>> Select(List<QueryFilter>? Filters)
        {
            var query = "SELECT * FROM `Messages` ";
            if (Filters != null)
            {
                query += "WHERE ";
                for (int i = 0; i < Filters.Count; i++)
                    query += $"`{Filters[i].Table}`.`{Filters[i].Column}`='{Filters[i].Filter}' AND";
                query = query.Remove(query.Length - 4);
            }
            var dataTable = await mySQLService.Select(query).ConfigureAwait(false);
            var messages = new List<MessageModel>();
            for (int i = 0; i < dataTable.Count; i++)
            {
                messages.Add(new MessageModel()
                {
                    Id = int.Parse(dataTable[i][0]),
                    CreateDate = DateTime.Parse(dataTable[i][1]),
                    UpdateDate = DateTime.Parse(dataTable[i][2]),
                    BotClientConnectionId = int.Parse(dataTable[i][3]),
                    Text = dataTable[i][4],
                    isBotMessage = dataTable[i][5] == "True" ? true : false
                });
            }
            return messages;
        }
    }
}
