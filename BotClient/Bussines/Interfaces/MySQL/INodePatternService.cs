using BotClient.Models.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces.MySQL
{
    public interface INodePatternService
    {
        Task<NodePatternModel> GetByPatternId(int PatternId);
        Task<List<NodePatternModel>> GetAll(int? Id, int? MissionId, int? PatternId, int? NodeId);
    }
}
