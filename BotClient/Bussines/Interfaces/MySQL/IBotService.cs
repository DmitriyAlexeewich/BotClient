using BotClient.Models.Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces
{
    public interface IBotService
    {
        Task<BotModel> GetById(int Id);
        Task<BotModel> GetByVkId(string VkId);
        Task<List<BotModel>> GetAll(int? Id, string? VkId);
        Task<bool> Update(BotModel BotData);
        Task<bool> SetIsDead(int Id, bool isDead);
        Task<bool> SetIsPrintBlock(int Id, bool isPrintBlock);
        Task<bool> SetIsLogin(int Id, bool isLogin);
        Task<bool> SetIsUpdatedCustomizeInfo(int Id, bool isUpdatedCusomizeInfo);
    }
}
