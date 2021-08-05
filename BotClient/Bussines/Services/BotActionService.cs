using BotClient.Bussines.Interfaces;
using BotClient.Models.Bot;
using BotClient.Models.Bot.Enumerators;
using BotClient.Models.Bot.Work.Enumerators;
using BotDataModels.Bot;
using BotDataModels.Bot.Enumerators;
using BotDataModels.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services
{
    public class BotActionService : IBotActionService
    {
        private readonly ISettingsService settingsService;
        private readonly IVkActionService vkActionService;

        private Random random;

        public BotActionService(ISettingsService SettingsService,
                              IVkActionService VkActionService)
        {
            settingsService = SettingsService;
            vkActionService = VkActionService;
            random = new Random();
        }

        public async Task<List<ParsedGroupModel>> SearchGroups(Guid WebDriverId, string KeyWord = "", bool FilteredBySubscribersCount = true, EnumSearchGroupType SearchGroupType = EnumSearchGroupType.AllTypes, string Country = "", string City = "", bool isSaftySearch = true)
        {
            var result = new List<ParsedGroupModel>();
            try
            {
                var goToGroupSection = await vkActionService.GoToGroupsSection(WebDriverId).ConfigureAwait(false);
                if ((!goToGroupSection.hasError) && (goToGroupSection.ActionResultMessage == EnumActionResult.Success))
                {
                    var searchResult = await vkActionService.SearchGroups(WebDriverId, KeyWord, FilteredBySubscribersCount, SearchGroupType, Country, City, isSaftySearch).ConfigureAwait(false);
                    if ((!searchResult.hasError) && (searchResult.ActionResultMessage == EnumActionResult.Success))
                        result = await vkActionService.GetGroups(WebDriverId).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotActionService", ex).ConfigureAwait(false);
            }
            return result;
        }

        public async Task<List<ParsedGroupModel>> GetClientGroups(Guid WebDriverId, string ClientVkId)
        {
            var result = new List<ParsedGroupModel>();
            try
            {
                var goToClientPage = await vkActionService.GoToProfile(WebDriverId, "id" + ClientVkId).ConfigureAwait(false);
                if (goToClientPage)
                {
                    var goToClientGroupPage = await vkActionService.GoToClientGroups(WebDriverId, ClientVkId).ConfigureAwait(false);
                    if (goToClientGroupPage)
                        result = await vkActionService.GetClientGroups(WebDriverId, ClientVkId).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotActionService", ex).ConfigureAwait(false);
            }
            return result;
        }

        public async Task SubscribeToGroupsByVkId(Guid WebDriverId, List<ParsedGroupModel> ParsedGroups)
        {
            try
            {
                for (int i = 0; i < ParsedGroups.Count; i++)
                {
                    if (await vkActionService.GoToGroupByVkId(WebDriverId, ParsedGroups[i].GroupVkId).ConfigureAwait(false))
                        await vkActionService.SubscribeToGroup(WebDriverId).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotActionService", ex).ConfigureAwait(false);
            }
        }

        public async Task SubscribeToGroupByElement(Guid WebDriverId, List<ParsedGroupModel> ParsedGroups)
        {
            try
            {
                for (int i = 0; i < ParsedGroups.Count; i++)
                    await vkActionService.SubscribeToGroup(WebDriverId, ParsedGroups[i].GroupElement).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotActionService", ex).ConfigureAwait(false);
            }
        }

        public async Task<List<ParsedAudioModel>> GetAudiosByLink(Guid WebDriverId, string Link)
        {
            var result = new List<ParsedAudioModel>();
            try
            {
                var goToAudioPageByLink = await vkActionService.GoToAudioPageByLink(WebDriverId, Link).ConfigureAwait(false);
                if (goToAudioPageByLink)
                    result = await vkActionService.ParseAudio(WebDriverId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotActionService", ex).ConfigureAwait(false);
            }
            return result;
        }

        public async Task<bool> AddAudioToSelfPage(Guid WebDriverId, List<ParsedAudioModel> Audios)
        {
            var result = false;
            try
            {
                for (int i = 0; i < Audios.Count; i++)
                {
                    result = await vkActionService.AddAudioToSelfPage(WebDriverId, Audios[i].AudioElement).ConfigureAwait(false);
                    if (!result)
                        break;
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotActionService", ex).ConfigureAwait(false);
            }
            return result;
        }

        public async Task<List<DocumentCreateModel>> GetDocsByLink(Guid WebDriverId, string Link)
        {
            var result = new List<DocumentCreateModel>();
            try
            {
                var goToDocsPageByLink = await vkActionService.GoToDocsPageByLink(WebDriverId, Link).ConfigureAwait(false);
                if (goToDocsPageByLink)
                    result = await vkActionService.ParseDocs(WebDriverId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotActionService", ex).ConfigureAwait(false);
            }
            return result;
        }

        public async Task<List<ParsedVideoModel>> GetVideosByLink(Guid WebDriverId, string Link)
        {
            var result = new List<ParsedVideoModel>();
            try
            {
                var goToVideoPageByLink = await vkActionService.GoToVideoPageByLink(WebDriverId, Link).ConfigureAwait(false);
                if (goToVideoPageByLink)
                    result = await vkActionService.ParseVideos(WebDriverId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotActionService", ex).ConfigureAwait(false);
            }
            return result;
        }

        public async Task<bool> AddVideoToSelfPage(Guid WebDriverId, List<ParsedVideoModel> Videos)
        {
            var result = false;
            try
            {
                for (int i = 0; i < Videos.Count; i++)
                {
                    result = await vkActionService.AddVideoToSelfPage(Videos[i].VideoElement).ConfigureAwait(false);
                    if (!result)
                        break;
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotActionService", ex).ConfigureAwait(false);
            }
            return result;
        }

        public async Task<bool> CustomizeBot(Guid WebDriverId, BotModel Bot, List<BotCustomizeSettingsModel> BotCustomizeSettings, BotCustomizeModel BotCustomize)
        {
            var result = false;
            try
            {
                if (Bot.isUpdatedCustomizeInfo)
                {
                    for (int i = 0; i < BotCustomizeSettings.Count; i++)
                    {
                        switch (BotCustomizeSettings[i].CustomizeType)
                        {
                            case EnumCustomizeType.ParseAudioByLink:
                                var audios = await GetAudiosByLink(WebDriverId, BotCustomizeSettings[i].Link).ConfigureAwait(false);
                                audios = RemoveRandom(audios, BotCustomizeSettings[i].AddPercent, BotCustomizeSettings[i].MinAdd, BotCustomizeSettings[i].MaxAdd);
                                if (audios.Count > 0)
                                    await AddAudioToSelfPage(WebDriverId, audios).ConfigureAwait(false);
                                break;
                            case EnumCustomizeType.ParseBooksByLink:
                                var books = await GetDocsByLink(WebDriverId, BotCustomizeSettings[i].Link).ConfigureAwait(false);
                                books = RemoveRandom(books, BotCustomizeSettings[i].AddPercent, BotCustomizeSettings[i].MinAdd, BotCustomizeSettings[i].MaxAdd);
                                for (int j = 0; j < books.Count; j++)
                                    BotCustomize.FavoriteBook += "\n" + books[j].Name;
                                break;
                            case EnumCustomizeType.SubscribeGroupsByCity:
                                var searchedGroups = await SearchGroups(WebDriverId, "", true, EnumSearchGroupType.AllTypes, BotCustomize.Coutry, BotCustomize.City).ConfigureAwait(false);
                                searchedGroups = RemoveRandom(searchedGroups, BotCustomizeSettings[i].AddPercent, BotCustomizeSettings[i].MinAdd, BotCustomizeSettings[i].MaxAdd);
                                await SubscribeToGroupByElement(WebDriverId, searchedGroups).ConfigureAwait(false);
                                break;
                            case EnumCustomizeType.SubscribeGroupsByClient:
                                var clientGroups = await GetClientGroups(WebDriverId, BotCustomizeSettings[i].Link).ConfigureAwait(false);
                                clientGroups = RemoveRandom(clientGroups, BotCustomizeSettings[i].AddPercent, BotCustomizeSettings[i].MinAdd, BotCustomizeSettings[i].MaxAdd);
                                await SubscribeToGroupsByVkId(WebDriverId, clientGroups).ConfigureAwait(false);
                                break;
                            case EnumCustomizeType.AddVideoByLink:
                                var videos = await GetVideosByLink(WebDriverId, BotCustomizeSettings[i].Link).ConfigureAwait(false);
                                videos = RemoveRandom(videos, BotCustomizeSettings[i].AddPercent, BotCustomizeSettings[i].MinAdd, BotCustomizeSettings[i].MaxAdd);
                                await AddVideoToSelfPage(WebDriverId, videos).ConfigureAwait(false);
                                break;
                            case EnumCustomizeType.SubscribeGroupByLink:
                                var groups = new List<ParsedGroupModel>();
                                groups.Add(new ParsedGroupModel()
                                {
                                    GroupVkId = BotCustomizeSettings[i].Link
                                });
                                await SubscribeToGroupsByVkId(WebDriverId, groups).ConfigureAwait(false);
                                break;
                            default:
                                break;
                        }
                    }
                    if(!BotCustomize.isComplete)
                        await vkActionService.Customize(WebDriverId, BotCustomize).ConfigureAwait(false);
                    result = true;
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotActionService", ex);
            }
            return result;
        }

        public int GetSearchVideoWordId(List<BotVideoModel> BotVideos, int MaxSearchVideoWordId)
        {
            var result = -1;
            var botWords = BotVideos;
            var maxWordId = MaxSearchVideoWordId;
            for (int i = 0; i < 100; i++)
            {
                result = random.Next(0, maxWordId);
                if (botWords.FirstOrDefault(item => item.WordId == result) == null)
                    break;
            }
            return result;
        }

        public async Task<BotVkVideo> StartVideo(Guid WebDriverId, List<VideoDictionaryModel> SearchWords)
        {
            BotVkVideo result = null;
            try
            {
                if (SearchWords.Count > 0)
                {
                    var goToCatalogResult = await vkActionService.GoToVideoCatalog(WebDriverId).ConfigureAwait(false);
                    if ((!goToCatalogResult.hasError) && (goToCatalogResult.ActionResultMessage == EnumActionResult.Success))
                    {
                        var findVideoResult = await vkActionService.FindVideo(WebDriverId, SearchWords[0].Word).ConfigureAwait(false);
                        if ((!findVideoResult.hasError) && (findVideoResult.ActionResultMessage == EnumActionResult.Success))
                        {
                            var videos = await vkActionService.GetVideos(WebDriverId).ConfigureAwait(false);
                            if (videos.Count > 0)
                            {
                                result = videos[random.Next(0, videos.Count)];
                                var clickVideoResult = await vkActionService.ClickVideo(WebDriverId, result.HTMLElement).ConfigureAwait(false);
                                if (!((!clickVideoResult.hasError) && (clickVideoResult.ActionResultMessage == EnumActionResult.Success)))
                                    result = null;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotActionService", ex).ConfigureAwait(false);
            }
            return result;
        }
        
        public async Task<bool> StopVideo(Guid WebDriverId, int StartDialogCount)
        {
            var result = false;
            try
            {
                var settings = settingsService.GetServerSettings();
                var waitingTime = settings.VideoWaitingTime + random.Next(-settings.VideoWaitingDeltaTime, settings.VideoWaitingDeltaTime);
                result = await hasNewMessagesByTime(WebDriverId, waitingTime, StartDialogCount).ConfigureAwait(false);
                await vkActionService.CloseVideo(WebDriverId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotActionService", ex).ConfigureAwait(false);
            }
            return result;
        }

        public async Task<BotMusicModel> StartMusic(Guid WebDriverId, List<BotMusicModel> BotMusic)
        {
            BotMusicModel result = null;
            try
            {
                var goToPageResult = await vkActionService.GoToMusicPage(WebDriverId).ConfigureAwait(false);
                if ((!goToPageResult.hasError) && (goToPageResult.ActionResultMessage == EnumActionResult.Success))
                {
                    if (random.Next(0, 2) != 1)
                    {
                        result = await vkActionService.GetFirstMusic(WebDriverId).ConfigureAwait(false);
                        var randListenAttept = random.Next(10, 20);
                        for (int i = 0; i < randListenAttept; i++)
                        {
                            if (BotMusic.FirstOrDefault(item => (item.Artist.IndexOf(result.Artist) != -1) && (item.SongName.IndexOf(result.SongName) != -1)) != null)
                                result = await vkActionService.GetNextMusic(WebDriverId).ConfigureAwait(false);
                            else
                                break;
                        }

                    }
                    else
                        await vkActionService.PlayAddedMusic(WebDriverId).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotActionService", ex).ConfigureAwait(false);
            }
            return result;
        }

        public async Task<bool> StopMusic(Guid WebDriverId, int StartDialogCount)
        {
            var result = false;
            try
            {
                var settings = settingsService.GetServerSettings();
                var waitingTime = settings.MusicWaitingTime + random.Next(-settings.MusicWaitingDeltaTime, settings.MusicWaitingDeltaTime);
                result = await hasNewMessagesByTime(WebDriverId, waitingTime, StartDialogCount).ConfigureAwait(false);
                await vkActionService.StopMusic(WebDriverId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotActionService", ex).ConfigureAwait(false);
            }
            return result;
        }

        public async Task<BotVkNews> StartReadNews(Guid WebDriverId, List<BotVkNews> BotNews)
        {
            BotVkNews result = null;
            try
            {
                var goToPageResult = await vkActionService.GoToNewsPage(WebDriverId).ConfigureAwait(false);
                if ((!goToPageResult.hasError) && (goToPageResult.ActionResultMessage == EnumActionResult.Success))
                {
                    var botNews = await vkActionService.GetNews(WebDriverId).ConfigureAwait(false);
                    for (int i = 0; i < BotNews.Count; i++)
                        botNews.RemoveAll(item => item.BotNews.NewsId == BotNews[i].BotNews.NewsId);
                    if (botNews.Count > 0)
                        result = botNews[random.Next(0, botNews.Count)];
                }

            }
            catch(Exception ex)
            {
                await settingsService.AddLog("BotActionService", ex).ConfigureAwait(false);
            }
            return result;
        }

        public async Task<bool> StopReadNews(Guid WebDriverId, BotVkNews BotNews, int StartDialogCount)
        {
            var result = false;
            try
            {
                var settings = settingsService.GetServerSettings();
                BotNews.NewsElement.ScrollTo();
                if (BotNews.BotNews.isLiked)
                    await vkActionService.LikePost(WebDriverId, BotNews.NewsElement).ConfigureAwait(false);
                if (BotNews.BotNews.isReposted)
                    await vkActionService.RepostPostToSelfPage(WebDriverId, BotNews.NewsElement).ConfigureAwait(false);
                var waitingTime = settings.NewsWaitingTime + random.Next(-settings.NewsWaitingDeltaTime, settings.NewsWaitingDeltaTime);
                result = await hasNewMessagesByTime(WebDriverId, waitingTime, StartDialogCount).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotActionService", ex).ConfigureAwait(false);
            }
            return result;
        }

        public async Task<bool> hasNewMessagesByTime(Guid WebDriverId, int WaitingTime, int StartDialogCount)
        {
            var result = false;
            try
            {
                var endTime = DateTime.Now.AddMilliseconds(WaitingTime);
                while (DateTime.Now < endTime)
                {
                    settingsService.WaitTime(10000);
                    var currentDialogCount = await vkActionService.GetNewDialogsCount(WebDriverId).ConfigureAwait(false);
                    if (StartDialogCount < currentDialogCount)
                    {
                        result = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotActionService", ex).ConfigureAwait(false);
            }
            return result;
        }

        public async Task<BotVkNewsPostModel> GetVkNewPost(Guid WebDriverId, string VkLink)
        {
            BotVkNewsPostModel result = null;
            try
            {
                var vkNewsPostVkId = await vkActionService.GoToNewsByLink(WebDriverId, VkLink).ConfigureAwait(false);
                if (vkNewsPostVkId.Length > 0)
                {
                    result = new BotVkNewsPostModel();
                    result.NewsVkId = vkNewsPostVkId;
                    result.CommentInput = await vkActionService.GetNewsPostInput(WebDriverId, vkNewsPostVkId).ConfigureAwait(false);
                    result.SendBtn = await vkActionService.GetNewsPostSendButton(WebDriverId, vkNewsPostVkId).ConfigureAwait(false);
                    result.Comments = await vkActionService.GetNewsPostComments(WebDriverId, vkNewsPostVkId).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                result = null;
                await settingsService.AddLog("BotActionService", ex).ConfigureAwait(false);
            }
            return result;
        }

        private List<T> RemoveRandom<T>(List<T> List, int SavePercent, int MinSave, int MaxSave)
        {
            if ((MinSave > 0) && (MaxSave > 0))
                List = RemoveRandomElementsByMinMax(List, MinSave, MaxSave);
            else
                List = RemoveRandomElementsByPercent(List, SavePercent);
            return List;
        }

        private List<T> RemoveRandomElementsByPercent<T>(List<T> List, int SavePercent)
        {
            if (SavePercent < 100)
            {
                int removeCount = (int)((List.Count / 100f) * (100f - SavePercent));
                if (removeCount >= List.Count)
                    removeCount = 0;
                for (int j = 0; j < removeCount && List.Count > 0; j++)
                    List.RemoveAt(random.Next(0, List.Count));
            }
            return List;
        }

        private List<T> RemoveRandomElementsByMinMax<T>(List<T> List, int MinSave, int MaxSave)
        {
            if ((MinSave > 0) && (MaxSave > 0))
            {
                int removeCount = List.Count - random.Next(MinSave, MaxSave);
                if (removeCount >= List.Count)
                    removeCount = 0;
                for (int i=0; i<removeCount; i++)
                    List.RemoveAt(random.Next(0, List.Count));
            }
            return List;
        }
    }
}
