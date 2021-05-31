using BotClient.Models.Bot;
using BotClient.Models.Bot.Work;
using BotClient.Models.Bot.Work.Enumerators;
using BotClient.Models.Client;
using BotClient.Models.HTMLElements;
using BotDataModels.Bot;
using BotDataModels.Client;
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
        Task<AlgoritmResult> SubscribeToGroup(Guid WebDriverId, string GroupURL, string GroupName);
        Task<AlgoritmResult> GoToGroup(Guid WebDriverId, string GroupURL);
        Task<List<PlatformPostModel>> GetPosts(Guid WebDriverId);
        Task<AlgoritmResult> WatchPost(Guid WebDriverId, PlatformPostModel PlatformPost, bool isRepost);
        Task<AlgoritmResult> Repost(Guid WebDriverId, EnumRepostType RepostType);
        Task<AlgoritmResult> RepostPostToSelfPage(Guid WebDriverId, WebHTMLElement Post);
        Task<AlgoritmResult> SendFirstMessage(Guid WebDriverId, string MessageText, bool? isSecond = false);
        Task<AlgoritmResult> GoToDialog(Guid WebDriverId, string ClientVkId);
        Task<List<DialogWithNewMessagesModel>> GetDialogsWithNewMessages(Guid WebDriverId);
        Task CloseDialog(Guid WebDriverId);
        Task<List<NewMessageModel>> GetNewMessagesInDialog(Guid WebDriverId, string ClientVkId);
        Task<bool> isBotDialogBlocked(Guid WebDriver);
        Task<AlgoritmResult> SendAnswerMessage(Guid WebDriverId, string MessageText, string ClientVkId, int BotClientRoleConnectorId);
        Task<bool> Logout(Guid WebDriverId);
        Task<string> GetClientName(Guid WebDriverId);
        Task<bool> hasCaptcha(Guid WebDriverId);
        Task<string> GetVkId(Guid WebDriverId);
        Task<AlgoritmResult> GoToSelfPage(Guid WebDriverId);
        Task<AlgoritmResult> GoToSettings(Guid WebDriverId);
        Task<AlgoritmResult> ChangePassword(Guid WebDriverId, string OldPassword, string NewPassword);
        Task<string> GetPageName(Guid WebDriverId);
        Task<bool> GetCanRecievedMessage(Guid WebDriverId);
        Task<List<ParsedClientCreateModel>> GetContacts(Guid WebDriverId);
        Task<int> GetNewDialogsCount(Guid WebDriverId);
        Task<bool> hasChatBlock(Guid WebDriverId);
        Task<List<ClientGroupCreateModel>> GetClientGroups(Guid WebDriverId, string ClientVkId);
    }
}
