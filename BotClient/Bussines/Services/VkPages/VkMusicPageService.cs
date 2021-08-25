using BotClient.Bussines.Interfaces;
using BotClient.Models.HTMLElements.Enumerators;
using BotDataModels.Settings;
using System;
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
                await settingsService.AddLog("VkMusicPageService", ex);
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
                await settingsService.AddLog("VkMusicPageService", ex);
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
                await settingsService.AddLog("VkMusicPageService", ex);
            }
            return result;
        }
    }
}
