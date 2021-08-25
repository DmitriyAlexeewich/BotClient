using BotClient.Bussines.Interfaces;
using BotClient.Models.Bot;
using BotClient.Models.Bot.Enumerators;
using BotClient.Models.Bot.Work;
using BotClient.Models.Bot.Work.Enumerators;
using BotClient.Models.Client;
using BotClient.Models.HTMLElements;
using BotClient.Models.HTMLElements.Enumerators;
using BotDataModels.Bot;
using BotDataModels.Client;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
                            await CloseModalWindow(WebDriverId).ConfigureAwait(false);
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
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<bool> isLoginSuccess(Guid WebDriverId)
        {
            var result = false;
            try
            {
                var settings = settingsService.GetServerSettings();
                settingsService.WaitTime(settings.LoginWaitingTime);
                var checkElement = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "login_blocked_wrap").ConfigureAwait(false);
                result = !webElementService.isElementAvailable(checkElement);
                if (result)
                {
                    checkElement = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "login_reg_button").ConfigureAwait(false);
                    result = !webElementService.isElementAvailable(checkElement);
                    if (result)
                    {
                        checkElement = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".FlatButton.FlatButton--positive.FlatButton--size-l.FlatButton--flexWide").ConfigureAwait(false);
                        result = !webElementService.isElementAvailable(checkElement);
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
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
                await ScrollUp(WebDriverId).ConfigureAwait(false);
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_pr", EnumClickType.URLClick).ConfigureAwait(false))
                {
                    await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                    if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "profile_edit_act", EnumClickType.URLClick).ConfigureAwait(false))
                    {
                        await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                        var customizeBufferResult = true;
                        if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "ui_rmenu_general", EnumClickType.URLClick).ConfigureAwait(false))
                        {
                            await CloseModalWindow(WebDriverId).ConfigureAwait(false);
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
                            await CloseModalWindow(WebDriverId).ConfigureAwait(false);
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
                            await CloseModalWindow(WebDriverId).ConfigureAwait(false);
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
                            await CloseModalWindow(WebDriverId).ConfigureAwait(false);
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
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<AlgoritmResult> GoToMusicPage(Guid WebDriverId)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            try
            {
                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_aud", EnumClickType.URLClick).ConfigureAwait(false))
                {
                    await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                    result.ActionResultMessage = EnumActionResult.Success;
                    result.hasError = false;
                }
                if ((result.hasError) && (await webDriverService.isUrlContains(WebDriverId, "audio").ConfigureAwait(false)))
                {
                    await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                    result = new AlgoritmResult()
                    {
                        ActionResultMessage = EnumActionResult.Success,
                        hasError = false,
                    };
                }
            }
            catch
            (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<BotMusicModel> GetFirstMusic(Guid WebDriverId)
        {
            try
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
                            var currentSongName = await webElementService.GetElementINNERText(WebDriverId, EnumWebHTMLElementSelector.CSSSelector,
                                ".audio_page_player_title_song_title", true).ConfigureAwait(false);
                            if (webElementService.ClickToElement(element, EnumClickType.ElementClick))
                            {
                                var musicLoadingWaitingTime = settingsService.GetServerSettings().MusicLoadingWaitingTimeInMinutes * 60000;
                                var music = await GetMusic(WebDriverId, currentSongName).ConfigureAwait(false);
                                for (int i = 0; i < 60; i++)
                                {
                                    var bufferMusic = await GetMusic(WebDriverId, currentSongName).ConfigureAwait(false);
                                    if ((bufferMusic != music) && (bufferMusic != null))
                                    {
                                        music = bufferMusic;
                                        break;
                                    }
                                    settingsService.WaitTime(1000);
                                    musicLoadingWaitingTime -= 1000;
                                }
                                return music;
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
                await settingsService.AddLog("VkActionService", ex);
            }
            return null;
        }

        public async Task<BotMusicModel> GetNextMusic(Guid WebDriverId)
        {
            try
            {
                var currentSongName = await webElementService.GetElementINNERText(WebDriverId, EnumWebHTMLElementSelector.CSSSelector,
                                    ".audio_page_player_title_song_title", true).ConfigureAwait(false);
                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector,
                    ".audio_page_player_ctrl.audio_page_player_next", EnumClickType.ElementClick).ConfigureAwait(false))
                {
                    var music = await GetMusic(WebDriverId, currentSongName).ConfigureAwait(false);
                    for (int i = 0; i < 60; i++)
                    {
                        if (music != null)
                            break;
                        music = await GetMusic(WebDriverId, currentSongName).ConfigureAwait(false);
                        settingsService.WaitTime(1000);
                    }
                    return music;
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return null;
        }

        public async Task<AlgoritmResult> AddMusic(Guid WebDriverId)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            try
            {
                if (random.Next(1, 10) > 5)
                {
                    var element = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".CatalogSection.CatalogSection--divided.CatalogSection__for_you",
                                                                             EnumWebHTMLElementSelector.CSSSelector, ".flat_button.primary").ConfigureAwait(false);
                    var addBtnAttribute = webElementService.GetAttributeValue(element, "class");
                    if (addBtnAttribute.IndexOf("audio_row__added") == -1)
                        await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "add", EnumClickType.ElementClick).ConfigureAwait(false);
                    result.hasError = false;
                    result.ActionResultMessage = EnumActionResult.Success;
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<AlgoritmResult> StopMusic(Guid WebDriverId)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            try
            {
                await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector,
                    ".top_audio_player_btn.top_audio_player_play._top_audio_player_play", EnumClickType.ElementClick);
                result = new AlgoritmResult()
                {
                    ActionResultMessage = EnumActionResult.Success,
                    hasError = false,
                };
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task PlayAddedMusic(Guid WebDriverId)
        {
            try
            {
                var musics = await webElementService.GetChildElements(WebDriverId, EnumWebHTMLElementSelector.TagName, "Body", 
                                                                      EnumWebHTMLElementSelector.CSSSelector, ".blind_label._audio_row__play_btn").ConfigureAwait(false);
                webElementService.ClickToElement(musics[random.Next(0, musics.Count)], EnumClickType.ElementClick);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
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
                var closeResult = false;
                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_vid", EnumClickType.URLClick).ConfigureAwait(false))
                {
                    await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                    var element = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".videocat_autoplay_video_wrap").ConfigureAwait(false);
                    if (element != null)
                    {
                        element = webElementService.GetElementInElement(element, EnumWebHTMLElementSelector.TagName, "a");
                        if (webElementService.ClickToElement(element, EnumClickType.URLClick))
                        {
                            settingsService.WaitTime(180000 + random.Next(-60000, 60000));
                            element = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id, "VideoLayerInfo__topControls", EnumWebHTMLElementSelector.TagName, "div");
                            closeResult = webElementService.ClickToElement(element, EnumClickType.URLClick);
                            result = new AlgoritmResult()
                            {
                                ActionResultMessage = EnumActionResult.Success,
                                hasError = false,
                            };
                        }
                    }
                }
                if (!closeResult)
                {
                    closeResult = await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                    closeResult = await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                    if (!closeResult)
                        await webDriverService.GoToURL(WebDriverId, "feed").ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<AlgoritmResult> GoToVideoCatalog(Guid WebDriverId)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true,
            };
            try
            {
                var closeResult = false;
                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_vid", EnumClickType.URLClick).ConfigureAwait(false))
                {
                    await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                    result = new AlgoritmResult()
                    {
                        ActionResultMessage = EnumActionResult.Success,
                        hasError = false,
                    };
                }
                if ((result.hasError) && (await webDriverService.isUrlContains(WebDriverId, "video").ConfigureAwait(false)))
                {
                    await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                    result = new AlgoritmResult()
                    {
                        ActionResultMessage = EnumActionResult.Success,
                        hasError = false,
                    };
                }

            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<bool> GoToProfile(Guid WebDriverId, string Link)
        {
            var result = await webDriverService.GoToURL(WebDriverId, Link).ConfigureAwait(false);
            if (result)
            {
                //await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                //await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
            }
            return result;
        }

        public async Task<AlgoritmResult> FindVideo(Guid WebDriverId, string SearchWord)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true,
            };
            try
            {
                var element = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "video_search_input").ConfigureAwait(false);
                if ((element != null) && (webElementService.PrintTextToElement(element, SearchWord)))
                {
                    if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector,
                        ".ui_search_button_search._ui_search_button_search", EnumClickType.URLClick).ConfigureAwait(false))
                    {
                        await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                        result = new AlgoritmResult()
                        {
                            ActionResultMessage = EnumActionResult.Success,
                            hasError = false,
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<List<BotVkVideo>> GetVideos(Guid WebDriverId)
        {
            var result = new List<BotVkVideo>();
            try
            {
                var videosContainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "video_search_global_videos_list").ConfigureAwait(false);
                var waitingVideo = settingsService.GetServerSettings().VideoWaitingTimeInMinutes * 60000;
                while (waitingVideo > 0)
                {
                    var videos = webElementService.GetChildElements(videosContainer, EnumWebHTMLElementSelector.CSSSelector, ".video_item__thumb_link");
                    if ((videos != null) && (videos.Count > 0))
                    {
                        for (int i = 0; i < videos.Count; i++)
                        {
                            if(videos[i] != null)
                            {
                            var attribute = webElementService.GetAttributeValue(videos[i], "href");
                            var innerText = webElementService.GetElementINNERText(videos[i], true);
                                if ((attribute != null) && (innerText != null) && (innerText.IndexOf("YouTube") == -1))
                                {
                                    var botVKVideo = new BotVkVideo()
                                    {
                                        BotVideo = new BotVideoModel()
                                        {
                                            URL = attribute
                                        },
                                        HTMLElement = videos[i]
                                    };
                                    if (result.IndexOf(botVKVideo) == -1)
                                        result.Add(botVKVideo);
                                }
                            }
                        }
                        break;
                    }
                    settingsService.WaitTime(1000);
                    waitingVideo -= 1000;
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<AlgoritmResult> ClickVideo(Guid WebDriverId, WebHTMLElement Element)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true,
            };
            try
            {
                if (webElementService.ClickToElement(Element, EnumClickType.URLClick))
                {
                    result.hasError = false;
                    result.ActionResultMessage = EnumActionResult.Success;
                }

            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<bool> SubscribeByVideo(Guid WebDriver)
        {
            var result = false;
            try
            {
                if (await webElementService.ClickToElement(WebDriver, EnumWebHTMLElementSelector.Id, "mv_subscribe_btn", EnumClickType.ElementClick).ConfigureAwait(false))
                    result = true;
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<bool> AddVideo(Guid WebDriver)
        {
            var result = false;
            try
            {
                if (await webElementService.ClickToElement(WebDriver, EnumWebHTMLElementSelector.Id, "mv_add_button", EnumClickType.ElementClick).ConfigureAwait(false))
                    result = true;
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<AlgoritmResult> CloseVideo(Guid WebDriverId)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true,
            };
            try
            {
                var element = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id, "VideoLayerInfo__topControls", EnumWebHTMLElementSelector.TagName, "div");
                var closeResult = webElementService.ClickToElement(element, EnumClickType.URLClick);
                result = new AlgoritmResult()
                {
                    ActionResultMessage = EnumActionResult.Success,
                    hasError = false,
                };
                if (!closeResult)
                {
                    closeResult = await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                    closeResult = await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                    if (!closeResult)
                        await webDriverService.GoToURL(WebDriverId, "feed").ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<AlgoritmResult> GoToNewsPage(Guid WebDriverId)
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
                    await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                    result = new AlgoritmResult()
                    {
                        ActionResultMessage = EnumActionResult.Success,
                        hasError = false,
                    };
                }
                if ((result.hasError) && (await webDriverService.isUrlContains(WebDriverId, "feed").ConfigureAwait(false)))
                {
                    await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                    result = new AlgoritmResult()
                    {
                        ActionResultMessage = EnumActionResult.Success,
                        hasError = false,
                    };
                }
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }

            return result;
        }

        public async Task<List<BotVkNews>> GetNews(Guid WebDriverId)
        {
            var result = new List<BotVkNews>();
            try
            {
                for(int i=0; i<3; i++)
                    await webElementService.ScrollElement(WebDriverId, EnumWebHTMLElementSelector.TagName, "body").ConfigureAwait(false);
                var newsContainers = await webElementService.GetChildElements(WebDriverId, EnumWebHTMLElementSelector.Id, "feed_rows", EnumWebHTMLElementSelector.CSSSelector, ".feed_row ").ConfigureAwait(false);
                for (int i = 0; i < newsContainers.Count; i++)
                {
                    var dataElement = webElementService.GetElementInElement(newsContainers[i], EnumWebHTMLElementSelector.TagName, "div");
                    var newsId = webElementService.GetAttributeValue(dataElement, "data-post-id");
                    if (newsId != null)
                        result.Add(new BotVkNews()
                                   {
                                       BotNews = new BotNewsModel() { NewsId = newsId },
                                       NewsElement = newsContainers[i]
                                   });
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
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
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
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
                await settingsService.AddLog("VkActionService", ex);
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
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
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
                await settingsService.AddLog("VkActionService", ex);
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
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
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
                await settingsService.AddLog("VkActionService", ex);
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
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "public_subscribe", EnumClickType.ElementClick).ConfigureAwait(false))
                {
                    result = new AlgoritmResult()
                    {
                        ActionResultMessage = EnumActionResult.Success,
                        hasError = false
                    };
                }
                else if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "join_button", EnumClickType.ElementClick).ConfigureAwait(false))
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
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<AlgoritmResult> SubscribeToGroup(Guid WebDriverId, WebHTMLElement GroupElement)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            try
            {
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                var subscribeButton = webElementService.GetElementInElement(GroupElement, EnumWebHTMLElementSelector.CSSSelector, ".flat_button.button_small.button_wide.search_sub_button");
                if (webElementService.ClickToElement(subscribeButton, EnumClickType.ElementClick))
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
                await settingsService.AddLog("VkActionService", ex);
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
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
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
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<AlgoritmResult> SendFirstMessage(Guid WebDriverId, string MessageText, int RoleId, int DialogId, bool? isSecond = false)
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
                        await webDriverService.GetScreenshot(WebDriverId, RoleId, DialogId, DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss")).ConfigureAwait(false);
                        if ((printMessageResult) && (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "mail_box_send", EnumClickType.ElementClick).ConfigureAwait(false)))
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
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<AlgoritmResult> CheckIsSended(Guid WebDriverId, string ClientVkId, int RoleId, int DialogId, bool isMarked)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            try
            {
                var goToDialogResult = await GoToDialog(WebDriverId, ClientVkId).ConfigureAwait(false);
                if ((!goToDialogResult.hasError) && (goToDialogResult.ActionResultMessage == EnumActionResult.Success))
                {
                    if (isMarked)
                    {
                        var jsText = "" +
                        "var elements = document.querySelectorAll('.im-mess._im_mess.im-mess_unread._im_mess_unread.im-mess_out');" +
                        "for (var i = 0; i < elements.length; i++)" +
                        "{ " +
                        "   elements[i].setAttribute('class', 'im-mess _im_mess im-mess_out'); " +
                        "}";
                        await webDriverService.ExecuteJS(WebDriverId, jsText).ConfigureAwait(false);
                    }

                    await webDriverService.GetScreenshot(WebDriverId, RoleId, DialogId, DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss")).ConfigureAwait(false);
                    await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_msg", EnumClickType.URLClick).ConfigureAwait(false);
                    result = new AlgoritmResult()
                    {
                        ActionResultMessage = EnumActionResult.Success,
                        hasError = false
                    };
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
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
                    await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                    result = new AlgoritmResult()
                    {
                        ActionResultMessage = EnumActionResult.Success,
                        hasError = false
                    };
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<List<DialogWithNewMessagesModel>> GetDialogsWithNewMessages(Guid WebDriverId)
        {
            var result = new List<DialogWithNewMessagesModel>();
            try
            {
                var dialogsWithNewMessagesCont = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_msg",
                    EnumWebHTMLElementSelector.CSSSelector, ".inl_bl.left_count").ConfigureAwait(false);
                if (dialogsWithNewMessagesCont != null)
                {
                    var newDialogsCount = await GetNewDialogsCount(WebDriverId).ConfigureAwait(false);
                    if (newDialogsCount > 0)
                    {
                        if ((await webDriverService.isUrlContains(WebDriverId, "im").ConfigureAwait(false)) || (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_msg", EnumClickType.URLClick).ConfigureAwait(false)))
                        {
                            await CloseModalWindow(WebDriverId).ConfigureAwait(false);

                            var dialogContainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "im_dialogs").ConfigureAwait(false);
                            var dialogsLinks = webElementService.GetChildElements(dialogContainer, EnumWebHTMLElementSelector.TagName, "li");
                            for (int i = 0; i < dialogsLinks.Count; i++)
                            {
                                /*
                                if(webElementService.GetElementInElement(dialogsLinks[i], EnumWebHTMLElementSelector.CSSSelector, ".nim-dialog--who") == null)
                                {
                                    var vkId = webElementService.GetAttributeValue(dialogsLinks[i], "data-peer");
                                    var platformLastMessageDateElement = webElementService.GetElementInElement(dialogsLinks[i], EnumWebHTMLElementSelector.CSSSelector, ".nim-dialog--date._im_dialog_date");
                                    var platformLastMessageDate = webElementService.GetElementINNERText(platformLastMessageDateElement, true);
                                    if ((vkId != null) && (platformLastMessageDate != null))
                                    {
                                        result.Add(new DialogWithNewMessagesModel()
                                        {
                                            ClientVkId = vkId,
                                            PlatformLastMessageDate = platformLastMessageDate
                                        });
                                    }
                                }
                                */
                                var unreadMarker = webElementService.GetElementInElement(dialogsLinks[i], EnumWebHTMLElementSelector.CSSSelector, ".nim-dialog--unread._im_dialog_unread_ct");
                                if (unreadMarker != null)
                                {
                                    var innerText = webElementService.GetElementINNERText(unreadMarker, true);
                                    int unreadCount = 0;
                                    if ((innerText != null) && (int.TryParse(innerText, out unreadCount)) && (unreadCount > 0))
                                    {
                                        var vkId = webElementService.GetAttributeValue(dialogsLinks[i], "data-peer");
                                        var platformLastMessageDateElement = webElementService.GetElementInElement(dialogsLinks[i], EnumWebHTMLElementSelector.CSSSelector, ".nim-dialog--date._im_dialog_date");
                                        var platformLastMessageDate = webElementService.GetElementINNERText(platformLastMessageDateElement, true);
                                        result.Add(new DialogWithNewMessagesModel()
                                        {
                                            ClientVkId = vkId,
                                            PlatformLastMessageDate = platformLastMessageDate
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<int> GetNewDialogsCount(Guid WebDriverId)
        {
            var result = 0;
            try
            {
                var dialogsWithNewMessagesCont = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_msg",
                    EnumWebHTMLElementSelector.CSSSelector, ".inl_bl.left_count").ConfigureAwait(false);
                var innerText = webElementService.GetElementINNERText(dialogsWithNewMessagesCont, true);
                if (!int.TryParse(innerText, out result))
                    result = 0;
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task CloseDialog(Guid WebDriverId)
        {
            try
            {
                await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_msg", EnumClickType.URLClick).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
        }

        public async Task<List<NewMessageModel>> GetNewMessagesInDialog(Guid WebDriverId, string ClientVkId)
        {
            try
            {
                var goToDialogByVkIdResult = await goToDialogByVkId(WebDriverId, ClientVkId).ConfigureAwait(false);
                if (goToDialogByVkIdResult)
                {
                    var messages = await GetMessages(WebDriverId).ConfigureAwait(false);
                    if ((messages != null) && (messages.Count > 0))
                    {
                        var messageText = new List<NewMessageModel>();
                        for (int i = 0; i < messages.Count; i++)
                        {
                            var senderAttribute = webElementService.GetAttributeValue(messages[i], "data-peer");
                            if ((senderAttribute != null) && (senderAttribute.IndexOf(ClientVkId) != -1))
                            {
                                var messageCont = webElementService.GetElementInElement(messages[i], EnumWebHTMLElementSelector.CSSSelector, ".im-mess--text.wall_module._im_log_body");
                                if (messageCont != null)
                                {
                                    var text = webElementService.GetElementINNERText(messageCont, true);
                                    var audio = webElementService.GetElementInElement(messageCont, EnumWebHTMLElementSelector.CSSSelector, ".im_msg_audiomsg");
                                    if ((text != null) || (audio != null))
                                    {
                                        messageText.Add(new NewMessageModel()
                                        {
                                            AttachedText = "",
                                            ReceiptMessageDatePlatformFormat = "",
                                            Text = text,
                                            hasAudio = audio == null
                                        });
                                    }
                                }
                            }
                            else
                                break;
                        }
                        if (messageText.Count > 0)
                        {
                            messageText[0].hasChatBlocked = await hasChatBlock(WebDriverId).ConfigureAwait(false);
                        }
                        if (messageText.Count > 0)
                            return messageText;
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return new List<NewMessageModel>();
        }

        public async Task<bool> isBotDialogBlocked(Guid WebDriver)
        {
            try
            {
                if (await webDriverService.hasWebHTMLElement(WebDriver, EnumWebHTMLElementSelector.CSSSelector, "._im_chat_input_error").ConfigureAwait(false))
                    return true;
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return false;
        }

        public async Task<AlgoritmResult> SendAnswerMessage(Guid WebDriverId, string MessageText, string ClientVkId, int BotClientRoleConnectorId)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            try
            {
                /*
                var inputElement = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.XPath,
                            "/html/body/div[11]/div/div/div[2]/div[2]/div[2]/div/div/div/div/div[1]/div[3]/div[2]/div[4]/div[2]/div[4]/div[1]/div[3]").ConfigureAwait(false);
                */
                var inputElement = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, 
                                   ".im_editable.im-chat-input--text._im_text").ConfigureAwait(false);
                var sendResult = false;
                if (inputElement != null)
                {
                    webElementService.ClearElement(inputElement);
                    webElementService.PrintTextToElement(inputElement, MessageText);
                    if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector,
                        ".im-send-btn.im-chat-input--send._im_send.im-send-btn_send", EnumClickType.ElementClick).ConfigureAwait(false))
                        sendResult = true;
                    else if (webElementService.SendKeyToElement(inputElement, Keys.Return))
                        sendResult = true;
                }
                if (sendResult)
                {
                    var messages = await GetMessages(WebDriverId).ConfigureAwait(false);
                    if ((messages != null) && (messages.Count > 0))
                    {
                        var senderAttribute = webElementService.GetAttributeValue(messages[0], "data-peer");
                        if ((senderAttribute != null) && (senderAttribute.IndexOf(ClientVkId) == -1))
                        {
                            result.ActionResultMessage = EnumActionResult.Success;
                            result.hasError = false;
                            return result;
                        }
                    }
                }
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
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
                    if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "top_logout_link", EnumClickType.URLClick))
                        result = true;
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<string> GetClientName(Guid WebDriverId)
        {
            try
            {
                //await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                //await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                return await webElementService.GetElementINNERText(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".page_name", true).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return "";
        }

        public async Task<bool> GetCanRecievedMessage(Guid WebDriverId)
        {
            var result = false;
            try
            {
                result = await webDriverService.hasWebHTMLElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".flat_button.profile_btn_cut_left").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<List<ParsedClientCreateModel>> GetContacts(Guid WebDriverId)
        {
            var result = new List<ParsedClientCreateModel>();
            try
            {
                var goToContactsBtn = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id, "profile_friends", 
                                                                                  EnumWebHTMLElementSelector.CSSSelector, ".module_header").ConfigureAwait(false);
                if (webElementService.ClickToElement(goToContactsBtn, EnumClickType.URLClick))
                {
                    var contanctsContainers = await webElementService.GetChildElements(WebDriverId, EnumWebHTMLElementSelector.Id, "list_content",
                                                                                       EnumWebHTMLElementSelector.CSSSelector, ".friends_list_bl").ConfigureAwait(false);
                    var scrollCount = 20;
                    for (int i = 0; i < scrollCount; i++)
                    {
                        await webElementService.ScrollElement(WebDriverId, EnumWebHTMLElementSelector.TagName, "body").ConfigureAwait(false);
                        var newContanctsContainers = await webElementService.GetChildElements(WebDriverId, EnumWebHTMLElementSelector.Id, "list_content",
                                                                                              EnumWebHTMLElementSelector.CSSSelector, ".friends_list_bl").ConfigureAwait(false);
                        if (newContanctsContainers.Count > contanctsContainers.Count)
                            scrollCount++;
                        contanctsContainers = newContanctsContainers;
                    }

                    for (int i = 0; i < contanctsContainers.Count; i++)
                    {
                        var contacts = webElementService.GetChildElements(contanctsContainers[i], EnumWebHTMLElementSelector.CSSSelector, ".friends_user_row.friends_user_row--fullRow");
                        for (int j = 0; j < contacts.Count; j++)
                        {
                            var parsedClient = new ParsedClientCreateModel();
                            var contactIdElement = webElementService.GetElementInElement(contacts[j], EnumWebHTMLElementSelector.TagName, "div");
                            var contactVkId = webElementService.GetAttributeValue(contactIdElement, "Id");
                            if ((contactVkId != null) && (contactVkId.Length > 0))
                            {
                                parsedClient.VkId = contactVkId.Replace("res", "");
                                if (webElementService.GetElementInElement(contacts[j], EnumWebHTMLElementSelector.CSSSelector, ".friends_field_act") != null)
                                    parsedClient.canRecievedMessage = true;
                            }
                            result.Add(parsedClient);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<AlgoritmResult> SubscribeToGroup(Guid WebDriverId, string GroupURL, string GroupName)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            try
            {
                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_gr", EnumClickType.URLClick).ConfigureAwait(false))
                {
                    if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "ui_rmenu_search", EnumClickType.URLClick).ConfigureAwait(false))
                    {
                        var input = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "search_query").ConfigureAwait(false);
                        if (webElementService.PrintTextToElement(input, GroupName))
                        {
                            var searchResultContainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "results").ConfigureAwait(false);
                            if (searchResultContainer != null)
                            {
                                var searchResult = webElementService.GetElementInElement(searchResultContainer, EnumWebHTMLElementSelector.TagName, "div");
                                for (int i = 0; i < 60; i++)
                                {
                                    searchResult = webElementService.GetElementInElement(searchResultContainer, EnumWebHTMLElementSelector.TagName, "div");
                                    settingsService.WaitTime(1000);
                                    if (searchResult != null)
                                        break;
                                }
                                if (searchResult != null)
                                {
                                    var groups = webElementService.GetChildElements(searchResultContainer, EnumWebHTMLElementSelector.CSSSelector,
                                                 ".groups_row.search_row.clear_fix");
                                    for (int i = 0; i < groups.Count; i++)
                                    {
                                        var groupHref = webElementService.GetElementInElement(groups[i], EnumWebHTMLElementSelector.TagName, "a");
                                        if (groupHref.GetAttributeValue("href").IndexOf(GroupURL) != -1)
                                        {
                                            var btn = webElementService.GetElementInElement(groups[i], EnumWebHTMLElementSelector.CSSSelector, ".flat_button");
                                            if (webElementService.ClickToElement(btn, EnumClickType.ElementClick))
                                            {
                                                result.hasError = false;
                                                result.ActionResultMessage = EnumActionResult.Success;
                                                break;
                                            }
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
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<AlgoritmResult> GoToGroup(Guid WebDriverId, string GroupURL)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            try
            {
                var goToGroupResult = false;
                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_gr", EnumClickType.URLClick).ConfigureAwait(false))
                    goToGroupResult = true;
                if (await webDriverService.isUrlContains(WebDriverId, "groups").ConfigureAwait(false))
                    goToGroupResult = true;
                if (goToGroupResult)
                {
                    var groupsContainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "groups_list_groups").ConfigureAwait(false);
                    var groups = webElementService.GetChildElements(groupsContainer, EnumWebHTMLElementSelector.CSSSelector, ".group_list_row.clear_fix._gl_row");
                    for (int i = 0; i < groups.Count; i++)
                    {
                        var groupHref = webElementService.GetElementInElement(groups[i], EnumWebHTMLElementSelector.CSSSelector, ".group_row_photo");
                        if (groupHref.GetAttributeValue("href").IndexOf(GroupURL) != -1)
                        {
                            var btn = webElementService.GetElementInElement(groups[i], EnumWebHTMLElementSelector.CSSSelector, ".group_row_title");
                            if (webElementService.ClickToElement(btn, EnumClickType.URLClick))
                            {
                                result.hasError = false;
                                result.ActionResultMessage = EnumActionResult.Success;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<AlgoritmResult> GoToGroupsSection(Guid WebDriverId)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            try
            {
                var goToGroupResult = await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_gr", EnumClickType.URLClick).ConfigureAwait(false);
                if ((!goToGroupResult) && (await webDriverService.isUrlContains(WebDriverId, "groups").ConfigureAwait(false)))
                    goToGroupResult = true;
                if (goToGroupResult)
                {
                    result.hasError = false;
                    result.ActionResultMessage = EnumActionResult.Success;
                }

            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<AlgoritmResult> SearchGroups(Guid WebDriverId, string KeyWord, bool FilteredBySubscribersCount, EnumSearchGroupType SearchGroupType, string Country, string City, bool isSaftySearch)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            try
            {
                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "ui_rmenu_search", EnumClickType.ElementClick).ConfigureAwait(false))
                {
                    settingsService.WaitTime(60000);
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
                            settingsService.WaitTime(10000);
                            webElementService.SendKeyToElement(textField, Keys.Return);
                            if (City.Length > 0)
                            {
                                var cityContainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "cCity").ConfigureAwait(false);
                                clickResult = webElementService.ClickToElement(cityContainer, EnumClickType.ElementClick);
                                textField = webElementService.GetElementInElement(cityContainer, EnumWebHTMLElementSelector.CSSSelector, ".selector_input");
                                if (clickResult)
                                {
                                    webElementService.PrintTextToElement(textField, City);
                                    settingsService.WaitTime(10000);
                                    webElementService.SendKeyToElement(textField, Keys.Return);
                                }
                            }
                        }
                    }
                    if (!isSaftySearch)
                        await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "safe_search", EnumClickType.ElementClick).ConfigureAwait(false);
                    result.hasError = false;
                    result.ActionResultMessage = EnumActionResult.Success;
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<List<ParsedGroupModel>> GetGroups(Guid WebDriverId)
        {
            var result = new List<ParsedGroupModel>();
            try
            {
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                var body = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.TagName, "body").ConfigureAwait(false);
                for (int i = 0; i < 100; i++)
                {
                    webElementService.ScrollElement(body);
                    settingsService.WaitTime(1000);
                }
                var groups = new List<WebHTMLElement>();
                if (await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "groups_list_groups").ConfigureAwait(false) != null)
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
                else
                {
                    groups = webElementService.GetChildElements(body, EnumWebHTMLElementSelector.CSSSelector, ".groups_row.search_row.clear_fix");
                    for (int i = 0; i < groups.Count; i++)
                    {
                        var groupName = webElementService.GetElementInElement(groups[i], EnumWebHTMLElementSelector.CSSSelector, ".labeled.title");
                        var groupId = webElementService.GetAttributeValue(groups[i], "data-id");
                        result.Add(new ParsedGroupModel()
                        {
                            GroupName = webElementService.GetElementINNERText(groupName, true),
                            GroupVkId = groupId,
                            GroupElement = groups[i]
                        });
                    }
                }
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<List<PlatformPostModel>> GetPosts(Guid WebDriverId)
        {
            var result = new List<PlatformPostModel>();
            try
            {
                var postsContainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "page_wall_posts").ConfigureAwait(false);
                var posts = webElementService.GetChildElements(postsContainer, EnumWebHTMLElementSelector.CSSSelector, "._post.post");
                for (int i = 0; i < posts.Count; i++)
                {
                    var postId = webElementService.GetAttributeValue(posts[i], "data-post-id");
                    int rating = 0;
                    var ratingContainer = webElementService.GetElementInElement(posts[i], EnumWebHTMLElementSelector.CSSSelector, ".like_btns");
                    var t = webElementService.GetElementINNERText(ratingContainer, true);
                    var ratingComponents = webElementService.GetChildElements(ratingContainer, EnumWebHTMLElementSelector.CSSSelector, ".like_btn");
                    for (int j = 0; j < ratingComponents.Count; j++)
                    {
                        if (webElementService.GetAttributeValue(ratingComponents[j], "class").IndexOf("_like") != -1)
                            rating += await GetRating(ratingComponents[j]).ConfigureAwait(false);
                        if (webElementService.GetAttributeValue(ratingComponents[j], "class").IndexOf("_comment") != -1)
                            rating += await GetRating(ratingComponents[j]).ConfigureAwait(false) * 100;
                        if (webElementService.GetAttributeValue(ratingComponents[j], "class").IndexOf("_share") != -1)
                            rating += await GetRating(ratingComponents[j]).ConfigureAwait(false) * 10;
                    }
                    result.Add(new PlatformPostModel(postId, posts[i], rating));
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<string> GetVkId(Guid WebDriverId)
        {
            var result = "";
            try
            {
                var element = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_pr", EnumWebHTMLElementSelector.TagName, "a").ConfigureAwait(false);
                var id = webElementService.GetAttributeValue(element, "href");
                while(id.IndexOf("/") != -1)
                    id = id.Remove(0, id.IndexOf("/") + 1);
                if (id.IndexOf("id") != -1)
                    id = id.Remove(0, id.IndexOf("id") + 2);
                result = id;
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<AlgoritmResult> GoToSelfPage(Guid WebDriverId)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            try
            {
                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_pr", EnumClickType.URLClick).ConfigureAwait(false))
                {
                    result.hasError = false;
                    result.ActionResultMessage = EnumActionResult.Success;
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<AlgoritmResult> GoToSettings(Guid WebDriverId)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            try
            {
                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "top_profile_link", EnumClickType.ElementClick).ConfigureAwait(false))
                {
                    settingsService.WaitTime(random.Next(100, 500));
                    if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "top_settings_link", EnumClickType.URLClick).ConfigureAwait(false))
                    {
                        result.hasError = false;
                        result.ActionResultMessage = EnumActionResult.Success;
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<AlgoritmResult> ChangePassword(Guid WebDriverId, string OldPassword, string NewPassword)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            try
            {
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
                                {
                                    result.hasError = false;
                                    result.ActionResultMessage = EnumActionResult.Success;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<string> GetPageName(Guid WebDriverId)
        {
            var result = "";
            try
            {
                result = await webElementService.GetElementINNERText(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".page_name", true).ConfigureAwait(false);
                if (result == null)
                    result = "";
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<bool> hasChatBlock(Guid WebDriverId)
        {
            var result = false;
            try
            {
                var blockMessageContainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, "._im_chat_input_error").ConfigureAwait(false);
                if (blockMessageContainer != null)
                {
                    var blockMessage = webElementService.GetElementINNERText(blockMessageContainer, true);
                    if (blockMessage.Length > 5)
                        result = true;
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<List<ParsedGroupModel>> GetClientGroups(Guid WebDriverId, string ClientVkId)
        {
            var result = new List<ParsedGroupModel>();
            try
            {
                var body = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.TagName, "body").ConfigureAwait(false);
                for (int i = 0; i < 100; i++)
                {
                    webElementService.ScrollElement(body);
                    settingsService.WaitTime(1000);
                }
                var groups = webElementService.GetChildElements(body, EnumWebHTMLElementSelector.CSSSelector, ".group_list_row.clear_fix._gl_row");
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
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<bool> GoToClientGroups(Guid WebDriverId, string ClientVkId)
        {
            var result = false;
            try
            {
                var goToGroupsPageresult = await webDriverService.GoToURL(WebDriverId, "groups?id=" + ClientVkId).ConfigureAwait(false);
                if (goToGroupsPageresult)
                    result = true;
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<bool> GoToAudioPageByLink(Guid WebDriverId, string Link)
        {
            var result = false;
            try
            {
                var goToGroupsPageresult = await webDriverService.GoToURL(WebDriverId, "audios" + Link).ConfigureAwait(false);
                if (goToGroupsPageresult)
                    result = true;
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<List<ParsedAudioModel>> ParseAudio(Guid WebDriverId)
        {
            var result = new List<ParsedAudioModel>();
            try
            {
                var body = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.TagName, "body");
                for (int i = 0; i < 10; i++)
                {
                    webElementService.ScrollElement(body);
                    settingsService.WaitTime(1000);
                }
                var audios = webElementService.GetChildElements(body, EnumWebHTMLElementSelector.CSSSelector, ".audio_row_content._audio_row_content");
                for (int i = 0; i < audios.Count; i++)
                {
                    var parent = webElementService.GetElementInElement(audios[i], EnumWebHTMLElementSelector.XPath, "..");
                    if (parent != null)
                    {
                        var nameElement = webElementService.GetElementInElement(parent, EnumWebHTMLElementSelector.CSSSelector, ".audio_row__performer_title");
                        result.Add(new ParsedAudioModel()
                        {
                            AudioCreate = new AudioCreateModel()
                            {
                                Name = webElementService.GetElementINNERText(nameElement, true),
                                VkId = webElementService.GetAttributeValue(parent, "data-full-id")
                            },
                            AudioElement = parent
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<bool> AddAudioToSelfPage(Guid WebDriverId, WebHTMLElement Audio)
        {
            var result = false;
            try
            {
                var playAudioButton = webElementService.GetElementInElement(Audio, EnumWebHTMLElementSelector.CSSSelector, ".blind_label._audio_row__play_btn");
                if (webElementService.ClickToElement(playAudioButton, EnumClickType.ElementClick))
                {
                    settingsService.WaitTime(10000);
                    result = await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "add", EnumClickType.ElementClick).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<bool> GoToDocsPageByLink(Guid WebDriverId, string Link)
        {
            var result = false;
            try
            {
                var goToGroupsPageresult = await webDriverService.GoToURL(WebDriverId, "docs?oid=" + Link).ConfigureAwait(false);
                if (goToGroupsPageresult)
                    result = true;
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<List<DocumentCreateModel>> ParseDocs(Guid WebDriverId)
        {
            var result = new List<DocumentCreateModel>();
            try
            {
                var body = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.TagName, "body");
                for (int i = 0; i < 10; i++)
                {
                    webElementService.ScrollElement(body);
                    settingsService.WaitTime(1000);
                }
                var docContainers = webElementService.GetChildElements(body, EnumWebHTMLElementSelector.CSSSelector, ".docs_item._docs_item");
                for (int i = 0; i < docContainers.Count; i++)
                {
                    var docLinkAndNameElement = webElementService.GetElementInElement(docContainers[i], EnumWebHTMLElementSelector.CSSSelector, ".docs_item_name");
                    var bookName = webElementService.GetElementINNERText(docLinkAndNameElement, true);
                    var fileExtensions = new string[4] {".doc", ".rtf", ".txt", ".docx"};
                    for (int j = 0; j < fileExtensions.Length; j++)
                        bookName = bookName.Replace(fileExtensions[j], "");
                    result.Add(new DocumentCreateModel()
                    {
                        Name = bookName,
                        DownloadLink = webElementService.GetAttributeValue(docLinkAndNameElement, "href")
                    });
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<bool> GoToGroupByVkId(Guid WebDriverId, string VkId)
        {
            var result = false;
            try
            {
                var goToGroupResult = await webDriverService.GoToURL(WebDriverId, "public" + VkId).ConfigureAwait(false);
                result = goToGroupResult;
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public async Task<bool> GoToVideoPageByLink(Guid WebDriverId, string Link)
        {
            var result = false;
            try
            {
                var goToVideosResult = await webDriverService.GoToURL(WebDriverId, "videos" + Link).ConfigureAwait(false);
                result = goToVideosResult;
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<List<ParsedVideoModel>> ParseVideos(Guid WebDriverId)
        {
            var result = new List<ParsedVideoModel>();
            try
            {
                var body = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.TagName, "body");
                for (int i = 0; i < 10; i++)
                {
                    webElementService.ScrollElement(body);
                    settingsService.WaitTime(1000);
                }
                var videos = webElementService.GetChildElements(body, EnumWebHTMLElementSelector.CSSSelector, ".video_item__thumb_link");
                for (int i = 0; i < videos.Count; i++)
                {
                    result.Add(new ParsedVideoModel()
                    {
                        Name = webElementService.GetAttributeValue(videos[i], "aria-label"),
                        VideoElement = videos[i]
                    });
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<bool> AddVideoToSelfPage(WebHTMLElement Video)
        {
            var result = false;
            try
            {
                var addButton = webElementService.GetElementInElement(Video, EnumWebHTMLElementSelector.CSSSelector, ".video_thumb_action_add");
                result = webElementService.ClickToElement(addButton, EnumClickType.ElementClick);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<string> GoToNewsByLink(Guid WebDriverId, string VkLink)
        {
            var result = "";
            try
            {
                if (await webDriverService.GoToURL(WebDriverId, VkLink).ConfigureAwait(false))
                {
                    var newsContainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, "._post").ConfigureAwait(false);
                    for (int i = 0; i < 5; i++)
                    {
                        var sendBtn = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".reply_send_button").ConfigureAwait(false);
                        if (sendBtn != null)
                        {
                            result = webElementService.GetAttributeValue(newsContainer, "data-post-id");
                            break;
                        }
                        settingsService.WaitTime(60000);
                    }
                }
                var errorElement = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".message_page_body").ConfigureAwait(false);
                var errorText = webElementService.GetElementINNERText(errorElement);
                if ((errorText != null) && (errorText.IndexOf("найдена") != -1))
                    result = "";
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<WebHTMLElement> GetNewsPostInput(Guid WebDriverId, string NewsPostVkId)
        {
            WebHTMLElement result = null;
            try
            {
                result = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "reply_field" + NewsPostVkId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<WebHTMLElement> GetNewsPostSendButton(Guid WebDriverId, string NewsPostVkId)
        {
            WebHTMLElement result = null;
            try
            {
                result = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "reply_button" + NewsPostVkId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<List<BotVkNewsPostCommentModel>> GetNewsPostComments(Guid WebDriverId, string NewsPostVkId)
        {
            var result = new List<BotVkNewsPostCommentModel>();
            try
            {
                var comments = await webElementService.GetChildElements(WebDriverId, EnumWebHTMLElementSelector.Id, "replies" + NewsPostVkId, 
                                                                                 EnumWebHTMLElementSelector.CSSSelector, ".reply").ConfigureAwait(false);
                for (int i = 0; i < comments.Count; i++)
                {
                    var commentatorId = webElementService.GetAttributeValue(comments[i], "data-answering-id");
                    if ((commentatorId != null) && (commentatorId.Length > 0))
                    {
                        var commentId = webElementService.GetAttributeValue(comments[i], "id");
                        if ((commentId != null) && (commentId.Length > 0))
                        {
                            result.Add(new BotVkNewsPostCommentModel()
                            {
                                CommentatorVkId = commentatorId,
                                CommentVkId = commentId
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<bool> SendMessageToPostNews(Guid WebDriverId, string VkId, string Text)
        {
            var result = false;
            try
            {
                var likeContainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".like_wrap._like_wall" + VkId).ConfigureAwait(false);
                if (likeContainer != null)
                {
                    webElementService.ScrollToElement(likeContainer);
                    var commentBtn = webElementService.GetElementInElement(likeContainer, EnumWebHTMLElementSelector.CSSSelector, ".PostBottomAction.comment._comment._reply_wrap");
                    if (commentBtn != null)
                    {
                        webElementService.ScrollToElement(commentBtn);
                        if (webElementService.ClickToElement(commentBtn, EnumClickType.ElementClick))
                        {
                            var input = await GetNewsPostInput(WebDriverId, VkId).ConfigureAwait(false);
                            if (input != null)
                            {
                                webElementService.ScrollToElement(input);
                                if (webElementService.PrintTextToElement(input, Text))
                                {
                                    var sendBtn = await GetNewsPostSendButton(WebDriverId, VkId).ConfigureAwait(false);
                                    result = webElementService.ClickToElement(sendBtn, EnumClickType.ElementClick);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<bool> LikePostNews(Guid WebDriverId, string VkId)
        {
            var result = false;
            try
            {
                var likeContainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".like_wrap._like_wall" + VkId).ConfigureAwait(false);
                if (likeContainer != null)
                {
                    webElementService.ScrollToElement(likeContainer);
                    var likeBtn = webElementService.GetElementInElement(likeContainer, EnumWebHTMLElementSelector.CSSSelector, ".PostBottomAction.PostButtonReactions.PostButtonReactions--post");
                    if (likeBtn == null)
                        likeBtn = webElementService.GetElementInElement(likeContainer, EnumWebHTMLElementSelector.CSSSelector, ".like_btn.like");
                    if (likeBtn != null)
                        result = webElementService.ClickToElement(likeBtn, EnumClickType.ElementClick);
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<bool> RepostPostToSelfPage(Guid WebDriverId, string VkId)
        {
            var result = false;
            try
            {
                var likeContainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".like_wrap._like_wall" + VkId).ConfigureAwait(false);
                if (likeContainer != null)
                {
                    webElementService.ScrollToElement(likeContainer);
                    var repostBtn = webElementService.GetElementInElement(likeContainer, EnumWebHTMLElementSelector.CSSSelector, ".PostBottomAction.share._share");
                    if (webElementService.ClickToElement(repostBtn, EnumClickType.ElementClick))
                    {
                        settingsService.WaitTime(10000);
                        var repostContainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "box_layer");
                        repostContainer = webElementService.GetElementInElement(repostContainer, EnumWebHTMLElementSelector.CSSSelector, ".popup_box_container");
                        repostBtn = webElementService.GetElementInElement(repostContainer, EnumWebHTMLElementSelector.CSSSelector, ".radiobtn");
                        settingsService.WaitTime(10000);
                        if (webElementService.ClickToElement(repostBtn, EnumClickType.ElementClick))
                        {
                            repostBtn = webElementService.GetElementInElement(repostContainer, EnumWebHTMLElementSelector.Id, "like_share_send");
                            settingsService.WaitTime(10000);
                            if (webElementService.ClickToElement(repostBtn, EnumClickType.ElementClick))
                                result = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<bool> LikePostNewsComment(Guid WebDriverId, string CommentId)
        {
            var result = false;
            try
            {
                var likeBtn = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.Id, CommentId, EnumWebHTMLElementSelector.CSSSelector, ".like_btn").ConfigureAwait(false);
                result = webElementService.ClickToElement(likeBtn, EnumClickType.ElementClick);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<bool> CreatePostNews(Guid WebDriverId, string Text)
        {
            var result = false;
            try
            {
                if (await webElementService.PrintTextToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "post_field", Text).ConfigureAwait(false))
                    result = await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "send_post", EnumClickType.ElementClick).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        private async Task<int> GetRating(WebHTMLElement Element)
        {
            var result = 0;
            try
            {
                var innerText = webElementService.GetElementINNERText(Element, true);
                if ((innerText != null) && (int.TryParse(Regex.Replace(innerText, "(?:[^0-9]|(?<=['\"])s)", ""), out result)))
                {
                    if (innerText.IndexOf("K") != -1)
                        result *= 1000;
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
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
                await settingsService.AddLog("VkActionService", ex);
            }
            return false;
        }

        private async Task<bool> CloseModalWindow(Guid WebDriverId)
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
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        private async Task<bool> CloseMessageBlockWindow(Guid WebDriverId)
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
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        public async Task<bool> hasCaptcha(Guid WebDriverId)
        {
            var result = false;
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    settingsService.WaitTime(1000);
                    result = await webDriverService.hasWebHTMLElement(WebDriverId, EnumWebHTMLElementSelector.Id, "validation_phone").ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
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
                await settingsService.AddLog("VkActionService", ex);
            }
            return result;
        }

        private async Task<List<WebHTMLElement>> GetMessages(Guid WebDriverId)
        {
            var messagesCont = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, "._im_peer_history.im-page-chat-contain").ConfigureAwait(false);
            if (messagesCont != null)
            {
                var messages = webElementService.GetChildElements(messagesCont, EnumWebHTMLElementSelector.CSSSelector, ".im-mess-stack._im_mess_stack");
                if ((messages != null) && (messages.Count > 0))
                {
                    messages.Reverse();
                    return messages;
                }
            }
            return new List<WebHTMLElement>();
        }

        private async Task<BotMusicModel> GetMusic(Guid WebDriverId, string CurrentSongName)
        {
            BotMusicModel result = null;
            for (int i = 0; i < 60; i++)
            {
                for (int j = 0; j < 60; j++)
                {
                    var nextSongName = await webElementService.GetElementINNERText(WebDriverId, EnumWebHTMLElementSelector.CSSSelector,
                    ".audio_page_player_title_song_title", true).ConfigureAwait(false);
                    if (nextSongName != null)
                    {
                        if ((CurrentSongName == null) || ((CurrentSongName != null) && (CurrentSongName.Length < 1)))
                            break;
                        else if (nextSongName != CurrentSongName)
                            break;
                    }
                    settingsService.WaitTime(1000);
                }
                CurrentSongName = await webElementService.GetElementINNERText(WebDriverId, EnumWebHTMLElementSelector.CSSSelector,
                ".audio_page_player_title_song_title", true).ConfigureAwait(false);
                var currentArtist = "";
                if (CurrentSongName != null)
                {
                    currentArtist = await webElementService.GetElementINNERText(WebDriverId, EnumWebHTMLElementSelector.CSSSelector,
                    ".audio_page_player_title_performer", true).ConfigureAwait(false);
                }
                if ((currentArtist != null) && (CurrentSongName != null) && (currentArtist.Length > 0) && (CurrentSongName.Length > 0))
                {
                    result = new BotMusicModel()
                    {
                        Artist = currentArtist,
                        SongName = CurrentSongName
                    };
                    break;
                }
                settingsService.WaitTime(1000);
            }
            return result;
        }

        private async Task ScrollUp(Guid WebDriverId)
        {
            try
            {
                var body = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.TagName, "body").ConfigureAwait(false);
                for (int i = 0; i < 10; i++)
                {
                    webElementService.SendKeyToElement(body, Keys.PageUp);
                    settingsService.WaitTime(1000);
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
