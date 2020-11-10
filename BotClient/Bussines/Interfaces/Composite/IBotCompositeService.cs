using BotClient.Models.Bot;
using BotClient.Models.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces.Composite
{
    public interface IBotCompositeService
    {
        Task<BotModel> GetBotById(int Id);
        Task<BotModel> GetBotByVkId(string VkId);
        Task<BotCustomizeModel> GetBotCustomize(int BotId);
        Task<List<PatternModel>> GetPatterns(int RoleId);
        Task<bool> UpdateBotData(BotModel BotData);
        Task<bool> SetIsDead(int Id, bool isDead);
        Task<bool> SetIsPrintBlock(int Id, bool isPrintBlock);
        Task<bool> SetIsLogin(int Id, bool isLogin);
        Task<bool> SetIsUpdatedCustomizeInfo(int Id, bool isUpdatedCusomizeInfo);
    }
}
