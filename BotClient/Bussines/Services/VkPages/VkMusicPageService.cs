using BotClient.Bussines.Interfaces;
using BotClient.Models.HTMLElements.Enumerators;
using BotDataModels.Bot;
using BotDataModels.Settings;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services.VkPages
{
    public class VkMusicPageService
    {

        private readonly IWebDriverService webDriverService;
        private readonly IWebElementService webElementService;
        private readonly ISettingsService settingsService;

        public VkMusicPageService(IWebDriverService WebDriverService,
                              IWebElementService WebElementService,
                              ISettingsService SettingsService)
        {
            webDriverService = WebDriverService;
            webElementService = WebElementService;
            settingsService = SettingsService;
        }

        private Random random = new Random();
        private WebConnectionSettings settings;

        public async Task<bool> GoToMusicPage(Guid WebDriverId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                var goToPersonalPageBtn = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_aud", EnumWebHTMLElementSelector.TagName, "a").ConfigureAwait(false);
                result = webElementService.ClickToElement(goToPersonalPageBtn, EnumClickType.URLClick);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkMusicPageService", ex);
            }
            return result;
        }

        public async Task<bool> SwitchToMyMusicChapter(Guid WebDriverId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                var switchToSelfMusicBtn = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, "._audio_section_tab._audio_section_tab__my._audio_section_tab__all",
                                                                                                    EnumWebHTMLElementSelector.TagName, "a").ConfigureAwait(false);
                var switchResult = webElementService.ClickToElement(switchToSelfMusicBtn, EnumClickType.URLClick);
                if (switchResult)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        var firstTrek = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".audio_row_content._audio_row_content").ConfigureAwait(false);
                        if (firstTrek != null)
                        {
                            result = true;
                            break;
                        }
                        else
                            settingsService.WaitTime(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkMusicPageService", ex);
            }
            return result;
        
        }

        public async Task<bool> SwitchToRecomedations(Guid WebDriverId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                var switchToRecomedationsBtn = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, "._audio_section_tab._audio_section_tab__for_you._audio_section_tab__recoms",
                                                                                                    EnumWebHTMLElementSelector.TagName, "a").ConfigureAwait(false);
                var switchResult = webElementService.ClickToElement(switchToRecomedationsBtn, EnumClickType.URLClick);
                if (switchResult)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        var firstTrek = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".audio_row_content._audio_row_content").ConfigureAwait(false);
                        if (firstTrek != null)
                        {
                            result = true;
                            break;
                        }
                        else
                            settingsService.WaitTime(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkMusicPageService", ex);
            }
            return result;
        }

        public async Task<List<string>> GetAddedMusicVkIds(Guid WebDriverId)
        {
            var result = new List<string>();
            try
            {
                settings = settingsService.GetServerSettings();
                var audioContainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, "._audio_page__audio_rows_list._audio_pl").ConfigureAwait(false);
                if (audioContainer != null)
                {
                    var audios = webElementService.GetChildElements(audioContainer, EnumWebHTMLElementSelector.CSSSelector, ".audio_row.audio_row_with_cover._audio_row");
                    for (int i = 0; i < audios.Count; i++)
                    {
                        var vkId = webElementService.GetAttributeValue(audios[i], "data-full-id");
                        if ((vkId != null) && (vkId.Length > 0))
                            result.Add(vkId);
                    }
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkMusicPageService", ex);
            }
            return result;
        }

        public async Task<bool> PlayAddedMusicByVkId(Guid WebDriverId, string VkId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                var audio = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, "._audio_row_" + VkId).ConfigureAwait(false);
                if (webElementService.ClickToElement(audio, EnumClickType.ElementClick))
                    result = true;
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkMusicPageService", ex);
            }
            return result;
        }

        public async Task<bool> PlayRecomedationPlaylist(Guid WebDriverId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".audio_pl__actions_btn.audio_pl__actions_play", EnumClickType.ElementClick).ConfigureAwait(false))
                    result = true;
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkMusicPageService", ex);
            }
            return result;
        }

        public async Task<BotMusicModel> GetCurrentMusicName(Guid WebDriverId)
        {
            var result = new BotMusicModel();
            try
            {
                settings = settingsService.GetServerSettings();
                result.Artist = await webElementService.GetElementINNERText(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".artist_link", true).ConfigureAwait(false);
                result.SongName = await webElementService.GetElementINNERText(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".audio_page_player_title_song_title", true).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkMusicPageService", ex);
            }
            return result;
        }

        public async Task<bool> PlayNextMusic(Guid WebDriverId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".top_audio_player_btn.top_audio_player_next", EnumClickType.ElementClick).ConfigureAwait(false))
                    result = true;
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkMusicPageService", ex);
            }
            return result;
        }

        public async Task<bool> StopMusic(Guid WebDriverId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                var stopBtnText = await webElementService.GetElementINNERText(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".blind_label._top_audio_player_play_blind_label", true).ConfigureAwait(false);
                if ((stopBtnText != null) && (stopBtnText.Length > 0) && (stopBtnText.IndexOf("произвес") == -1))
                    await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".top_audio_player_btn.top_audio_player_play._top_audio_player_play", EnumClickType.ElementClick).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkMusicPageService", ex);
            }
            return result;
        }
    }
}
