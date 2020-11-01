using BotClient.Bussines.Interfaces;
using BotClient.Bussines.Interfaces.Composite;
using BotClient.Bussines.Interfaces.MySQL;
using BotClient.Models.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services.Composite
{
    public class MissionCompositeService : IMissionCompositeService
    {
        private readonly IRoleMissionConnectorService roleMissionConnectorService;
        private readonly IMissionService missionService;
        private readonly IMissionNodeService missionNodeService;
        private readonly INodePatternService nodePatternService;

        public MissionCompositeService(IRoleMissionConnectorService RoleMissionConnectorService,
                                      IMissionService MissionService,
                                      IMissionNodeService MissionNodeService,
                                      INodePatternService NodePatternService)
        {
            roleMissionConnectorService = RoleMissionConnectorService;
            missionService = MissionService;
            missionNodeService = MissionNodeService;
            nodePatternService = NodePatternService;
        }

        public async Task<List<RoleMissionConnectorModel>> GetRoleMissionConnections(int RoleId, bool? isActive)
        {
            var missions = await roleMissionConnectorService.GetByRoleId(RoleId).ConfigureAwait(false);
            if (isActive != null)
                missions = missions.Where(item => item.isActive == isActive).ToList();
            return missions;
        }

        public async Task<MissionModel> GetMission(int MissionId)
        {
            var mission = await missionService.GetById(MissionId).ConfigureAwait(false);
            return mission;
        }

        public async Task<List<MissionNodeModel>> GetNodes(int MissionId, int? PatternId, string? Path)
        {
            var nodes = await missionNodeService.GetAll(MissionId, null, PatternId, Path).ConfigureAwait(false);
            return nodes;
        }

        public async Task<List<NodePatternModel>> GetNodePatterns(int MissionId, int PatternId)
        {
            var patterns = await nodePatternService.GetAll(null, MissionId, PatternId, null).ConfigureAwait(false);
            return patterns;
        }
    }
}
