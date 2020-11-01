using BotClient.Models.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces.MySQL
{
    public interface IPatternService
    {
        Task<List<PatternModel>> GetByRoleId(int RoleId);
        Task<List<PatternModel>> GetAllNoneRole();
        Task<List<PatternModel>> GetAll(int? Id, int? RoleId, bool? isInclude, bool? isRegex, bool? isRolePattern);
    }
}
