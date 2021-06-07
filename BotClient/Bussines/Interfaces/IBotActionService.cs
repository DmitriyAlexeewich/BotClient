using BotClient.Models.Bot;
using BotClient.Models.Bot.Enumerators;
using BotDataModels.Bot;
using BotDataModels.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces
{
    public interface IBotActionService
    {
        Task<List<ParsedGroupModel>> SearchGroups(Guid WebDriverId, string KeyWord = "", bool FilteredBySubscribersCount = true, EnumSearchGroupType SearchGroupType = EnumSearchGroupType.AllTypes, string Country = "", string City = "", bool isSaftySearch = true);
        Task<List<ParsedGroupModel>> GetClientGroups(Guid WebDriverId, string ClientVkId);
        Task<List<ParsedAudioModel>> GetAudiosByLink(Guid WebDriverId, string Link);
        Task<bool> AddAudioToSelfPage(Guid WebDriverId, List<ParsedAudioModel> Audios);
        Task<List<DocumentCreateModel>> GetDocsByLink(Guid WebDriverId, string Link);
        Task<List<ParsedVideoModel>> GetVideosByLink(Guid WebDriverId, string Link);
        Task<bool> AddVideoToSelfPage(Guid WebDriverId, List<ParsedVideoModel> Videos);
        Task<bool> CustomizeBot(Guid WebDriverId, BotModel Bot, List<BotCustomizeSettingsModel> BotCustomizeSettings, BotCustomizeModel BotCustomize);
    }
}
