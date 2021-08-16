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
        int GetSearchVideoWordId(List<BotVideoModel> BotVideos, int MaxSearchVideoWordId);
        Task<BotVkVideo> StartVideo(Guid WebDriverId, List<VideoDictionaryModel> SearchWords);
        Task<bool> StopVideo(Guid WebDriverId, BotVkVideo BotVideo, int StartDialogCount);
        Task<BotMusicModel> StartMusic(Guid WebDriverId, List<BotMusicModel> BotMusic);
        Task<bool> StopMusic(Guid WebDriverId, int StartDialogCount);
        Task<bool> AddMusic(Guid WebDriverId);
        Task<BotVkNews> StartReadNews(Guid WebDriverId, List<BotNewsModel> BotNews);
        Task<BotVkNews> StopReadNews(Guid WebDriverId, BotVkNews BotNews, int StartDialogCount);
        Task<bool> hasNewMessagesByTime(Guid WebDriverId, int WaitingTime, int StartDialogCount);
        Task<string> GetBotFullName(Guid WebDriverId);
        Task<string> GenerateAndUpdatePassword(Guid WebDriverId, string OriginalPassword);
    }
}
