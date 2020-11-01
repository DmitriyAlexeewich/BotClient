using BotClient.Models.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces.MySQL
{
    public interface IRoleService
    {
        Task<RoleModel> GetById(int Id);
    }
}
