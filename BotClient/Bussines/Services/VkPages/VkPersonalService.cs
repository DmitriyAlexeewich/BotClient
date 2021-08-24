using BotClient.Bussines.Interfaces;
using BotClient.Models.HTMLElements.Enumerators;
using BotDataModels.Bot;
using BotDataModels.Settings;
using OpenQA.Selenium;
using System;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services.VkPages
{
    public class VkPersonalService
    {

        private readonly IWebDriverService webDriverService;
        private readonly IWebElementService webElementService;
        private readonly ISettingsService settingsService;

        public VkPersonalService(IWebDriverService WebDriverService,
                              IWebElementService WebElementService,
                              ISettingsService SettingsService)
        {
            webDriverService = WebDriverService;
            webElementService = WebElementService;
            settingsService = SettingsService;
        }

        private Random random = new Random();
        private WebConnectionSettings settings;

        public async Task<bool> GoToPersonalPage(Guid WebDriverId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                var goToPersonalPageBtn = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_pr", EnumWebHTMLElementSelector.TagName, "a").ConfigureAwait(false);
                result = webElementService.ClickToElement(goToPersonalPageBtn, EnumClickType.URLClick);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkPersonalService", ex);
            }
            return result;
        }

        public async Task<string> GetPageName(Guid WebDriverId)
        {
            var result = "";
            try
            {
                settings = settingsService.GetServerSettings();
                result = await webElementService.GetElementINNERText(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".page_name", true).ConfigureAwait(false);
                if (result == null)
                    result = "";
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkPersonalService", ex);
            }
            return result;
        }

        public async Task<bool> isFemalePage(Guid WebDriverId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                var genderElement = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".profile_online_lv").ConfigureAwait(false);
                if (genderElement != null)
                {
                    var genderText = webElementService.GetElementINNERText(genderElement, true);
                    if (genderText.IndexOf("ла") != -1)
                        result = true;
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkPersonalService", ex);
            }
            return result;
        }

        public async Task<bool> Customize(Guid WebDriverId, BotCustomizeModel CustomizeData)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                await webElementService.ScrollElementJs(WebDriverId, EnumWebHTMLElementSelector.TagName, "body", true).ConfigureAwait(false);
                //await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_pr", EnumClickType.URLClick).ConfigureAwait(false))
                {
                    //await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                    if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "profile_edit_act", EnumClickType.URLClick).ConfigureAwait(false))
                    {
                        //await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                        var customizeBufferResult = true;
                        if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "ui_rmenu_general", EnumClickType.URLClick).ConfigureAwait(false))
                        {
                            //await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                            await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "pedit_home_town", CustomizeData.City).ConfigureAwait(false);
                            await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "pedit_add_grandparent_link", CustomizeData.Grands).ConfigureAwait(false);
                            await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "pedit_add_parent_link", CustomizeData.Parents).ConfigureAwait(false);
                            await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "pedit_add_sibling_link", CustomizeData.SistersAndBrothers).ConfigureAwait(false);
                            await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "pedit_add_child_link", CustomizeData.Childs).ConfigureAwait(false);
                            await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "pedit_add_grandchild_link", CustomizeData.GrandChilds).ConfigureAwait(false);
                            customizeBufferResult = await SaveCustomize(WebDriverId, "pedit_general").ConfigureAwait(false);
                        }
                        if ((customizeBufferResult) && (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "ui_rmenu_contacts", EnumClickType.URLClick).ConfigureAwait(false)))
                        {
                            //await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                            await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "pedit_mobile", CustomizeData.MobilePhone).ConfigureAwait(false);
                            await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "pedit_home", CustomizeData.HomePhone).ConfigureAwait(false);
                            await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "pedit_skype", CustomizeData.Skype).ConfigureAwait(false);
                            await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "pedit_website", CustomizeData.JobSite).ConfigureAwait(false);
                            var countryCountainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "pedit_country_row").ConfigureAwait(false);
                            var countrySelectorBtn = webElementService.GetElementInElement(countryCountainer, EnumWebHTMLElementSelector.TagName, "input");
                            if (webElementService.ClickToElement(countrySelectorBtn, EnumClickType.ElementClick))
                            {
                                var countryRows = webElementService.GetChildElements(countryCountainer, EnumWebHTMLElementSelector.TagName, "li");
                                for (int i = 0; i < countryRows.Count; i++)
                                {
                                    if (webElementService.GetElementINNERText(countryRows[i], true).IndexOf(CustomizeData.Coutry) != -1)
                                        webElementService.ClickToElement(countryRows[i], EnumClickType.ElementClick);
                                }
                            }
                            var citySearchTextBox = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id, "pedit_city_row",
                                                                                                EnumWebHTMLElementSelector.TagName, "input").ConfigureAwait(false);
                            if ((webElementService.ClearElement(citySearchTextBox)) && (webElementService.PrintTextToElement(citySearchTextBox, CustomizeData.City)))
                            {
                                settingsService.WaitTime(5000);
                                webElementService.SendKeyToElement(citySearchTextBox, Keys.Return);
                            }
                            customizeBufferResult = await SaveCustomize(WebDriverId, "pedit_contacts").ConfigureAwait(false);
                        }
                        if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "ui_rmenu_interests", EnumClickType.URLClick).ConfigureAwait(false))
                        {
                            //await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                            await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "pedit_interests_activities", CustomizeData.Job).ConfigureAwait(false);
                            await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "pedit_interests_interests", CustomizeData.Interest).ConfigureAwait(false);
                            await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "pedit_interests_music", CustomizeData.FavoriteMusic).ConfigureAwait(false);
                            await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "pedit_interests_movies", CustomizeData.FavoriteFilms).ConfigureAwait(false);
                            await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "pedit_interests_tv", CustomizeData.FavoriteTVShows).ConfigureAwait(false);
                            await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "pedit_interests_books", CustomizeData.FavoriteBook).ConfigureAwait(false);
                            await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "pedit_interests_games", CustomizeData.FavoriteGame).ConfigureAwait(false);
                            await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "pedit_interests_quotes", CustomizeData.FavoriteQuote).ConfigureAwait(false);
                            await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "pedit_interests_about", CustomizeData.PersonalInfo).ConfigureAwait(false);
                            customizeBufferResult = await SaveCustomize(WebDriverId, "pedit_interests").ConfigureAwait(false);
                        }
                        if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "ui_rmenu_personal", EnumClickType.URLClick).ConfigureAwait(false))
                        {
                            //await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                            var selectorContainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".pedit_labeled").ConfigureAwait(false);
                            if (webElementService.ClickToElement(selectorContainer, EnumClickType.ElementClick))
                            {
                                var variations = webElementService.GetChildElements(selectorContainer, EnumWebHTMLElementSelector.TagName, "li");
                                for (int i = 0; i < variations.Count; i++)
                                {
                                    var innerText = webElementService.GetElementINNERText(variations[i], true);
                                    if (innerText.IndexOf(CustomizeData.Political) != -1)
                                        webElementService.ClickToElement(variations[i], EnumClickType.ElementClick);
                                }
                            }
                            customizeBufferResult = await SaveCustomize(WebDriverId, "pedit_general").ConfigureAwait(false);
                        }
                        result = customizeBufferResult;
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkPersonalService", ex);
            }
            return result;
        }

        private async Task<bool> SaveCustomize(Guid WebDriverId, string BtnParentId)
        {
            try
            {
                if (true /*await CloseModalWindow(WebDriverId).ConfigureAwait(false)*/)
                {
                    var saveBtn = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id, BtnParentId, EnumWebHTMLElementSelector.TagName, "button").ConfigureAwait(false);
                    if ((saveBtn != null) && (webElementService.ClickToElement(saveBtn, EnumClickType.ElementClick)))
                    {
                        if (true /*await CloseModalWindow(WebDriverId).ConfigureAwait(false)*/)
                        {
                            var saveResultMessageContainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "pedit_result").ConfigureAwait(false);
                            if (await webDriverService.hasWebHTMLElement(WebDriverId, saveResultMessageContainer, EnumWebHTMLElementSelector.CSSSelector, ".msg.ok_msg").ConfigureAwait(false))
                                return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return false;
        }

        public async Task<bool> ShowTopProfileMenu(Guid WebDriverId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                result = await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "top_profile_link", EnumClickType.ElementClick).ConfigureAwait(false);
                settingsService.WaitTime(random.Next(1000, 5000));
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkPersonalService", ex);
            }
            return result;
        }

        public async Task<bool> GoToSettingsPage(Guid WebDriverId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                result = await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "top_settings_link", EnumClickType.URLClick).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkPersonalService", ex);
            }
            return result;
        }

        public async Task<bool> ChangePassword(Guid WebDriverId, string OldPassword, string NewPassword)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                var changePasswordLink = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id, "chgpass",
                                                                                     EnumWebHTMLElementSelector.CSSSelector, ".settings_right_control").ConfigureAwait(false);
                if (webElementService.ClickToElement(changePasswordLink, EnumClickType.ElementClick))
                {
                    if (await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "settings_old_pwd", OldPassword).ConfigureAwait(false))
                    {
                        if (await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "settings_new_pwd", NewPassword).ConfigureAwait(false))
                        {
                            if (await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "settings_confirm_pwd", NewPassword).ConfigureAwait(false))
                            {
                                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "settings_pwd_btn", EnumClickType.ElementClick).ConfigureAwait(false))
                                    result = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkPersonalService", ex);
            }
            return result;
        }

        public async Task<bool> LogOut(Guid WebDriverId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                result = await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "top_logout_link", EnumClickType.URLClick).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkPersonalService", ex);
            }
            return result;
        }

    }
}
