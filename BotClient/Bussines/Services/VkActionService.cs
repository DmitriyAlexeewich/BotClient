using BotClient.Bussines.Interfaces;
using BotClient.Models.Bot.Work;
using BotClient.Models.Bot.Work.Enumerators;
using BotClient.Models.Client;
using BotClient.Models.HTMLElements.Enumerators;
using BotDataModels.Bot;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services
{
    public class VkActionService : IVkActionService
    {
        private readonly IWebDriverService webDriverService;
        private readonly ISettingsService settingsService;
        private readonly IWebElementService webElementService;
        public VkActionService(IWebDriverService WebDriverService,
                               ISettingsService SettingsService,
                               IWebElementService WebElementService)
        {
            webDriverService = WebDriverService;
            settingsService = SettingsService;
            webElementService = WebElementService;
        }
        Random random = new Random();

        public async Task<AlgoritmResult> Login(Guid WebDriverId, string Username, string Password)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            try
            {
                if (await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "index_email", Username).ConfigureAwait(false))
                {
                    if (await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "index_pass", Password).ConfigureAwait(false))
                    {
                        if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "index_login_button", EnumClickType.URLClick).ConfigureAwait(false))
                        {
                            return new AlgoritmResult()
                            {
                                ActionResultMessage = EnumActionResult.Success,
                                hasError = false
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex.Message);
            }
            return result;
        }

        public async Task<bool> isLoginSuccess(Guid WebDriverId)
        {
            try
            {
                var checkElement = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_pr").ConfigureAwait(false);
                return webElementService.isElementAvailable(checkElement);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex.Message);
            }
            return false;
        }

        public async Task<AlgoritmResult> Customize(Guid WebDriverId, BotCustomizeModel CustomizeData)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            try
            {
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_pr", EnumClickType.URLClick).ConfigureAwait(false))
                {
                    if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "profile_edit_act", EnumClickType.URLClick).ConfigureAwait(false))
                    {
                        var customizeBufferResult = true;
                        if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "ui_rmenu_general", EnumClickType.URLClick).ConfigureAwait(false))
                        {
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
                                var cityRow = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.XPath, @"//*[@id=""list_options_container_9""]",
                                                                                                EnumWebHTMLElementSelector.TagName, "li").ConfigureAwait(false);
                                webElementService.ClickToElement(cityRow, EnumClickType.ElementClick);
                            }
                            customizeBufferResult = await SaveCustomize(WebDriverId, "pedit_contacts").ConfigureAwait(false);
                        }
                        if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "ui_rmenu_interests", EnumClickType.URLClick).ConfigureAwait(false))
                        {
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
                        if (customizeBufferResult)
                        {
                            return new AlgoritmResult()
                            {
                                ActionResultMessage = EnumActionResult.Success,
                                hasError = false
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex.Message);
            }
            return result;
        }

        public async Task<AlgoritmResult> ListenMusic(Guid WebDriverId)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true,
            };
            try
            {
                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_aud", EnumClickType.URLClick).ConfigureAwait(false))
                {
                    var element = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, "._audio_section_tab__for_you._audio_section_tab__recoms",
                                                                                EnumWebHTMLElementSelector.CSSSelector, ".ui_tab");
                    if (webElementService.ClickToElement(element, EnumClickType.ElementClick))
                    {
                        if (await webDriverService.hasWebHTMLElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".CatalogSection.CatalogSection--divided.CatalogSection__for_you").ConfigureAwait(false))
                        {
                            element = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".CatalogSection.CatalogSection--divided.CatalogSection__for_you",
                                                                      EnumWebHTMLElementSelector.CSSSelector, ".flat_button.primary").ConfigureAwait(false);
                            if (element != null)
                            {
                                if (webElementService.ClickToElement(element, EnumClickType.ElementClick))
                                {
                                    Thread.Sleep(180000);
                                    if (random.Next(1, 10) > 5)
                                        await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "add", EnumClickType.ElementClick).ConfigureAwait(false);
                                    result = new AlgoritmResult()
                                    {
                                        ActionResultMessage = EnumActionResult.Success,
                                        hasError = false,
                                    };
                                }
                            }
                        }
                    }
                }
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
            }
            catch
            (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex.Message);
            }
            return result;
        }

        public async Task<AlgoritmResult> WatchVideo(Guid WebDriverId)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true,
            };
            try
            {
                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_vid", EnumClickType.URLClick).ConfigureAwait(false))
                {
                    var element = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".videocat_autoplay_video_wrap").ConfigureAwait(false);
                    if (element != null)
                    {
                        element = webElementService.GetElementInElement(element, EnumWebHTMLElementSelector.TagName, "a");
                        if (webElementService.ClickToElement(element, EnumClickType.URLClick))
                        {
                            Thread.Sleep(5000);
                            element = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id, "VideoLayerInfo__topControls", EnumWebHTMLElementSelector.TagName, "div");
                            webElementService.ClickToElement(element, EnumClickType.URLClick);
                            result = new AlgoritmResult()
                            {
                                ActionResultMessage = EnumActionResult.Success,
                                hasError = false,
                            };
                        }
                    }
                }
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex.Message);
            }
            return result;
        }

        public async Task<AlgoritmResult> News(Guid WebDriverId)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true,
            };
            try
            {
                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_nwsf", EnumClickType.URLClick).ConfigureAwait(false))
                {
                    var body = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.TagName, "body").ConfigureAwait(false);
                    webElementService.ScrollElement(body);
                    await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                    result.ActionResultMessage = EnumActionResult.Success;
                    result.hasError = false;
                    return result;
                }
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex.Message);
            }
            return result;
        }

        public async Task<AlgoritmResult> AvatarLike(Guid WebDriverId)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            try
            {
                var element = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id, "profile_photo_link",
                                                                            EnumWebHTMLElementSelector.TagName, "img").ConfigureAwait(false);
                var attribute = webElementService.GetAttributeValue(element, "src");
                if ((attribute != null) && (attribute.IndexOf("camera_200", StringComparison.Ordinal) == -1))
                {
                    if (webElementService.ClickToElement(element, EnumClickType.URLClick))
                    {
                        if (await webDriverService.hasWebHTMLElement(WebDriverId, EnumWebHTMLElementSelector.Id, "pv_photo").ConfigureAwait(false))
                        {
                            if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".like_btn.like._like", EnumClickType.ElementClick).ConfigureAwait(false))
                            {
                                result = new AlgoritmResult()
                                {
                                    ActionResultMessage = EnumActionResult.Success,
                                    hasError = false
                                };
                            }
                        }
                    }
                }
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex.Message);
            }
            return result;
        }

        public async Task<AlgoritmResult> NewsLike(Guid WebDriverId, EnumNewsLikeType NewsLikeType)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            try
            {
                var element = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id,
                                                                            "page_wall_posts", EnumWebHTMLElementSelector.CSSSelector, "._post_content").ConfigureAwait(false);
                if (element != null)
                {
                    element = webElementService.GetElementInElement(element, EnumWebHTMLElementSelector.CSSSelector, ".like_btn.like._like");
                    if (webElementService.ClickToElement(element, EnumClickType.ElementClick))
                    {
                        result = new AlgoritmResult()
                        {
                            ActionResultMessage = EnumActionResult.Success,
                            hasError = false
                        };
                    }
                }
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex.Message);
            }
            return result;
        }

        public async Task<AlgoritmResult> Subscribe(Guid WebDriverId)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            try
            {
                var element = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id, "friend_status",
                                                                          EnumWebHTMLElementSelector.TagName, "button").ConfigureAwait(false);
                if (webElementService.ClickToElement(element, EnumClickType.ElementClick))
                {
                    result = new AlgoritmResult()
                    {
                        ActionResultMessage = EnumActionResult.Success,
                        hasError = false
                    };
                }
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex.Message);
            }
            return result;
        }

        public async Task<AlgoritmResult> SubscribeToGroup(Guid WebDriverId)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            try
            {
                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "public_subscribe", EnumClickType.ElementClick).ConfigureAwait(false))
                {
                    result = new AlgoritmResult()
                    {
                        ActionResultMessage = EnumActionResult.Success,
                        hasError = false
                    };
                }
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex.Message);
            }
            return result;
        }

        public async Task<AlgoritmResult> Repost(Guid WebDriverId, EnumRepostType RepostType)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            try
            {
                var element = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id,
                                                                            "page_wall_posts", EnumWebHTMLElementSelector.CSSSelector, "._post_content").ConfigureAwait(false);
                if (element != null)
                {
                    element = webElementService.GetElementInElement(element, EnumWebHTMLElementSelector.CSSSelector, "div[class='_post_content']");
                    if (element != null)
                    {
                        element = webElementService.GetElementInElement(element, EnumWebHTMLElementSelector.CSSSelector, ".like_btn.share._share.empty");
                        if (element == null)
                            element = webElementService.GetElementInElement(element, EnumWebHTMLElementSelector.CSSSelector, ".like_btn.share._share");
                        if (webElementService.ClickToElement(element, EnumClickType.ElementClick))
                        {
                            element = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "box_layer").ConfigureAwait(false);
                            if (await webDriverService.hasWebHTMLElement(WebDriverId, element, EnumWebHTMLElementSelector.CSSSelector, ".popup_box_container").ConfigureAwait(false))
                            {
                                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "like_share_send", EnumClickType.ElementClick))
                                {
                                    result = new AlgoritmResult()
                                    {
                                        ActionResultMessage = EnumActionResult.Success,
                                        hasError = false
                                    };
                                }
                            }
                        }
                    }
                }
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex.Message);
            }
            return result;
        }

        public async Task<AlgoritmResult> SendFirstMessage(Guid WebDriverId, string MessageText)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            try
            {
                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".flat_button.profile_btn_cut_left", EnumClickType.ElementClick).ConfigureAwait(false))
                {
                    if (await webDriverService.hasWebHTMLElement(WebDriverId, EnumWebHTMLElementSelector.Id, "mail_box_editable").ConfigureAwait(false))
                    {
                        var textBlock = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "mail_box_editable").ConfigureAwait(false);
                        var printMessageResult = webElementService.PrintTextToElement(textBlock, MessageText);
                        if ((printMessageResult) && (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "mail_box_send", EnumClickType.ElementClick).ConfigureAwait(false)))
                        {
                            if (await hasCaptcha(WebDriverId).ConfigureAwait(false) == false)
                            {
                                result = new AlgoritmResult()
                                {
                                    ActionResultMessage = EnumActionResult.Success,
                                    hasError = false
                                };
                            }
                        }
                    }
                }
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex.Message);
            }
            return result;
        }

        public async Task<AlgoritmResult> GoToDialog(Guid WebDriverId, string ClientVkId)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            try
            {
                var goToDialogByVkIdResult = await goToDialogByVkId(WebDriverId, ClientVkId).ConfigureAwait(false);
                if (goToDialogByVkIdResult)
                {
                    result = new AlgoritmResult()
                    {
                        ActionResultMessage = EnumActionResult.Success,
                        hasError = false
                    };
                }
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex.Message);
            }
            return result;
        }

        public async Task<DialogWithNewMessagesModel> GetDialogWithNewMessages(Guid WebDriverId)
        {
            try
            {
                var dialogsWithNewMessagesCont = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_msg",
                    EnumWebHTMLElementSelector.CSSSelector, ".inl_bl.left_count").ConfigureAwait(false);
                if (dialogsWithNewMessagesCont != null)
                {
                    var dialogsWithNewMessagesCount = webElementService.GetElementINNERText(dialogsWithNewMessagesCont, true);
                    var parseResult = 0;
                    if ((int.TryParse(dialogsWithNewMessagesCount, out parseResult)) && (parseResult > 0))
                    {
                        if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_msg", EnumClickType.URLClick).ConfigureAwait(false))
                        {
                            var dialogContainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "im_dialogs").ConfigureAwait(false);
                            var dialogsUnreadMarks = webElementService.GetChildElements(dialogContainer, EnumWebHTMLElementSelector.CSSSelector, ".nim-dialog--unread._im_dialog_unread_ct");
                            for (int i = 0; i < dialogsUnreadMarks.Count; i++)
                            {
                                var text = webElementService.GetElementINNERText(dialogsUnreadMarks[i], true);
                                parseResult = 0;
                                if ((int.TryParse(text, out parseResult)) && (parseResult > 0))
                                {
                                    var parent = dialogsUnreadMarks[i];
                                    for (int j = 0; j < 4; j++)
                                    {
                                        if (parent != null)
                                        {
                                            parent = webElementService.GetElementInElement(parent, EnumWebHTMLElementSelector.XPath, "./..");
                                            if (!parent.isAvailable)
                                                parent = null;
                                        }
                                        else
                                            break;
                                    }
                                    if (parent != null)
                                    {
                                        var vkId = webElementService.GetAttributeValue(parent, "data-peer");
                                        if (vkId != null)
                                        {
                                            return new DialogWithNewMessagesModel()
                                            {
                                                ClientVkId = vkId,
                                                MessagesCount = parseResult
                                            };
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex.Message);
            }
            return null;
        }

        public async Task<List<NewMessageModel>> GetNewMessagesInDialog(Guid WebDriverId, string ClientVkId)
        {
            try
            {
                var goToDialogByVkIdResult = await goToDialogByVkId(WebDriverId, ClientVkId).ConfigureAwait(false);
                if (goToDialogByVkIdResult)
                {
                    var messagesCont = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, "._im_peer_history.im-page-chat-contain").ConfigureAwait(false);
                    if (messagesCont != null)
                    {
                        var messages = webElementService.GetChildElements(messagesCont, EnumWebHTMLElementSelector.CSSSelector, ".im-mess-stack._im_mess_stack");
                        if ((messages != null) && (messages.Count > 0))
                        {
                            var messageText = new List<NewMessageModel>();
                            messages.Reverse();
                            for (int i = 0; i < messages.Count; i++)
                            {
                                var senderAttribute = webElementService.GetAttributeValue(messages[i], "data-peer");
                                if ((senderAttribute != null) && (senderAttribute.IndexOf(ClientVkId) != -1))
                                {
                                    var messageCont = webElementService.GetElementInElement(messages[i], EnumWebHTMLElementSelector.CSSSelector, ".im-mess--text.wall_module._im_log_body");
                                    if (messageCont != null)
                                    {
                                        var text = webElementService.GetElementINNERText(messageCont, true);
                                        if (text != null)
                                        {
                                            messageText.Add(new NewMessageModel()
                                            {
                                                AttachedText = "",
                                                ReceiptMessageDatePlatformFormat = "",
                                                Text = text
                                            });
                                        }
                                    }
                                }
                                else
                                    break;
                            }
                            if (messageText.Count > 0)
                                return messageText;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex.Message);
            }
            return new List<NewMessageModel>();
        }

        public async Task<AlgoritmResult> SendAnswerMessage(Guid WebDriverId, string MessageText)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            try
            {
                var inputElement = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.XPath,
                            "/html/body/div[11]/div/div/div[2]/div[2]/div[2]/div/div/div/div/div[1]/div[3]/div[2]/div[4]/div[2]/div[4]/div[1]/div[3]").ConfigureAwait(false);
                if (inputElement != null)
                {
                    webElementService.ClearElement(inputElement);
                    webElementService.PrintTextToElement(inputElement, MessageText);
                    if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector,
                        ".im-send-btn.im-chat-input--send._im_send.im-send-btn_send", EnumClickType.ElementClick).ConfigureAwait(false))
                    {
                        if (!(await hasCaptcha(WebDriverId).ConfigureAwait(false)))
                        {
                            return new AlgoritmResult()
                            {
                                ActionResultMessage = EnumActionResult.Success,
                                hasError = false
                            };
                        }
                    }
                    else if ((webElementService.SendKeyToElement(inputElement, Keys.Return)) && (!(await hasCaptcha(WebDriverId).ConfigureAwait(false))))
                    {
                        return new AlgoritmResult()
                        {
                            ActionResultMessage = EnumActionResult.Success,
                            hasError = false
                        };
                    }
                }
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex.Message);
            }
            return result;
        }

        public async Task<bool> Logout(Guid WebDriverId)
        {
            var result = false;
            try
            {
                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "top_profile_link", EnumClickType.ElementClick))
                {
                    if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "top_profile_mrow", EnumClickType.URLClick))
                        result = true;
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex.Message);
            }
            return result;
        }

        public async Task<string> GetClientName(Guid WebDriverId)
        {
            try
            {
                return await webElementService.GetElementINNERText(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".page_name", true).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex.Message);
            }
            return "";
        }

        private async Task<bool> SaveCustomize(Guid WebDriverId, string BtnParentId)
        {
            try
            {
                if (await CloseModalWindow(WebDriverId).ConfigureAwait(false))
                {
                    var saveBtn = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id, BtnParentId, EnumWebHTMLElementSelector.TagName, "button").ConfigureAwait(false);
                    if ((saveBtn != null) && (webElementService.ClickToElement(saveBtn, EnumClickType.ElementClick)))
                    {
                        if (await CloseModalWindow(WebDriverId).ConfigureAwait(false))
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
                settingsService.AddLog("VkActionService", ex.Message);
            }
            return false;
        }

        private async Task<bool> CloseModalWindow(Guid WebDriverId)
        {
            try
            {
                return await webElementService.SendKeyToElement(WebDriverId, EnumWebHTMLElementSelector.TagName, "body", Keys.Escape).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex.Message);
            }
            return false;
        }

        private async Task<bool> CloseMessageBlockWindow(Guid WebDriverId)
        {
            try
            {
                var element = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "box_layer_wrap").ConfigureAwait(false);
                var attribute = webElementService.GetAttributeValue(element, "style");
                if ((attribute != null) && (attribute.IndexOf("block;", StringComparison.Ordinal) != -1))
                {
                    return await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                }
                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "vkconnect_continue_button", EnumClickType.ElementClick).ConfigureAwait(false))
                    return true;
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex.Message);
            }
            return false;
        }

        private async Task<bool> hasCaptcha(Guid WebDriverId)
        {
            var result = false;
            try
            {
                if (await webDriverService.hasWebHTMLElement(WebDriverId, EnumWebHTMLElementSelector.Id, "validation_skip").ConfigureAwait(false))
                {
                    if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "validation_skip", EnumClickType.ElementClick).ConfigureAwait(false))
                    {
                        await webDriverService.hasWebHTMLElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".popup_box_container").ConfigureAwait(false);
                        result = true;
                    }
                }
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex.Message);
            }
            return result;
        }

        private async Task<bool> goToDialogByVkId(Guid WebDriverId, string VkId)
        {
            var result = false;
            try
            {
                result = await webDriverService.GoToURL(WebDriverId, "im?sel=" + VkId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("VkActionService", ex.Message);
            }
            return result;
        }
    }
}
