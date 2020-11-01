using BotClient.Bussines.Interfaces;
using BotClient.Bussines.Interfaces.Composite;
using BotClient.Bussines.Interfaces.MySQL;
using BotClient.Models.Bot;
using BotClient.Models.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services.Composite
{
    public class BotCompositeService : IBotCompositeService
    {
        private readonly IBotService botService;
        private readonly IPatternService patternService;
        private readonly IBotCustomizeService botCustomizeService;

        public BotCompositeService(IBotService BotService,
                                   IBotCustomizeService BotCustomizeService,
                                   IPatternService PatternService)
        {
            botService = BotService;
            botCustomizeService = BotCustomizeService;
            patternService = PatternService;
        }

        public async Task<BotModel> GetBotById(int Id)
        {
            var bot = await botService.GetById(Id).ConfigureAwait(false);
            return bot;
        }

        public async Task<BotModel> GetBotByVkId(string VkId)
        {
            var bot = await botService.GetByVkId(VkId).ConfigureAwait(false);
            return bot;
        }

        public async Task<BotCustomizeModel> GetBotCustomize(int BotId)
        {
            var botCustomize = await botCustomizeService.GetByBotId(BotId).ConfigureAwait(false);
            return botCustomize;
        }

        public async Task<List<PatternModel>> GetPatterns(int RoleId)
        {
            var patterns = await patternService.GetByRoleId(RoleId).ConfigureAwait(false);
            return patterns;
        }

        public async Task<bool> UpdateBotData(BotModel BotData)
        {
            var result = await botService.Update(BotData).ConfigureAwait(false);
            return result;
        }

        public async Task<bool> SetIsDead(int Id, bool isDead)
        {
            var result = await botService.SetIsDead(Id, isDead).ConfigureAwait(false);
            return result;
        }

        public async Task<bool> SetIsPrintBlock(int Id, bool isPrintBlock)
        {
            var result = await botService.SetIsPrintBlock(Id, isPrintBlock).ConfigureAwait(false);
            return result;
        }

        public async Task<bool> SetIsLogin(int Id, bool isLogin)
        {
            var result = await botService.SetIsLogin(Id, isLogin).ConfigureAwait(false);
            return result;
        }

        public async Task<bool> SetIsUpdatedCusomizeInfo(int Id, bool isUpdatedCusomizeInfo)
        {
            var result = await botService.SetIsUpdatedCusomizeInfo(Id, isUpdatedCusomizeInfo).ConfigureAwait(false);
            return result;
        }

    }
}
