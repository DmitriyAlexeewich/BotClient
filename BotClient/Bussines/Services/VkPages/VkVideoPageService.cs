using BotClient.Bussines.Interfaces;
using BotClient.Models.Bot;
using BotClient.Models.HTMLElements.Enumerators;
using BotDataModels.Bot;
using BotDataModels.Settings;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services.VkPages
{
    public class VkVideoPageService
    {

        private readonly IWebDriverService webDriverService;
        private readonly IWebElementService webElementService;
        private readonly ISettingsService settingsService;

        public VkVideoPageService(IWebDriverService WebDriverService,
                              IWebElementService WebElementService,
                              ISettingsService SettingsService)
        {
            webDriverService = WebDriverService;
            webElementService = WebElementService;
            settingsService = SettingsService;
        }

        private Random random = new Random();
        private WebConnectionSettings settings;

        public async Task<bool> GoToVideoPage(Guid WebDriverId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                var goToPersonalPageBtn = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_vid", EnumWebHTMLElementSelector.TagName, "a").ConfigureAwait(false);
                result = webElementService.ClickToElement(goToPersonalPageBtn, EnumClickType.URLClick);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkMusicPageService", ex);
            }
            return result;
        }

        public async Task<bool> PlayRecomedationVideo(Guid WebDriverId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                var recomendationsVideoBlock = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id, "video_main_block",
                                                                                                        EnumWebHTMLElementSelector.CSSSelector, ".videocat_autoplay_video_wrap").ConfigureAwait(false);
                if (recomendationsVideoBlock != null)
                {
                    var recomedationsVideo = webElementService.GetElementInElement(recomendationsVideoBlock, EnumWebHTMLElementSelector.TagName, "a");
                    result = webElementService.ClickToElement(recomedationsVideo, EnumClickType.URLClick);

                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkMusicPageService", ex);
            }
            return result;
        }

        public async Task<bool> PlayLiveTranslation(Guid WebDriverId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                var recomendationsVideoBlock = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id, "videocat_other_blocks",
                                                                                                        EnumWebHTMLElementSelector.CSSSelector, ".videocat_autoplay_video_wrap").ConfigureAwait(false);
                if (recomendationsVideoBlock != null)
                {
                    var recomedationsVideo = webElementService.GetElementInElement(recomendationsVideoBlock, EnumWebHTMLElementSelector.TagName, "a");
                    result = webElementService.ClickToElement(recomedationsVideo, EnumClickType.URLClick);

                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkMusicPageService", ex);
            }
            return result;
        }

        public async Task<bool> SearchVideo(Guid WebDriverId, string KeyWord)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                var searchedVideoContainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "video_search_global_videos_list").ConfigureAwait(false);
                if (searchedVideoContainer != null)
                {
                    var searchedVideos = webElementService.GetChildElements(searchedVideoContainer, EnumWebHTMLElementSelector.CSSSelector, ".video_item._video_item");
                    if (await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "video_search_input", KeyWord).ConfigureAwait(false))
                    {
                        if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".ui_search_button_search._ui_search_button_search", EnumClickType.ElementClick).ConfigureAwait(false))
                        {
                            var checkVideoAtteptCount = 5;
                            for (int i = 0; i < checkVideoAtteptCount; i++)
                            {
                                var tempSearchedVideos = webElementService.GetChildElements(searchedVideoContainer, EnumWebHTMLElementSelector.CSSSelector, ".video_item._video_item");
                                if (searchedVideos.Count != tempSearchedVideos.Count)
                                {
                                    i = -1;
                                    searchedVideos = tempSearchedVideos;
                                }
                                else
                                    settingsService.WaitTime(1000);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkMusicPageService", ex);
            }
            return result;
        }

        public async Task<List<BotVkVideo>> GetSearchVideoResult(Guid WebDriverId)
        {
            var result = new List<BotVkVideo>();
            try
            {
                settings = settingsService.GetServerSettings();
                var searchedVideos = await webElementService.GetChildElements(WebDriverId, EnumWebHTMLElementSelector.Id, "video_search_global_videos_list", 
                                                                                            EnumWebHTMLElementSelector.CSSSelector, ".video_item._video_item");
                for (int i = 0; i < searchedVideos.Count; i++)
                {
                    var videoVkId = webElementService.GetAttributeValue(searchedVideos[i], "data-id");
                    var linkElement = webElementService.GetElementInElement(searchedVideos[i], EnumWebHTMLElementSelector.TagName, "a");
                    var url = webElementService.GetAttributeValue(linkElement, "href");
                    result.Add(new BotVkVideo()
                    {
                        VkId = videoVkId,
                        BotVideo = new BotVideoModel()
                        {
                            URL = url
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkMusicPageService", ex);
            }
            return result;
        }

        public async Task<bool> PlaySearchedVideo(Guid WebDriverId, string VkId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                var videoElement = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id, "video_item_" + VkId, EnumWebHTMLElementSelector.TagName, "a").ConfigureAwait(false);
                if (webElementService.ClickToElement(videoElement, EnumClickType.URLClick))
                    result = false;
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkMusicPageService", ex);
            }
            return result;
        }

        public async Task<bool> isLikable(Guid WebDriverId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                var likeContainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "mv_main_info").ConfigureAwait(false);
                var likeBtn = webElementService.GetElementInElement(likeContainer, EnumWebHTMLElementSelector.CSSSelector, ".like_btn.like._like");
                if (likeBtn != null)
                    result = true;
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkMusicPageService", ex);
            }
            return result;
        }

        public async Task<bool> isAddable(Guid WebDriverId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                var addBtn = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "mv_add_button").ConfigureAwait(false);
                if (addBtn != null)
                    result = true;
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkMusicPageService", ex);
            }
            return result;
        }

        public async Task<bool> isRepostable(Guid WebDriverId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                var repostContainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "mv_main_info").ConfigureAwait(false);
                var repostBtn = webElementService.GetElementInElement(repostContainer, EnumWebHTMLElementSelector.CSSSelector, ".like_btn.share._share");
                if (repostBtn != null)
                    result = true;
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkMusicPageService", ex);
            }
            return result;
        }

        public async Task<int> GetVideoDuration(Guid WebDriverId)
        {
            var result = 0;
            try
            {
                settings = settingsService.GetServerSettings();
                var videoDurationText = await webElementService.GetElementINNERText(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, "._time_duration", true).ConfigureAwait(false);
                if ((videoDurationText != null) && (videoDurationText.Length > 0))
                {
                    var videoDurationInSeconds = 0;
                    var videoDurationParts = videoDurationText.Split(":").ToList();
                    videoDurationParts.Reverse();
                    for (int i = 0; i < videoDurationParts.Count; i++)
                    {
                        int tempTime = 0;
                        Int32.TryParse(videoDurationParts[i], out tempTime);
                        videoDurationInSeconds += tempTime * (int)Math.Pow(60, i);
                    }
                    if (videoDurationInSeconds > 0)
                        result = videoDurationInSeconds * 1000;
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkMusicPageService", ex);
            }
            return result;
        }

        public async Task<bool> Like(Guid WebDriverId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                var likeContainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "mv_main_info").ConfigureAwait(false);
                var likeBtn = webElementService.GetElementInElement(likeContainer, EnumWebHTMLElementSelector.CSSSelector, ".like_btn.like._like");
                result = webElementService.ClickToElement(likeBtn, EnumClickType.ElementClick);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkMusicPageService", ex);
            }
            return result;
        }

        public async Task<bool> Add(Guid WebDriverId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                var addBtn = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "mv_add_button").ConfigureAwait(false);
                result = webElementService.ClickToElement(addBtn, EnumClickType.ElementClick);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkMusicPageService", ex);
            }
            return result;
        }

        public async Task<bool> Repost(Guid WebDriverId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                var repostContainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "mv_main_info").ConfigureAwait(false);
                var repostBtn = webElementService.GetElementInElement(repostContainer, EnumWebHTMLElementSelector.CSSSelector, ".like_btn.share._share");
                if (webElementService.ClickToElement(repostBtn, EnumClickType.ElementClick))
                {
                    //repost from stadart action
                    result = true;
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkMusicPageService", ex);
            }
            return result;
        }

        public async Task<bool> Stop(Guid WebDriverId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                var closeBtn = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id, "VideoLayerInfo__topControls",
                                                                                        EnumWebHTMLElementSelector.CSSSelector, ".mv_top_button.mv_top_close").ConfigureAwait(false);
                if (webElementService.ClickToElement(closeBtn, EnumClickType.URLClick))
                {
                    result = true;
                }
                else
                {
                    var videoBlock = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "mv_layer_wrap").ConfigureAwait(false);
                    if (webElementService.SendKeyToElement(videoBlock, Keys.Escape))
                        result = true;
                }
                
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkMusicPageService", ex);
            }
            return result;
        }
    }
}
