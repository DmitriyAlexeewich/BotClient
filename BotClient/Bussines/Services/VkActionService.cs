﻿using BotClient.Bussines.Interfaces;
using BotClient.Models.Bot;
using BotClient.Models.Bot.Work;
using BotClient.Models.Bot.Work.Enumerators;
using BotClient.Models.Enumerators;
using BotClient.Models.HTMLElements;
using BotClient.Models.HTMLElements.Enumerators;
using BotClient.Models.HTMLWebDriver;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
            return result;
        }

        public async Task<bool> isLoginSuccess(Guid WebDriverId)
        {
            var checkElement = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_pr").ConfigureAwait(false);
            return webElementService.isElementAvailable(checkElement);
        }

        public async Task<AlgoritmResult> Customize(Guid WebDriverId, BotCustomizeModel CustomizeData)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            if (await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false))
            {
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
                            var citySearchTextBox = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id, "pedit_city_row", 
                                                                                                EnumWebHTMLElementSelector.TagName, "input").ConfigureAwait(false);
                            if ((webElementService.ClearElement(citySearchTextBox)) && (webElementService.PrintTextToElement(citySearchTextBox, CustomizeData.City)))
                            {
                                var cityRow = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.XPath, "//*[@id='list_options_container_9']",
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
            return result;
        }

        public async Task<AlgoritmResult> ListenMusic(Guid WebDriverId)
        {
            if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_aud", EnumClickType.URLClick).ConfigureAwait(false))
            {
                var element = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, "._audio_section_tab__for_you._audio_section_tab__recoms", 
                                                                            EnumWebHTMLElementSelector.CSSSelector, ".ui_tab");
                if (webElementService.ClickToElement(element, EnumClickType.ElementClick))
                {
                    if (await webDriverService.hasWebHTMLElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".clear_fix._audio_pl.audio_recoms_audios_block.audio_w_covers").ConfigureAwait(false))
                    {
                        element = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".clear_fix._audio_pl.audio_recoms_audios_block.audio_w_covers",
                                                                  EnumWebHTMLElementSelector.CSSSelector, ".audio_recoms_audios_block_col_left").ConfigureAwait(false);
                        if (element != null)
                        {
                            element = webElementService.GetElementInElement(element, EnumWebHTMLElementSelector.CSSSelector, ".audio_row.audio_row_with_cover._audio_row._audio_row_322676110_456239137.audio_can_add.audio_has_thumb.audio_row2");
                            if (webElementService.ClickToElement(element, EnumClickType.ElementClick))
                            {
                                Thread.Sleep(180000);
                                return new AlgoritmResult()
                                {
                                    ActionResultMessage = EnumActionResult.Success,
                                    hasError = false,
                                };
                            }
                            await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                        }
                        await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                    }
                    await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                }
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
            }
            await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
            return new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true,
            };
        }

        public async Task<AlgoritmResult> WatchVideo(Guid WebDriverId)
        {
            if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_vid", EnumClickType.URLClick).ConfigureAwait(false))
            {
                var element = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "videocat_page_block_trends").ConfigureAwait(false);
                if (element != null)
                {
                    element = webElementService.GetElementInElement(element, EnumWebHTMLElementSelector.CSSSelector, ".videocat_row.videocat_row_trends");
                    if (element != null)
                    {
                        element = webElementService.GetElementInElement(element, EnumWebHTMLElementSelector.CSSSelector, ".videocat_grid_wrap.videocat_items_block.clear_fix");
                        if (element != null)
                        {
                            element = webElementService.GetElementInElement(element, EnumWebHTMLElementSelector.TagName, "div");
                            if (webElementService.ClickToElement(element, EnumClickType.URLClick))
                            {
                                Thread.Sleep(720000);
                                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                                return new AlgoritmResult()
                                {
                                    ActionResultMessage = EnumActionResult.Success,
                                    hasError = false,
                                };
                            }
                            await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                            await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                        }
                        await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                        await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                    }
                    await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                    await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                }
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
            }
            await CloseModalWindow(WebDriverId).ConfigureAwait(false);
            await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
            return new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true,
            };
        }

        public async Task<AlgoritmResult> News(Guid WebDriverId)
        {
            if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_nwsf", EnumClickType.URLClick).ConfigureAwait(false))
            {
                var body = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.TagName, "body").ConfigureAwait(false);
                webElementService.ScrollElement(body);
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                return new AlgoritmResult()
                {
                    ActionResultMessage = EnumActionResult.Success,
                    hasError = false,
                };
            }
            await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
            return new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true,
            };
        }

        public async Task<AlgoritmResult> AvatarLike(Guid WebDriverId)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
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
                        await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                        await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                    }
                    await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                    await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                }
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
            }
            await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
            await CloseModalWindow(WebDriverId).ConfigureAwait(false);
            return result;
        }

        public async Task<AlgoritmResult> NewsLike(Guid WebDriverId, EnumNewsLikeType NewsLikeType)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
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
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
            }
            await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
            await CloseModalWindow(WebDriverId).ConfigureAwait(false);
            return result;
        }

        public async Task<AlgoritmResult> Subscribe(Guid WebDriverId)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
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
            return result;
        }

        public async Task<AlgoritmResult> SubscribeToGroup(Guid WebDriverId)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
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
            return result;
        }

        public async Task<AlgoritmResult> Repost(Guid WebDriverId, EnumRepostType RepostType)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
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
                            await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                            await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                        }
                        await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                        await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                    }
                    await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                    await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                }
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
            }
            await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
            await CloseModalWindow(WebDriverId).ConfigureAwait(false);
            return result;
        }

        public async Task<AlgoritmResult> SendFirstMessage(Guid WebDriverId, string MessageText)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".flat_button.profile_btn_cut_left", EnumClickType.ElementClick).ConfigureAwait(false))
            {
                if (await webDriverService.hasWebHTMLElement(WebDriverId, EnumWebHTMLElementSelector.Id, "mail_box_editable").ConfigureAwait(false))
                {
                    var textBlock = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "mail_box_editable").ConfigureAwait(false);
                    var printMessageResult = false;
                    for (int i = 0; i < MessageText.Length; i++)
                    {
                        printMessageResult = webElementService.PrintTextToElement(textBlock, MessageText[i].ToString());
                        Thread.Sleep(random.Next(100, 500));
                    }
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
                        await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                        await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                    }
                    await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                    await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                }
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
            }
            await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
            await CloseModalWindow(WebDriverId).ConfigureAwait(false);
            return result;
        }

        private async Task<bool> SaveCustomize(Guid WebDriverId, string BtnParentId)
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
            return false;
        }

        private async Task<bool> CloseModalWindow(Guid WebDriverId)
        {
            return await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.TagName, "body", Keys.Home).ConfigureAwait(false);
        }

        private async Task<bool> CloseMessageBlockWindow(Guid WebDriverId)
        {
            var element = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "").ConfigureAwait(false);
            var attribute = webElementService.GetAttributeValue(element, "style");
            if ((attribute != null) && (attribute.IndexOf("block;", StringComparison.Ordinal) != -1))
            {
                return await CloseModalWindow(WebDriverId).ConfigureAwait(false);
            }
            if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "", EnumClickType.ElementClick).ConfigureAwait(false))
                return true;
            return false;
        }

        private async Task<bool> hasCaptcha(Guid WebDriverId)
        {
            if (await webDriverService.hasWebHTMLElement(WebDriverId, EnumWebHTMLElementSelector.Id, "validation_skip").ConfigureAwait(false))
            {
                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "validation_skip", EnumClickType.ElementClick).ConfigureAwait(false))
                {
                    await webDriverService.hasWebHTMLElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".popup_box_container").ConfigureAwait(false);
                    await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                    await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                    return true;
                }
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
            }
            await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
            await CloseModalWindow(WebDriverId).ConfigureAwait(false);
            return false;
        }
    }
}
