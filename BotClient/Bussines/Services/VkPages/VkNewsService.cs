using BotClient.Bussines.Interfaces;
using BotClient.Bussines.Interfaces.VkPages;
using BotClient.Models.HTMLElements;
using BotClient.Models.HTMLElements.Enumerators;
using BotDataModels.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services.VkPages
{
    public class VkNewsService
    {
        private readonly IWebDriverService webDriverService;
        private readonly IWebElementService webElementService;
        private readonly ISettingsService settingsService;
        private readonly IVkStandartActionService vkStandartActionService;

        public VkNewsService(IWebDriverService WebDriverService,
                             IWebElementService WebElementService,
                             ISettingsService SettingsService,
                             IFileSystemService FileSystemService,
                             IVkStandartActionService VkStandartActionService)
        {
            webDriverService = WebDriverService;
            webElementService = WebElementService;
            settingsService = SettingsService;
            vkStandartActionService = VkStandartActionService;
        }

        private Random random = new Random();
        private WebConnectionSettings settings;

        public async Task<bool> AddCommentToNews(Guid WebDriverId, string VkId, string Comment)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                var news = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "post-" + VkId).ConfigureAwait(false);
                if (news != null)
                {
                
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        private async Task<WebHTMLElement> GetNewsElement(Guid WebDriverId, string VkId)
        {
            WebHTMLElement result = null;
            try
            {
                settings = settingsService.GetServerSettings();
                var news = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "post-" + VkId).ConfigureAwait(false);
                if (news != null)
                {

                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }
    }
}
