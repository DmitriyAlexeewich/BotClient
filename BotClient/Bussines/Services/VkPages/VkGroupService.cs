using BotClient.Bussines.Interfaces;
using BotClient.Bussines.Interfaces.VkPages;
using BotClient.Models.Bot;
using BotClient.Models.Bot.Enumerators;
using BotClient.Models.HTMLElements;
using BotClient.Models.HTMLElements.Enumerators;
using BotDataModels.Settings;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services.VkPages
{
    public class VkGroupService
    {
        private readonly IWebDriverService webDriverService;
        private readonly IWebElementService webElementService;
        private readonly ISettingsService settingsService;
        private readonly IVkStandartActionService vkStandartActionService;

        public VkGroupService(IWebDriverService WebDriverService,
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

        public async Task<bool> GoToGroupsSection(Guid WebDriverId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                var goToGroupBtn = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_gr", EnumWebHTMLElementSelector.TagName, "a").ConfigureAwait(false);
                result = webElementService.ClickToElement(goToGroupBtn, EnumClickType.URLClick);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<bool> SearchGroups(Guid WebDriverId, string KeyWord, bool FilteredBySubscribersCount, EnumSearchGroupType SearchGroupType, string Country, string City, bool isSaftySearch)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "ui_rmenu_search", EnumClickType.ElementClick).ConfigureAwait(false))
                {
                    settingsService.WaitTime(5000);
                    if (KeyWord.Length > 0)
                    {
                        var searchInput = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "search_query").ConfigureAwait(false);
                        var printResult = webElementService.PrintTextToElement(searchInput, KeyWord);
                        if (printResult)
                            webElementService.SendKeyToElement(searchInput, Keys.Return);
                    }
                    if (FilteredBySubscribersCount)
                    {
                        var expandSunscribeSelectorClick = await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "sort_filter", EnumClickType.ElementClick).ConfigureAwait(false);
                        if (expandSunscribeSelectorClick)
                        {
                            var selectors = await webElementService.GetChildElements(WebDriverId, EnumWebHTMLElementSelector.Id, "list_options_container_1", EnumWebHTMLElementSelector.TagName, "li").ConfigureAwait(false);
                            if (selectors.Count > 1)
                                webElementService.ClickToElement(selectors[1], EnumClickType.ElementClick);
                        }
                    }
                    if (SearchGroupType != EnumSearchGroupType.AllTypes)
                    {
                        var expandGroupTypeSelectorClick = await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "c[type]", EnumClickType.ElementClick).ConfigureAwait(false);
                        if (expandGroupTypeSelectorClick)
                        {
                            var selectors = await webElementService.GetChildElements(WebDriverId, EnumWebHTMLElementSelector.Id, "list_options_container_2", EnumWebHTMLElementSelector.TagName, "li").ConfigureAwait(false);
                            if (selectors.Count > 1)
                            {
                                if (SearchGroupType == EnumSearchGroupType.Group)
                                    webElementService.ClickToElement(selectors[1], EnumClickType.ElementClick);
                                else
                                    webElementService.ClickToElement(selectors[2], EnumClickType.ElementClick);
                            }
                        }
                    }
                    if (Country.Length > 0)
                    {
                        var countryContainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "cCountry").ConfigureAwait(false);
                        var clickResult = webElementService.ClickToElement(countryContainer, EnumClickType.ElementClick);
                        var textField = webElementService.GetElementInElement(countryContainer, EnumWebHTMLElementSelector.CSSSelector, ".selector_input");
                        if (clickResult)
                        {
                            webElementService.PrintTextToElement(textField, Country);
                            settingsService.WaitTime(5000);
                            webElementService.SendKeyToElement(textField, Keys.Return);
                            if (City.Length > 0)
                            {
                                var cityContainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "cCity").ConfigureAwait(false);
                                clickResult = webElementService.ClickToElement(cityContainer, EnumClickType.ElementClick);
                                textField = webElementService.GetElementInElement(cityContainer, EnumWebHTMLElementSelector.CSSSelector, ".selector_input");
                                if (clickResult)
                                {
                                    webElementService.PrintTextToElement(textField, City);
                                    settingsService.WaitTime(5000);
                                    webElementService.SendKeyToElement(textField, Keys.Return);
                                }
                            }
                        }
                    }
                    if (!isSaftySearch)
                        await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "safe_search", EnumClickType.ElementClick).ConfigureAwait(false);
                    result = true;
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<List<ParsedGroupModel>> GetSearchResultGroups(Guid WebDriverId, string VkId = "")
        {
            var result = new List<ParsedGroupModel>();
            try
            {
                settings = settingsService.GetServerSettings();
                await vkStandartActionService.CloseModalWindow(WebDriverId).ConfigureAwait(false);
                await vkStandartActionService.CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                var body = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.TagName, "body").ConfigureAwait(false);
                var groups = new List<WebHTMLElement>();
                var groupsCount = 0;
                var isGroupsSearch = true;
                if (await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "groups_list_groups").ConfigureAwait(false) != null)
                    isGroupsSearch = false;

                for (int i = 0; i < 60; i++)
                {
                    webElementService.ScrollElement(body);
                    settingsService.WaitTime(1000);
                    if (isGroupsSearch)
                        groups = webElementService.GetChildElements(body, EnumWebHTMLElementSelector.CSSSelector, ".group_list_row.clear_fix._gl_row");
                    else
                        groups = webElementService.GetChildElements(body, EnumWebHTMLElementSelector.CSSSelector, ".groups_row.search_row.clear_fix");
                    if (groups.Count > groupsCount)
                        groupsCount = groups.Count;
                    else
                        break;
                }
                if (isGroupsSearch)
                {
                    groups = webElementService.GetChildElements(body, EnumWebHTMLElementSelector.CSSSelector, ".groups_row.search_row.clear_fix");
                    for (int i = 0; i < groups.Count; i++)
                    {
                        var groupName = webElementService.GetElementInElement(groups[i], EnumWebHTMLElementSelector.CSSSelector, ".labeled.title");
                        var groupId = webElementService.GetAttributeValue(groups[i], "data-id");
                        var subscribersCount = 0;
                        var subscribersCountElement = webElementService.GetElementInElement(groups[i], EnumWebHTMLElementSelector.CSSSelector, ".labeled.labeled_link");
                        if (subscribersCountElement != null)
                        {
                            var subscribersCountText = webElementService.GetElementINNERText(subscribersCountElement, true);
                            subscribersCountText = subscribersCountText.Replace(" ", "");
                            if ((subscribersCountText != null) && (subscribersCountText.Length > 0))
                            {
                                subscribersCountText = new String(subscribersCountText.TakeWhile(Char.IsDigit).ToArray());
                                if (subscribersCountText.Length > 0)
                                {
                                    if (!int.TryParse(subscribersCountText, out subscribersCount))
                                        subscribersCount = 0;
                                }
                            }
                        }
                        var isAdding = true;
                        if (VkId.Length > 0)
                        {
                            if (groupId.IndexOf(VkId) != -1)
                                isAdding = true;
                            else
                                isAdding = false;
                        }
                        if (isAdding)
                        {
                            result.Add(new ParsedGroupModel()
                            {
                                GroupName = webElementService.GetElementINNERText(groupName, true),
                                GroupVkId = groupId,
                                GroupElement = groups[i],
                                SubscribersCount = subscribersCount
                            });
                        }
                    }
                }
                else
                {
                    groups = webElementService.GetChildElements(body, EnumWebHTMLElementSelector.CSSSelector, ".group_list_row.clear_fix._gl_row");
                    for (int i = 0; i < groups.Count; i++)
                    {
                        var innerText = webElementService.GetElementINNERText(groups[i], true);
                        if (innerText.IndexOf("Закрытая группа") == -1)
                        {
                            var groupName = webElementService.GetElementInElement(groups[i], EnumWebHTMLElementSelector.CSSSelector, ".group_row_title");
                            var groupId = webElementService.GetAttributeValue(groups[i], "id");
                            result.Add(new ParsedGroupModel()
                            {
                                GroupName = webElementService.GetElementINNERText(groupName, true),
                                GroupVkId = groupId.Replace("gl_groups", "")
                            });
                        }
                    }
                }
                await vkStandartActionService.CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                await vkStandartActionService.CloseModalWindow(WebDriverId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<List<ParsedGroupModel>> GetClientGroups(Guid WebDriverId)
        {
            var result = new List<ParsedGroupModel>();
            try
            {
                settings = settingsService.GetServerSettings();
                await vkStandartActionService.CloseModalWindow(WebDriverId).ConfigureAwait(false);
                await vkStandartActionService.CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                var body = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.TagName, "body").ConfigureAwait(false);
                var groups = new List<WebHTMLElement>();
                var groupsCount = 0;

                for (int i = 0; i < 60; i++)
                {
                    webElementService.ScrollElement(body);
                    settingsService.WaitTime(1000);
                    groups = webElementService.GetChildElements(body, EnumWebHTMLElementSelector.CSSSelector, ".groups_row.search_row.clear_fix");
                    if (groups.Count > groupsCount)
                        groupsCount = groups.Count;
                    else
                        break;
                }
                groups = webElementService.GetChildElements(body, EnumWebHTMLElementSelector.CSSSelector, ".group_list_row.clear_fix._gl_row");
                for (int i = 0; i < groups.Count; i++)
                {
                    var innerText = webElementService.GetElementINNERText(groups[i], true);
                    if (innerText.IndexOf("Закрытая группа") == -1)
                    {
                        var groupName = webElementService.GetElementInElement(groups[i], EnumWebHTMLElementSelector.CSSSelector, ".group_row_title");
                        var groupId = webElementService.GetAttributeValue(groups[i], "id");
                        result.Add(new ParsedGroupModel()
                        {
                            GroupName = webElementService.GetElementINNERText(groupName, true),
                            GroupVkId = groupId.Replace("gl_groups", "")
                        });
                    }
                }
                await vkStandartActionService.CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                await vkStandartActionService.CloseModalWindow(WebDriverId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<bool> SubscribeToGroupBySearchResult(Guid WebDriverId, string GroupVkId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                var subscribeBtn = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "search_sub-" + GroupVkId).ConfigureAwait(false);
                var btnStyle = webElementService.GetAttributeValue(subscribeBtn, "style");
                if ((btnStyle != null) && (btnStyle.Length > 0) && (btnStyle.IndexOf("none") == -1))
                    result = await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "search_sub-" + GroupVkId, EnumClickType.ElementClick).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex);
            }
            return result;        
        }

        public async Task<bool> GoToGroupBySearchResult(Guid WebDriverId, string GroupVkId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                var subscribeBtn = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "search_sub-" + GroupVkId).ConfigureAwait(false);
                var groupElement = await webElementService.GetParentElement(subscribeBtn, 2).ConfigureAwait(false);
                if (groupElement != null)
                {
                    var groupLinkElement = webElementService.GetElementInElement(groupElement, EnumWebHTMLElementSelector.CSSSelector, "info");
                    groupLinkElement = webElementService.GetElementInElement(groupLinkElement, EnumWebHTMLElementSelector.TagName, "a");
                    if (groupLinkElement != null)
                        result = webElementService.ClickToElement(groupLinkElement, EnumClickType.URLClick);
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<bool> GoToGroupByClientGroups(Guid WebDriverId, string GroupVkId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                var groupElement = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "gl_groups" + GroupVkId).ConfigureAwait(false);
                groupElement = webElementService.GetElementInElement(groupElement, EnumWebHTMLElementSelector.CSSSelector, ".group_row_title");
                result = webElementService.ClickToElement(groupElement, EnumClickType.URLClick);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<bool> SubscribeToGroupInGroupPage(Guid WebDriverId)
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                await vkStandartActionService.CloseModalWindow(WebDriverId).ConfigureAwait(false);
                await vkStandartActionService.CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                var subscribeBtn = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "public_subscribe").ConfigureAwait(false);
                if(subscribeBtn == null)
                    subscribeBtn = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "join_button").ConfigureAwait(false);
                result = webElementService.ClickToElement(subscribeBtn, EnumClickType.ElementClick);
                await vkStandartActionService.CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                await vkStandartActionService.CloseModalWindow(WebDriverId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<string> GetGroupName(Guid WebDriverId)
        {
            var result = "";
            try
            {
                settings = settingsService.GetServerSettings();
                result = await webElementService.GetElementINNERText(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".page_name", true).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<List<PlatformPostModel>> GetGroupPosts(Guid WebDriverId)
        {
            var result = new List<PlatformPostModel>();
            try
            {
                settings = settingsService.GetServerSettings();
                var posts = new List<WebHTMLElement>();
                var body = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.TagName, "body").ConfigureAwait(false);
                var postsContainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "page_wall_posts").ConfigureAwait(false);
                for (int i = 0; i < 100; i++)
                {
                    settingsService.WaitTime(3000);
                    var tempPosts = webElementService.GetChildElements(postsContainer, EnumWebHTMLElementSelector.CSSSelector, "._post.post");
                    if (tempPosts.Count > posts.Count)
                        posts = tempPosts;
                    else
                        break;
                    webElementService.ScrollElement(body);
                }
                for (int i = 0; i < posts.Count; i++)
                {
                    var raiting = 0;
                    string[] raitingElementsCSS = { ".like_views", ".like_btn.like._like", ".like_btn.share._share", ".like_btn.comment._comment" };
                    for (int j = 0; j < raitingElementsCSS.Length; j++)
                    {
                        var raitingElement = webElementService.GetElementInElement(posts[i], EnumWebHTMLElementSelector.CSSSelector, raitingElementsCSS[j]);
                        var raitingText = webElementService.GetElementINNERText(raitingElement, true);
                        if ((raitingText != null) && (raitingText.Length > 0))
                        {
                            int raitingMultiplier = 1;
                            if (raitingText.IndexOf("K") != -1)
                                raitingMultiplier = 1000;
                            raitingMultiplier *= (int)Math.Pow(10, j);
                            raitingText = Regex.Match(raitingText, @"\d+").Value;

                            if ((raitingText.Length > 0) && (Int32.TryParse(raitingText, out raiting)))
                                raiting *= raitingMultiplier;
                        }
                    }
                    var postId = webElementService.GetAttributeValue(posts[i], "id");
                    if (postId.Length > 0)
                    {
                        postId = postId.Replace("post-", "");
                        result.Add(new PlatformPostModel(postId, null, raiting));
                    }
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }
        
        public async Task<bool> CreatePost(Guid WebDriverId, string Text = "", string FilePath = "")
        {
            var result = false;
            try
            {
                settings = settingsService.GetServerSettings();
                if (await AddFileToPost(WebDriverId, FilePath).ConfigureAwait(false))
                {
                    if ((Text.Length < 1) || (await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "post_field", Text).ConfigureAwait(false)))
                        result = await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "send_post", EnumClickType.ElementClick).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        private async Task<bool> AddFileToPost(Guid WebDriverId, string FilePath = "")
        {
            var result = false;
            try
            {
                if (FilePath.Length > 0)
                {
                    var addPhotoBtn = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id, "page_add_media",
                                                                                    EnumWebHTMLElementSelector.CSSSelector, ".ms_item.ms_item_photo._type_photo").ConfigureAwait(false);
                    if (webElementService.ClickToElement(addPhotoBtn, EnumClickType.ElementClick))
                    {
                        if (await webElementService.SendKeyToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "choose_photo_upload", FilePath).ConfigureAwait(false))
                        {
                            for (int i = 0; i < 60; i++)
                            {
                                settingsService.WaitTime(1000);
                                if (await webDriverService.hasWebHTMLElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".preview._preview").ConfigureAwait(false))
                                {
                                    result = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                    result = true;
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }
    }
}
