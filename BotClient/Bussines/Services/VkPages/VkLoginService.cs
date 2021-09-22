using BotClient.Bussines.Interfaces;
using BotClient.Models.HTMLElements;
using BotClient.Models.HTMLElements.Enumerators;
using BotDataModels.Settings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services.VkPages
{
    public class VkLoginService
    {

        private readonly IWebDriverService webDriverService;
        private readonly IWebElementService webElementService;
        private readonly ISettingsService settingsService;

        public VkLoginService(IWebDriverService WebDriverService,
                              IWebElementService WebElementService,
                              ISettingsService SettingsService)
        {
            webDriverService = WebDriverService;
            webElementService = WebElementService;
            settingsService = SettingsService;
        }

        private Random random = new Random();
        private WebConnectionSettings settings;

        public async Task<bool> EnterLogin(Guid WebDriverId, string Login)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                result = await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "index_email", Login).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkLoginService", ex);
            }
            return result;
        }

        public async Task<bool> EnterPassword(Guid WebDriverId, string Password)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                result = await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "index_pass", Password).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkLoginService", ex);
            }
            return result;
        }

        public async Task<bool> ClickEnter(Guid WebDriverId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                result = await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "index_login_button", EnumClickType.URLClick).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkLoginService", ex);
            }
            return result;
        }

        public async Task<bool> isLoginSuccess(Guid WebDriverId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                var checkElements = new List<WebHTMLElementModel>()
                {
                    new WebHTMLElementModel()
                    {
                        SelectorType = EnumWebHTMLElementSelector.Id,
                        Link = "login_blocked_wrap"
                    },
                    new WebHTMLElementModel()
                    {
                        SelectorType = EnumWebHTMLElementSelector.Id,
                        Link = "login_reg_button"
                    },
                    new WebHTMLElementModel()
                    {
                        SelectorType = EnumWebHTMLElementSelector.CSSSelector,
                        Link = ".FlatButton.FlatButton--positive.FlatButton--size-l.FlatButton--flexWide"
                    }
                };
                for (int i = 0; i < checkElements.Count; i++)
                {
                    var element = await webDriverService.GetElement(WebDriverId, checkElements[i].SelectorType, checkElements[i].Link).ConfigureAwait(false);
                    result = !webElementService.isElementAvailable(element);
                    if (!result)
                        break;
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkLoginService", ex);
            }
            return result;
        }
    }
}
