using BotClient.Models.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces
{
    public interface IMissionNodeService
    {
        Task<List<MissionNodeModel>> GetByMissionId(int MissionId);
        Task<List<MissionNodeModel>> GetAll(int? MissionId, int? NodeId, int? PatternId, string? Path);
    }
}
