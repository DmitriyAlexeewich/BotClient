using BotClient.Bussines.Interfaces;
using BotClient.Models.Bot;
using BotClient.Models.Bot.Work;
using BotClient.Models.Bot.Work.Enumerators;
using BotClient.Models.Client;
using BotClient.Models.HTMLElements;
using BotClient.Models.HTMLElements.Enumerators;
using BotDataModels.Bot;
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
                var checkElement = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "login_blocked_wrap").ConfigureAwait(false);
                result = !webElementService.isElementAvailable(checkElement);
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
                                var cityRow = await webElementService.GetElementInElement(WebDriverId, EnumWebHTMLElementSelector.XPath, @"//*[@id=""list_options_container_9""]",
                                                                                                EnumWebHTMLElementSelector.TagName, "li").ConfigureAwait(false);
                                webElementService.ClickToElement(cityRow, EnumClickType.ElementClick);
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
                                var musicLoadingWaitingTime = settingsService.GetServerSettings().MusicLoadingWaitingTime;
                                var music = await GetMusic(WebDriverId, currentSongName).ConfigureAwait(false);
                                for (int i = 0; i < 60; i++)
                                {
                                    var bufferMusic = await GetMusic(WebDriverId, currentSongName).ConfigureAwait(false);
                                    if ((bufferMusic != music) && (bufferMusic != null))
                                    {
                                        music = bufferMusic;
                                        break;
                                    }
                                    Thread.Sleep(1000);
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
                        Thread.Sleep(1000);
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
                var MusicWaitingDeltaTime = settingsService.GetServerSettings().MusicWaitingDeltaTime;
                Thread.Sleep(settingsService.GetServerSettings().MusicWaitingTime + random.Next(-MusicWaitingDeltaTime, MusicWaitingDeltaTime));
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
                var musics = await webElementService.GetChildElements(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".blind_label._audio_row__play_btn").ConfigureAwait(false);
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
                            Thread.Sleep(180000 + random.Next(-60000, 60000));
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
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
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
                var waitingVideo = settingsService.GetServerSettings().VideoWaitingTime;
                while (waitingVideo > 0)
                {
                    var videos = webElementService.GetChildElements(videosContainer, EnumWebHTMLElementSelector.CSSSelector, ".video_item__thumb_link");
                    if ((videos != null) && (videos.Count > 0))
                    {
                        for (int i = 0; i < videos.Count; i++)
                        {
                            var attribute = webElementService.GetAttributeValue(videos[i], "href");
                            var innerText = webElementService.GetElementINNERText(videos[i], true);
                            if ((attribute != null) && (innerText.IndexOf("YouTube") == -1))
                            {
                                var botVKVideo = new BotVkVideo()
                                {
                                    URL = attribute,
                                    HTMLElement = videos[i]
                                };
                                if (result.IndexOf(botVKVideo) == -1)
                                    result.Add(botVKVideo);
                            }
                        }
                        break;
                    }
                    Thread.Sleep(1000);
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

        public async Task<AlgoritmResult> CloseVideo(Guid WebDriverId)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true,
            };
            try
            {
                var videoWaitingDeltaTime = settingsService.GetServerSettings().VideoWaitingDeltaTime;
                Thread.Sleep(settingsService.GetServerSettings().VideoWaitingTime + random.Next(-videoWaitingDeltaTime, videoWaitingDeltaTime));
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
                    await CloseModalWindow(WebDriverId).ConfigureAwait(false);
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

        public async Task<AlgoritmResult> RepostPostToSelfPage(Guid WebDriverId, WebHTMLElement Post)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            try
            {
                var ratingContainer = webElementService.GetElementInElement(Post, EnumWebHTMLElementSelector.CSSSelector, ".like_btns");
                var repostBtn = webElementService.GetElementInElement(ratingContainer, EnumWebHTMLElementSelector.CSSSelector, ".like_btn.share._share");
                if (webElementService.ClickToElement(repostBtn, EnumClickType.ElementClick))
                {
                    var repostContainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "box_layer");
                    repostContainer = webElementService.GetElementInElement(repostContainer, EnumWebHTMLElementSelector.CSSSelector, ".popup_box_container");
                    repostBtn = webElementService.GetElementInElement(repostContainer, EnumWebHTMLElementSelector.CSSSelector, ".radiobtn");
                    if (webElementService.ClickToElement(repostBtn, EnumClickType.ElementClick))
                    {
                        repostBtn = webElementService.GetElementInElement(repostContainer, EnumWebHTMLElementSelector.Id, "like_share_send");
                        if (webElementService.ClickToElement(repostBtn, EnumClickType.ElementClick))
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

        public async Task<AlgoritmResult> SendFirstMessage(Guid WebDriverId, string MessageText, bool? isSecond = false)
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
                        {/*
                            if ((isSecond.Value) || (await hasCaptcha(WebDriverId, true).ConfigureAwait(false) == false))
                            {
                                result = new AlgoritmResult()
                                {
                                    ActionResultMessage = EnumActionResult.Success,
                                    hasError = false
                                };
                            }
                            */
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
                            await CloseModalWindow(WebDriverId).ConfigureAwait(false);
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
                await settingsService.AddLog("VkActionService", ex);
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
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return new List<NewMessageModel>();
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
                    await webDriverService.GetScreenshot(WebDriverId, BotClientRoleConnectorId, DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss")).ConfigureAwait(false);
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
                await CloseModalWindow(WebDriverId).ConfigureAwait(false);
                await CloseMessageBlockWindow(WebDriverId).ConfigureAwait(false);
                return await webElementService.GetElementINNERText(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".page_name", true).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("VkActionService", ex);
            }
            return "";
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
                    var input = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "groups_list_search").ConfigureAwait(false);
                    if (webElementService.PrintTextToElement(input, GroupName))
                    {
                        var searchResultContainer = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.Id, "groups_list_search_cont").ConfigureAwait(false);
                        if (searchResultContainer != null)
                        {
                            var searchResult = webElementService.GetElementInElement(searchResultContainer, EnumWebHTMLElementSelector.TagName, "div");
                            for (int i = 0; i < 60; i++)
                            {
                                searchResult = webElementService.GetElementInElement(searchResultContainer, EnumWebHTMLElementSelector.TagName, "div");
                                Thread.Sleep(1000);
                            }
                            if (searchResult != null)
                            {
                                var groups = webElementService.GetChildElements(searchResultContainer, EnumWebHTMLElementSelector.CSSSelector,
                                             ".group_list_row.clear_fix._gl_row");
                                for (int i = 0; i < groups.Count; i++)
                                {
                                    var groupHref = webElementService.GetElementInElement(groups[i], EnumWebHTMLElementSelector.CSSSelector, ".group_row_photo");
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
                if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "l_gr", EnumClickType.URLClick).ConfigureAwait(false))
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

        public async Task<AlgoritmResult> WatchPost(Guid WebDriverId, PlatformPostModel PlatformPost, bool isRepost)
        {
            var result = new AlgoritmResult()
            {
                ActionResultMessage = EnumActionResult.ElementError,
                hasError = true
            };
            try
            {
                var postContent = webElementService.GetElementInElement(PlatformPost.Element, EnumWebHTMLElementSelector.CSSSelector, ".wall_text");
                var postActiveContent = webElementService.GetElementInElement(postContent, EnumWebHTMLElementSelector.TagName, "div");
                if (webElementService.ClickToElement(postActiveContent, EnumClickType.URLClick))
                {
                    result.hasError = false;
                    result.ActionResultMessage = EnumActionResult.Success;
                    if (isRepost)
                    {
                        await webElementService.SendKeyToElement(WebDriverId, EnumWebHTMLElementSelector.TagName, "body", Keys.Escape).ConfigureAwait(false);
                        var repostResult = await RepostPostToSelfPage(WebDriverId, PlatformPost.Element).ConfigureAwait(false);
                        if ((repostResult.hasError) || (repostResult.ActionResultMessage != EnumActionResult.Success))
                        {
                            result.hasError = true;
                            result.ActionResultMessage = EnumActionResult.ElementError;
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
                if (await webDriverService.hasWebHTMLElement(WebDriverId, EnumWebHTMLElementSelector.Id, "validation_skip").ConfigureAwait(false))
                {
                    if (await webElementService.ClickToElement(WebDriverId, EnumWebHTMLElementSelector.Id, "validation_skip", EnumClickType.ElementClick).ConfigureAwait(false))
                    {
                        await webDriverService.hasWebHTMLElement(WebDriverId, EnumWebHTMLElementSelector.CSSSelector, ".popup_box_container").ConfigureAwait(false);
                        result = true;
                    }
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
                    Thread.Sleep(1000);
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
                Thread.Sleep(1000);
            }
            return result;
        }
        
    }
}
