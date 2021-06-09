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
