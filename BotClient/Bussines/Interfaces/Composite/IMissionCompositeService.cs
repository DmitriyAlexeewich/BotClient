using BotClient.Models.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces.Composite
{
    public interface IMissionCompositeService
    {
        Task<List<RoleMissionConnectorModel>> GetRoleMissionConnections(int RoleId, bool? isActive);
        Task<MissionModel> GetMission(int MissionId);
        Task<List<MissionNodeModel>> GetNodes(int MissionId, int? PatternId, string? Path);
        Task<List<NodePatternModel>> GetNodePatterns(int MissionId, int PatternId);
    }
}
