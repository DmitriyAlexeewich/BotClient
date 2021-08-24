using BotClient.Bussines.Interfaces;
using BotClient.Models.HTMLElements.Enumerators;
using BotDataModels.Settings;
using System;
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

    }
}
