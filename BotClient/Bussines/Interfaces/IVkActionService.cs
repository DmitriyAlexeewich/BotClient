using BotClient.Models.Bot;
using BotClient.Models.Bot.Work;
using BotClient.Models.Bot.Work.Enumerators;
using BotClient.Models.Client;
using BotClient.Models.HTMLElements;
using BotDataModels.Bot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces
{
    public interface IVkActionService
    {
        Task<AlgoritmResult> Login(Guid WebDriverId, string Username, string Password);
        Task<bool> isLoginSuccess(Guid WebDriverId);
        Task<AlgoritmResult> Customize(Guid WebDriverId, BotCustomizeModel CustomizeData);
        Task<AlgoritmResult> GoToMusicPage(Guid WebDriverId);
        Task<BotMusicModel> GetFirstMusic(Guid WebDriverId);
        Task<BotMusicModel> GetNextMusic(Guid WebDriverId);
        Task<AlgoritmResult> StopMusic(Guid WebDriverId);
        Task<AlgoritmResult> AddMusic(Guid WebDriverId);
        Task PlayAddedMusic(Guid WebDriverId);
        Task<AlgoritmResult> WatchVideo(Guid WebDriverId);
        Task<bool> GoToProfile(Guid WebDriverId, string Link);
        Task<AlgoritmResult> GoToVideoCatalog(Guid WebDriverId);
        Task<AlgoritmResult> FindVideo(Guid WebDriverId, string SearchWord);
        Task<List<BotVkVideo>> GetVideos(Guid WebDriverId);
        Task<AlgoritmResult> ClickVideo(Guid WebDriverId, WebHTMLElement Element);
        Task<AlgoritmResult> CloseVideo(Guid WebDriverId);
        Task<AlgoritmResult> News(Guid WebDriverId);
        Task<AlgoritmResult> AvatarLike(Guid WebDriverId);
        Task<AlgoritmResult> NewsLike(Guid WebDriverId, EnumNewsLikeType NewsLikeType);
        Task<AlgoritmResult> Subscribe(Guid WebDriverId);
        Task<AlgoritmResult> SubscribeToGroup(Guid WebDriverId);
        Task<AlgoritmResult> Repost(Guid WebDriverId, EnumRepostType RepostType);
        Task<AlgoritmResult> SendFirstMessage(Guid WebDriverId, string MessageText, bool? isSecond = false);
        Task<AlgoritmResult> GoToDialog(Guid WebDriverId, string ClientVkId);
        Task<DialogWithNewMessagesModel> GetDialogWithNewMessages(Guid WebDriverId);
        Task<List<NewMessageModel>> GetNewMessagesInDialog(Guid WebDriverId, string ClientVkId);
        Task<AlgoritmResult> SendAnswerMessage(Guid WebDriverId, string MessageText, string ClientVkId, int BotClientRoleConnectorId);
        Task<bool> Logout(Guid WebDriverId);
        Task<string> GetClientName(Guid WebDriverId);
    }
}
