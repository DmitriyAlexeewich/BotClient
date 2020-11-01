using BotClient.Models.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces.MySQL
{
    public interface IRoleMissionConnectorService
    {
        Task<List<RoleMissionConnectorModel>> GetByRoleId(int RoleId);
        Task<List<RoleMissionConnectorModel>> GetByMissionId(int MissionId);
        Task<List<RoleMissionConnectorModel>> GetAll(int? RoleId, int? MissionId, bool? isActive);
    }
}
