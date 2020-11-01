using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces
{
    public interface IMySQLService
    {
        Task<List<List<string>>> Select(string Query);

        Task<bool> ExecuteNonQuery(string Query);
    }
}
