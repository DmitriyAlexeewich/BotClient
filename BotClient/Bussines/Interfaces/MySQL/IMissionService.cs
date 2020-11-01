using BotClient.Models.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces.MySQL
{
    public interface IMissionService
    {
        Task<MissionModel> GetById(int Id);
    }
}
