using BotClient.Bussines.Interfaces;
using BotClient.Models.Bot;
using BotClient.Models.Bot.Enumerators;
using BotClient.Models.Bot.Work.Enumerators;
using BotDataModels.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services
{
    public class BotActionService
    {
        private readonly ISettingsService settingsService;
        private readonly IVkActionService vkActionService;

        public BotActionService(ISettingsService SettingsService,
                              IVkActionService VkActionService)
        {
            settingsService = SettingsService;
            vkActionService = VkActionService;
        }

        public async Task<List<ClientGroupCreateModel>> SearchGroups(Guid WebDriverId, string KeyWord = "", bool FilteredBySubscribersCount = true, EnumSearchGroupType SearchGroupType = EnumSearchGroupType.AllTypes, string Country = "", string City = "", bool isSaftySearch = true)
        {
            var result = new List<ClientGroupCreateModel>();
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

        public async Task<List<ClientGroupCreateModel>> GetClientGroups(Guid WebDriverId, string ClientVkId)
        {
            var result = new List<ClientGroupCreateModel>();
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
                    result = await vkActionService.AddAudioToSelfPage(Audios[i].AudioElement).ConfigureAwait(false);
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
    
        
    }
}
