using BotClient.Bussines.Interfaces;
using BotClient.Bussines.Interfaces.VkPages;
using BotClient.Models.HTMLElements.Enumerators;
using OpenQA.Selenium;
using System;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services.VkPages
{
    public class VkStandartActionService : IVkStandartActionService
    {
        private readonly IWebDriverService webDriverService;
        private readonly ISettingsService settingsService;
        private readonly IWebElementService webElementService;
        public VkStandartActionService(IWebDriverService WebDriverService,
                               ISettingsService SettingsService,
                               IWebElementService WebElementService)
        {
            webDriverService = WebDriverService;
            settingsService = SettingsService;
            webElementService = WebElementService;
        }
        Random random = new Random();

        public async Task<bool> CloseModalWindow(Guid WebDriverId)
        {
            var result = false;
            try
            {
                if (await webDriverService.isUrlContains(WebDriverId, "vk.com/im") == false)
                    result = await webElementService.SendKeyToElement(WebDriverId, EnumWebHTMLElementSelector.TagName, "body", Keys.Escape).ConfigureAwait(false);
                else
                {
                    var blockWindow = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".popup_box_container").ConfigureAwait(false);
                    if (blockWindow != null)
                    {
                        var closeBtn = webElementService.GetElementInElement(blockWindow, EnumWebHTMLElementSelector.CSSSelector, ".box_x_button");
                        result = webElementService.ClickToElement(closeBtn, EnumClickType.ElementClick);
                    }
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }
        public async Task<bool> CloseMessageBlockWindow(Guid WebDriverId)
        {
            var result = false;
            try
            {
                var element = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "box_layer_wrap").ConfigureAwait(false);
                var attribute = webElementService.GetAttributeValue(element, "style");
                if ((attribute != null) && (attribute.IndexOf("block;", StringComparison.Ordinal) != -1))
                {
                    result = await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                }
                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "vkconnect_continue_button", EnumClickType.ElementClick).ConfigureAwait(false))
                    result = true;
                element = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".popup_box_container").ConfigureAwait(false);
                if (element != null)
                    result = webElementService.ClickToElement(element, EnumClickType.ElementClick);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }
    }
}
