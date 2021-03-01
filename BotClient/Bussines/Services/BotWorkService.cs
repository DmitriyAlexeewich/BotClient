using BotClient.Bussines.Interfaces;
using BotClient.Models.Bot;
using BotClient.Models.Bot.Enumerators;
using BotClient.Models.Bot.Work;
using BotClient.Models.Bot.Work.Enumerators;
using BotClient.Models.Enumerators;
using BotClient.Models.HTMLWebDriver;
using BotClient.Models.WebReports;
using BotDataModels.Bot.Enumerators;
using BotDataModels.Role;
using BotDataModels.Role.Enumerators;
using BotMySQL.Bussines.Interfaces.Composite;
using BotMySQL.Bussines.Interfaces.MySQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services
{
    public class BotWorkService : IBotWorkService
    {
        private readonly IBotCompositeService botCompositeService;
        private readonly IClientCompositeService clientCompositeService;
        private readonly IMissionCompositeService missionCompositeService;
        private readonly IWebDriverService webDriverService;
        private readonly ISettingsService settingsService;
        private readonly IVkActionService vkActionService;
        private readonly IDialogScreenshotService dialogScreenshotService;
        private readonly IVideoDictionaryService videoDictionaryService;
        private readonly ITextService textService;
        private readonly IRoleServerConnectorService roleServerConnectorService;
        private readonly IPlatformGroupService platformGroupService;

        public BotWorkService(IBotCompositeService BotCompositeService,
                              IClientCompositeService ClientCompositeService,
                              IMissionCompositeService MissionCompositeService,
                              IWebDriverService WebDriverService,
                              ISettingsService SettingsService,
                              IVkActionService VkActionService,
                              IDialogScreenshotService DialogScreenshotService,
                              IVideoDictionaryService VideoDictionaryService,
                              ITextService TextService,
                              IRoleServerConnectorService RoleServerConnectorService,
                              IPlatformGroupService PlatformGroupService)
        {
            botCompositeService = BotCompositeService;
            clientCompositeService = ClientCompositeService;
            missionCompositeService = MissionCompositeService;
            webDriverService = WebDriverService;
            settingsService = SettingsService;
            vkActionService = VkActionService;
            dialogScreenshotService = DialogScreenshotService;
            videoDictionaryService = VideoDictionaryService;
            textService = TextService;
            roleServerConnectorService = RoleServerConnectorService;
            platformGroupService = PlatformGroupService;
        }

        private Random random = new Random();
        private List<BotWorkStatusModel> botsWorkStatus = new List<BotWorkStatusModel>();
        private List<HTMLWebDriver> browsers = new List<HTMLWebDriver>();
        private List<int> missionsId = new List<int>();

        public async Task<bool> StartBot(int ServerId, int BotCount)
        {
            var result = true;
            try
            {
                var bots = botCompositeService.GetBotsByServerId(ServerId, null, BotCount);
                browsers = await webDriverService.GetWebDrivers().ConfigureAwait(false);
                var sessionBotsCount = bots.Count / browsers.Count;
                for (int i = 0; i < browsers.Count; i++)
                {
                    for (int j = 0; j < sessionBotsCount; j++)
                    {
                        var bot = bots[random.Next(0, bots.Count)];
                        botsWorkStatus.Add(new BotWorkStatusModel()
                        {
                            BotData = bot,
                            WebDriverId = browsers[i].Id
                        });
                        bots.Remove(bot);
                    }
                }
                UpdateMissionList(ServerId);
                for (int i = 0; i < browsers.Count; i++)
                {
                    var browserId = browsers[i].Id;
                    Task.Run(() => { RunBot(browserId, ServerId); });
                }
            }
            catch(Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
                result = false;
            }
            return result;
        }

        private async Task RunBot(Guid WebDriverId, int ServerId)
        {
            try
            {
                var dayUpdate = false;
                while (true)
                {
                    var setting = settingsService.GetServerSettings();
                    var bot = botsWorkStatus.FirstOrDefault(item => item.WebDriverId == WebDriverId);
                    if (bot != null)
                    {
                        var currentDay = DateTime.Now;
                        var loginflag = await vkActionService.Login(WebDriverId, bot.BotData.Login, bot.BotData.Password).ConfigureAwait(false);
                        if ((!loginflag.hasError) && (await vkActionService.isLoginSuccess(WebDriverId).ConfigureAwait(false)))
                        {
                            var updateOnlineDateResult = botCompositeService.UpdateOnlineDate(bot.BotData.Id, DateTime.Now);
                            if (updateOnlineDateResult.HasError)
                                await settingsService.AddLog("BotWorkService", updateOnlineDateResult.ExceptionMessage).ConfigureAwait(false);

                            if (bot.BotData.isUpdatedCustomizeInfo)
                            {
                                var cusomizeData = botCompositeService.GetBotCustomizeByBotId(bot.BotData.Id);
                                var cusomizeResult = await vkActionService.Customize(WebDriverId, cusomizeData).ConfigureAwait(false);
                                if (!cusomizeResult.hasError)
                                    botCompositeService.SetIsUpdatedCustomizeInfo(bot.BotData.Id, false);
                            }
                            var botSchedule = new List<EnumBotActionType>();
                            var startTime = new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, 8, 0, 0);
                            var endTime = new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, 23, 0, 0);
                            var maxSecondActionsCount = 0;
                            if ((currentDay > startTime) && (currentDay < endTime))
                            {
                                var maxClientsRoleConnectionActionsCount = random.Next(setting.MinRoleActionCountPerSession, setting.MaxRoleActionCountPerSession);
                                for (int j = 0; j < maxClientsRoleConnectionActionsCount; j++)
                                {
                                    botSchedule.Add(EnumBotActionType.RoleMission);
                                    maxSecondActionsCount++;
                                }
                            }
                            else
                                maxSecondActionsCount = random.Next(setting.MinNightSecondActionCountPerSession, setting.MaxNightSecondActionCountPerSession);
                            for (int j = 0; j < maxSecondActionsCount; j++)
                                botSchedule.Add((EnumBotActionType)random.Next(1, 5));
                            botSchedule = settingsService.ShuffleSchedule(botSchedule);
                            await CheckMessage(WebDriverId, bot.BotData.Id).ConfigureAwait(false);
                            var roleAttept = 0;
                            var roleAtteptMaxCount = random.Next(setting.MinRoleAtteptCount, setting.MaxRoleAtteptCount);
                            for (int j = 0; j < botSchedule.Count; j++)
                            {
                                var isActionError = false;
                                switch (botSchedule[j])
                                {
                                    case EnumBotActionType.ListenMusic:
                                        botCompositeService.CreateBotActionHistory(bot.BotData.Id, botSchedule[j], $"Переход к музыкальному каталогу");
                                        await ListenMusic(WebDriverId, bot.BotData.Id).ConfigureAwait(false);
                                        break;
                                    case EnumBotActionType.WatchVideo:
                                        botCompositeService.CreateBotActionHistory(bot.BotData.Id, botSchedule[j], $"Переход к видеокаталогу");
                                        await WatchVideo(WebDriverId, bot.BotData.Id).ConfigureAwait(false);
                                        break;
                                    case EnumBotActionType.News:
                                        botCompositeService.CreateBotActionHistory(bot.BotData.Id, botSchedule[j], $"Просмотр новостей");
                                        await vkActionService.News(WebDriverId).ConfigureAwait(false);
                                        break;
                                    case EnumBotActionType.Group:
                                        botCompositeService.CreateBotActionHistory(bot.BotData.Id, botSchedule[j], $"Просмотр или подписка на группу");
                                        await SubscribeRepostGroup(WebDriverId, bot.BotData.Id);
                                        break;
                                    case EnumBotActionType.RoleMission:
                                        if (!bot.BotData.isPrintBlock)
                                        {
                                            Thread.Sleep(random.Next(setting.MinRoleWaitingTime, setting.MaxRoleWaitingTime));
                                            var setBotClientsRoleConnection = clientCompositeService.SetClientToBot(bot.BotData.Id, GetMissionId(bot.BotData.Id));
                                            if ((!setBotClientsRoleConnection.HasError) && (setBotClientsRoleConnection.Result != null))
                                            {
                                                var botClientsRoleConnection = setBotClientsRoleConnection.Result;
                                                var roleMissionResult = await ExecuteRoleMission(WebDriverId, botClientsRoleConnection).ConfigureAwait(false);
                                                if (roleMissionResult)
                                                {
                                                    clientCompositeService.SetBotClientRoleConnectionSuccess(botClientsRoleConnection.Id, true);
                                                    roleAttept = 0;
                                                    roleAtteptMaxCount = random.Next(1, 3);
                                                    botSchedule = await SimplifySchedule(botSchedule, j).ConfigureAwait(false);
                                                }
                                                else if (roleAttept < roleAtteptMaxCount)
                                                {
                                                    clientCompositeService.SetBotClientRoleConnectionSuccess(botClientsRoleConnection.Id, false);
                                                    botSchedule.Insert(j + 1, EnumBotActionType.RoleMission);
                                                    botSchedule.Insert(j + 1, (EnumBotActionType)random.Next(1, 5));
                                                    roleAttept++;
                                                }
                                                clientCompositeService.SetBotClientRoleConnectionComplete(botClientsRoleConnection.Id);
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                if (botSchedule[j] != EnumBotActionType.RoleMission)
                                {
                                    roleAttept = 0;
                                    roleAtteptMaxCount = random.Next(1, 4);
                                }
                                if (isActionError)
                                    break;
                                await CheckMessage(WebDriverId, bot.BotData.Id).ConfigureAwait(false);
                                bot.BotData = botCompositeService.GetBotById(bot.BotData.Id);
                            }
                            botCompositeService.CreateBotActionHistory(bot.BotData.Id, EnumBotActionType.RoleMission, $"Выход из профиля");
                        }
                        else
                            botCompositeService.SetIsDead(bot.BotData.Id, true);
                        var logoutresult = await Logout(WebDriverId).ConfigureAwait(false);
                        botCompositeService.SetIsOnline(bot.BotData.Id, false);
                        botsWorkStatus.Remove(bot);
                        botsWorkStatus.Add(bot);
                        UpdateMissionList(ServerId);
                        if ((DateTime.Now.Hour > 6) && (DateTime.Now.Hour < 7) && (!dayUpdate))
                        {
                            for (int i = 0; i < botsWorkStatus.Count; i++)
                                botsWorkStatus[i].RepostCount = 0;
                        /*
                                var deleteDay = DateTime.Now;
                                deleteDay = deleteDay.AddDays(-7);
                                var screenshots = dialogScreenshotService.GetByDateTime(deleteDay);
                                if ((screenshots != null) && (screenshots.Count > 0))
                                {
                                    screenshots = screenshots.Where(item => item.UpdateDate < deleteDay && item.ScreenshotsCount < 2).ToList();
                                    var deleteResult = await settingsService.DeleteScreenshotFolder(screenshots).ConfigureAwait(false);
                                    if (deleteResult)
                                    {
                                        var screenshotsId = screenshots.Select(item => item.Id).ToList();
                                        dialogScreenshotService.SetIsDeleted(screenshotsId);
                                    }
                                }
                        */
                            dayUpdate = true;
                        }
                        if (DateTime.Now.Hour > 7)
                            dayUpdate = false;
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
        }

        private bool OnlyInteger(string Row)
        {
            try
            {
                for (int i = 0; i < Row.Length; i++)
                {
                    if (!Char.IsNumber(Row[i]))
                        return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                settingsService.AddLog("BotWorkService", ex);
            }
            return false;
        }

        private async Task ListenMusic(Guid WebDriverId, int BotId)
        {
            try
            {
                var goToPageResult = await vkActionService.GoToMusicPage(WebDriverId).ConfigureAwait(false);
                if ((!goToPageResult.hasError) && (goToPageResult.ActionResultMessage == EnumActionResult.Success))
                {
                    if (random.Next(0, 100) > 50)
                    {
                        var music = await vkActionService.GetFirstMusic(WebDriverId).ConfigureAwait(false);
                        var randListenAttept = random.Next(10, 20);
                        var hasBotMusic = false;
                        if (music != null)
                            hasBotMusic = botCompositeService.hasBotMusic(BotId, music.Artist, music.SongName);
                        for (int i = 0; i < randListenAttept; i++)
                        {
                            if ((music != null) && (!hasBotMusic))
                                break;
                            else
                                music = await vkActionService.GetNextMusic(WebDriverId).ConfigureAwait(false);
                            if (music != null)
                                hasBotMusic = botCompositeService.hasBotMusic(BotId, music.Artist, music.SongName);
                        }
                        if (music != null)
                        {
                            botCompositeService.CreateBotActionHistory(BotId, EnumBotActionType.ListenMusic, $"Прослушивание {music.SongName} исполнитель {music.Artist}");
                            if (!hasBotMusic)
                            {
                                botCompositeService.CreateBotMusic(BotId, music.Artist, music.SongName);
                                await vkActionService.AddMusic(WebDriverId).ConfigureAwait(false);
                            }
                        }
                    }
                    else
                        await vkActionService.PlayAddedMusic(WebDriverId).ConfigureAwait(false);
                    await vkActionService.StopMusic(WebDriverId).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
        }

        private async Task WatchVideo(Guid WebDriverId, int BotId)
        {
            try
            {
                var goToCatalogResult = await vkActionService.GoToVideoCatalog(WebDriverId).ConfigureAwait(false);
                if ((!goToCatalogResult.hasError) && (goToCatalogResult.ActionResultMessage == EnumActionResult.Success))
                {
                    var botWords = botCompositeService.GetBotVideos(BotId);
                    var maxWordId = videoDictionaryService.GetMaxId();
                    var randomWordId = 0;
                    for (int i = 0; i < 100; i++)
                    {
                        randomWordId = random.Next(0, maxWordId);
                        if (botWords.FirstOrDefault(item => item.WordId == randomWordId) == null)
                            break;
                    }
                    var word = videoDictionaryService.GetAll(randomWordId, 1);
                    if (word.Count > 0)
                    {
                        botCompositeService.CreateBotActionHistory(BotId, EnumBotActionType.ListenMusic,
                                                                                   $"Поиск видео {word[0].Word}");
                        var findVideoResult = await vkActionService.FindVideo(WebDriverId, word[0].Word).ConfigureAwait(false);
                        if ((!findVideoResult.hasError) && (findVideoResult.ActionResultMessage == EnumActionResult.Success))
                        {
                            var videos = await vkActionService.GetVideos(WebDriverId).ConfigureAwait(false);
                            for (int i = 0; i < botWords.Count; i++)
                                videos.RemoveAll(item => item.URL == botWords[i].URL);
                            if (videos.Count > 0)
                            {
                                var randomVideoIndex = random.Next(0, videos.Count);
                                var clickVideoResult = await vkActionService.ClickVideo(WebDriverId, videos[randomVideoIndex].HTMLElement).ConfigureAwait(false);
                                if ((!clickVideoResult.hasError) && (clickVideoResult.ActionResultMessage == EnumActionResult.Success))
                                {
                                    botCompositeService.CreateBotActionHistory(BotId, EnumBotActionType.ListenMusic,
                                                                                   $"Просмотр {videos[randomVideoIndex].URL}");
                                    var stopVideo = await vkActionService.CloseVideo(WebDriverId).ConfigureAwait(false);
                                    botCompositeService.CreateBotVideos(BotId, word[0].Id, videos[randomVideoIndex].URL);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
        }

        private async Task<bool> ExecuteRoleMission(Guid WebDriverId, BotClientRoleConnectorModel BotClientRoleConnector)
        {
            try
            {
                var role = missionCompositeService.GetRoleById(BotClientRoleConnector.RoleId);
                botCompositeService.CreateBotActionHistory(BotClientRoleConnector.BotId, EnumBotActionType.RoleMission, 
                                                               $"Выполнение сценария {role.Id} ({role.Title}, {role.UpdateDate}) " +
                                                               $"с контактёром {BotClientRoleConnector.ClientId}");
                var client = clientCompositeService.GetClientById(BotClientRoleConnector.ClientId);
                if (client != null)
                {
                    var currentMission = missionCompositeService.GetRoleMissionConnections(BotClientRoleConnector.RoleId, true);
                    if ((currentMission != null) && (currentMission.Count > 0))
                    {
                        var mission = missionCompositeService.GetMissionById(currentMission[0].Id);
                        botCompositeService.CreateBotActionHistory(BotClientRoleConnector.BotId, EnumBotActionType.RoleMission, 
                                                                       $"Выполнение миссии {mission.Id} ({mission.Title}, {role.UpdateDate}) " +
                                                                       $"с контактёром {BotClientRoleConnector.ClientId}");
                        var nodes = missionCompositeService.GetNodes(currentMission[0].Id, null, null);
                        if ((nodes != null) && (nodes.Count > 0))
                        {
                            nodes.Sort((a, b) => a.NodeId.CompareTo(b.NodeId));
                            var stepResult = true;
                            int i = 0;
                            for (i = 0; i < nodes.Count; i++)
                            {
                                if (nodes[i].PatternId == -1)
                                {
                                    switch (nodes[i].Type)
                                    {
                                        case EnumMissionActionType.GoToProfile:
                                            botCompositeService.CreateBotActionHistory(BotClientRoleConnector.BotId, EnumBotActionType.RoleMission,
                                                                    $"Переход на страницу контактёра {BotClientRoleConnector.ClientId}");
                                            if (OnlyInteger(client.VkId))
                                                stepResult = await vkActionService.GoToProfile(WebDriverId, "id" + client.VkId).ConfigureAwait(false);
                                            else
                                                stepResult = await vkActionService.GoToProfile(WebDriverId, client.VkId).ConfigureAwait(false);
                                            if (stepResult)
                                            {
                                                client.FullName = await vkActionService.GetClientName(WebDriverId);
                                                botCompositeService.CreateBotActionHistory(BotClientRoleConnector.BotId, EnumBotActionType.RoleMission,
                                                                    $"Успешный переход на страницу контактёра {BotClientRoleConnector.ClientId} ({client.FullName})");
                                                clientCompositeService.UpdateClientData(client);
                                            }
                                            break;
                                        case EnumMissionActionType.GoToGroup:
                                            botCompositeService.CreateBotActionHistory(BotClientRoleConnector.BotId, EnumBotActionType.RoleMission,
                                                                    $"Переход на страницу группы {nodes[i].Text}");
                                            stepResult = await vkActionService.GoToProfile(WebDriverId, nodes[i].Text).ConfigureAwait(false);
                                            if (stepResult)
                                            {
                                                botCompositeService.CreateBotActionHistory(BotClientRoleConnector.BotId, EnumBotActionType.RoleMission,
                                                                    $"Успешный переход на страницу группы {nodes[i].Text}");
                                            }
                                            break;
                                        case EnumMissionActionType.AvatarLike:
                                            botCompositeService.CreateBotActionHistory(BotClientRoleConnector.BotId, EnumBotActionType.RoleMission,
                                                                    $"Лайк аватара");
                                            var avatarResult = await vkActionService.AvatarLike(WebDriverId).ConfigureAwait(false);
                                            stepResult = !avatarResult.hasError;
                                            if (stepResult)
                                            {
                                                botCompositeService.CreateBotActionHistory(BotClientRoleConnector.BotId, EnumBotActionType.RoleMission,
                                                                    $"Успешный лайк аватара");
                                            }
                                            break;
                                        case EnumMissionActionType.NewsLike:
                                            botCompositeService.CreateBotActionHistory(BotClientRoleConnector.BotId, EnumBotActionType.RoleMission,
                                                                   $"Лайк новости");
                                            var newsLikeResult = await vkActionService.NewsLike(WebDriverId, EnumNewsLikeType.LikeFirstNews).ConfigureAwait(false);
                                            stepResult = !newsLikeResult.hasError;
                                            if (stepResult)
                                            {
                                                botCompositeService.CreateBotActionHistory(BotClientRoleConnector.BotId, EnumBotActionType.RoleMission,
                                                                    $"Успешный лайк новости");
                                            }
                                            break;
                                        case EnumMissionActionType.Subscribe:
                                            botCompositeService.CreateBotActionHistory(BotClientRoleConnector.BotId, EnumBotActionType.RoleMission,
                                                                   $"Подписка на контактёра {BotClientRoleConnector.ClientId} ({client.FullName})");
                                            var subscribeResult = await vkActionService.Subscribe(WebDriverId).ConfigureAwait(false);
                                            stepResult = !subscribeResult.hasError;
                                            if (stepResult)
                                            {
                                                botCompositeService.CreateBotActionHistory(BotClientRoleConnector.BotId, EnumBotActionType.RoleMission,
                                                                    $"Успешная подписка на контактёра {BotClientRoleConnector.ClientId} ({client.FullName})");
                                            }
                                            break;
                                        case EnumMissionActionType.SubscribeToGroup:
                                            botCompositeService.CreateBotActionHistory(BotClientRoleConnector.BotId, EnumBotActionType.RoleMission,
                                                                   $"Подписка на группу {nodes[i].Text}");
                                            var subscribeToGroupResult = await vkActionService.SubscribeToGroup(WebDriverId).ConfigureAwait(false);
                                            stepResult = !subscribeToGroupResult.hasError;
                                            if (stepResult)
                                            {
                                                botCompositeService.CreateBotActionHistory(BotClientRoleConnector.BotId, EnumBotActionType.RoleMission,
                                                                    $"Успешная подписка на группу {nodes[i].Text}");
                                            }
                                            break;
                                        case EnumMissionActionType.Repost:
                                            botCompositeService.CreateBotActionHistory(BotClientRoleConnector.BotId, EnumBotActionType.RoleMission,
                                                                   $"Репост");
                                            var repostResult = await vkActionService.Repost(WebDriverId, EnumRepostType.First).ConfigureAwait(false);
                                            stepResult = !repostResult.hasError;
                                            if (stepResult)
                                            {
                                                botCompositeService.CreateBotActionHistory(BotClientRoleConnector.BotId, EnumBotActionType.RoleMission,
                                                                       $"Успешный репост");
                                            }
                                            break;
                                        case EnumMissionActionType.SendMessage:
                                            var newMessage = await textService.RandOriginalMessage(nodes[i].Text).ConfigureAwait(false);
                                            if (newMessage != null)
                                            {
                                                botCompositeService.CreateBotActionHistory(BotClientRoleConnector.BotId, EnumBotActionType.RoleMission,
                                                                       $"Отправка сообщения {newMessage.Text} контактёру {BotClientRoleConnector.ClientId} ({client.FullName})");
                                                string apologies = "";
                                                stepResult = false;
                                                var hasCaptcha = false;
                                                for (int j = 0; j < newMessage.TextParts.Count && hasCaptcha == false; j++)
                                                {
                                                    var sendResult = await vkActionService.SendFirstMessage(WebDriverId, newMessage.TextParts[j].Text, stepResult).ConfigureAwait(false);
                                                    hasCaptcha = await vkActionService.hasCaptcha(WebDriverId).ConfigureAwait(false);
                                                    if ((sendResult.ActionResultMessage == EnumActionResult.Success) && (!sendResult.hasError))
                                                    {
                                                        clientCompositeService.CreateMessage(BotClientRoleConnector.Id, newMessage.TextParts[j].Text);
                                                        stepResult = true;
                                                        if (apologies.Length < 1)
                                                        {
                                                            apologies = await GetApologies(newMessage, j).ConfigureAwait(false);
                                                            if (apologies.Length > 0)
                                                            {
                                                                var sendApologiesResult = await vkActionService.SendFirstMessage(WebDriverId, apologies).ConfigureAwait(false);
                                                                if ((!sendApologiesResult.hasError) && (sendApologiesResult.ActionResultMessage == EnumActionResult.Success))
                                                                    clientCompositeService.CreateMessage(BotClientRoleConnector.Id, apologies);
                                                            }
                                                        }
                                                    }
                                                    else
                                                        break;
                                                }
                                                if (hasCaptcha)
                                                    botCompositeService.SetIsPrintBlock(BotClientRoleConnector.BotId, true);
                                                if (stepResult)
                                                {
                                                    if (!hasCaptcha)
                                                    {
                                                        if (apologies.Length < 1)
                                                        {
                                                            apologies = await GetApologies(newMessage).ConfigureAwait(false);
                                                            if (apologies.Length > 0)
                                                            {
                                                                var sendApologiesResult = await vkActionService.SendFirstMessage(WebDriverId, apologies).ConfigureAwait(false);
                                                                clientCompositeService.CreateMessage(BotClientRoleConnector.Id, apologies);
                                                                if ((!sendApologiesResult.hasError) && (sendApologiesResult.ActionResultMessage == EnumActionResult.Success))
                                                                    clientCompositeService.CreateMessage(BotClientRoleConnector.Id, newMessage.Text);
                                                            }
                                                        }
                                                        botCompositeService.CreateBotActionHistory(BotClientRoleConnector.BotId, EnumBotActionType.RoleMission,
                                                                               $"Успешная отправка сообщения {newMessage.Text} контактёру {BotClientRoleConnector.ClientId} ({client.FullName})");
                                                    }
                                                    else
                                                    {
                                                        botCompositeService.CreateBotActionHistory(BotClientRoleConnector.BotId, EnumBotActionType.RoleMission,
                                                                               $"Боту ");
                                                    }
                                                }
                                            }
                                            else
                                                stepResult = false;
                                            break;
                                        case EnumMissionActionType.End:
                                            stepResult = true;
                                            break;
                                        case EnumMissionActionType.WaitAnswerMessage:
                                            stepResult = true;
                                            break;
                                    }
                                    if ((stepResult) && (nodes[i].isRequired) && (BotClientRoleConnector.MissionId < 0))
                                    {
                                        clientCompositeService.SetBotClientRoleConnectionMissionId(BotClientRoleConnector.Id, nodes[i].MissionId);
                                        BotClientRoleConnector.MissionId = nodes[i].MissionId;
                                    }
                                    if (nodes[i].Path.Length > 0)
                                        nodes[i].Path += ";";
                                    nodes[i].Path += nodes[i].NodeId;
                                    clientCompositeService.SetBotClientRoleConnectionMissionPath(BotClientRoleConnector.Id, nodes[i].Path);
                                    if ((!stepResult) && (nodes[i].isRequired))
                                        return false;
                                }
                                else
                                    break;
                            }
                            return stepResult;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
            return false;
        }

        private async Task<bool> CheckMessage(Guid WebDriverId, int BotId)
        {
            try
            {
                botCompositeService.CreateBotActionHistory(BotId, EnumBotActionType.RoleMission, $"Переход к списку диалогов");
                var dialog = await vkActionService.GetDialogWithNewMessages(WebDriverId).ConfigureAwait(false);
                while (dialog != null)
                {
                    var botClientRoleConnector = clientCompositeService.GetBotClientRoleConnectionByBotClientVkId(BotId, dialog.ClientVkId, true);
                    if (botClientRoleConnector != null)
                    {
                        var readNewMessagesResult = await ReadNewMessages(WebDriverId, botClientRoleConnector.RoleId, botClientRoleConnector, dialog.ClientVkId).ConfigureAwait(false);
                        if (!readNewMessagesResult)
                            clientCompositeService.SetBotClientRoleConnectionSuccess(botClientRoleConnector.Id, false);
                    }
                    dialog = await vkActionService.GetDialogWithNewMessages(WebDriverId).ConfigureAwait(false);
                }
                var botDialogsWithNewBotMessages = clientCompositeService.GetBotClientRoleConnectionWithNewBotMessages(BotId);
                if ((botDialogsWithNewBotMessages != null) && (botDialogsWithNewBotMessages.Count > 0))
                {
                    botCompositeService.CreateBotActionHistory(BotId, EnumBotActionType.RoleMission, $"Ответ на {botDialogsWithNewBotMessages.Count} сообщений");
                    for (int i = 0; i < botDialogsWithNewBotMessages.Count; i++)
                    {
                        var client = clientCompositeService.GetClientById(botDialogsWithNewBotMessages[i].ClientId);
                        if (client != null)
                        {
                            botCompositeService.CreateBotActionHistory(BotId, EnumBotActionType.RoleMission, $"Переход к диалогу с контактёром {client.Id} ({client.FullName})");
                            var goToDialogResult = await vkActionService.GoToDialog(WebDriverId, client.VkId).ConfigureAwait(false);
                            if (!goToDialogResult.hasError)
                            {
                                botCompositeService.CreateBotActionHistory(BotId, EnumBotActionType.RoleMission, 
                                                                               $"Успешный переход к диалогу с контактёром {client.Id} ({client.FullName})");
                                var messages = clientCompositeService.GetMessagesByConnectionId(botDialogsWithNewBotMessages[i].Id);
                                if ((messages != null) && (messages.Count > 0))
                                {
                                    var sendAnswerMessageResult = new AlgoritmResult()
                                    {
                                        ActionResultMessage = EnumActionResult.ElementError,
                                        hasError = true
                                    };
                                    for (int j = 0; j < messages.Count; j++)
                                    {
                                        var newBotMessage = await textService.RandOriginalMessage(messages[j].Text).ConfigureAwait(false);
                                        botCompositeService.CreateBotActionHistory(BotId, EnumBotActionType.RoleMission,
                                                                                       $"Отправка сообщения контактёру {client.Id} ({client.FullName})");
                                        if (newBotMessage != null)
                                        {
                                            var hasCaptcha = false;
                                            string apologies = "";
                                            var sendResult = false;
                                            for (int k = 0; k < newBotMessage.TextParts.Count && hasCaptcha == false; k++)
                                            {
                                                sendAnswerMessageResult = await vkActionService.SendAnswerMessage(WebDriverId, newBotMessage.TextParts[k].Text, 
                                                    client.VkId, botDialogsWithNewBotMessages[i].Id).ConfigureAwait(false);
                                                hasCaptcha = await vkActionService.hasCaptcha(WebDriverId).ConfigureAwait(false);
                                                if (!hasCaptcha)
                                                {
                                                    if ((!sendAnswerMessageResult.hasError) && (sendAnswerMessageResult.ActionResultMessage == EnumActionResult.Success))
                                                    {
                                                        sendResult = true;
                                                        if (apologies.Length < 1)
                                                        {
                                                            apologies = await GetApologies(newBotMessage, k).ConfigureAwait(false);
                                                            if (apologies.Length > 0)
                                                            {
                                                                var sendApologiesResult = await vkActionService.SendAnswerMessage(WebDriverId, apologies,
                                                                                          client.VkId, botDialogsWithNewBotMessages[i].Id).ConfigureAwait(false);
                                                                if ((sendApologiesResult.ActionResultMessage == EnumActionResult.Success) && (!sendApologiesResult.hasError))
                                                                    clientCompositeService.CreateMessage(botDialogsWithNewBotMessages[i].Id, apologies);
                                                            }
                                                        }
                                                    }
                                                    else
                                                        break;
                                                }
                                                else
                                                    break;
                                            }
                                            if (hasCaptcha)
                                                botCompositeService.SetIsPrintBlock(BotId, true);
                                            if (sendResult)
                                            {
                                                if (!hasCaptcha)
                                                {
                                                    if (apologies.Length < 1)
                                                    {
                                                        apologies = await GetApologies(newBotMessage).ConfigureAwait(false);
                                                        if (apologies.Length > 0)
                                                        {
                                                            var sendApologiesResult = await vkActionService.SendAnswerMessage(WebDriverId, apologies,
                                                                                      client.VkId, botDialogsWithNewBotMessages[i].Id).ConfigureAwait(false);
                                                            if ((sendApologiesResult.ActionResultMessage == EnumActionResult.Success) && (!sendApologiesResult.hasError))
                                                                clientCompositeService.CreateMessage(botDialogsWithNewBotMessages[i].Id, apologies);
                                                        }
                                                    }
                                                }
                                            }
                                            await webDriverService.GetScreenshot(WebDriverId, botDialogsWithNewBotMessages[i].Id, DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss")).ConfigureAwait(false);
                                        }
                                    }
                                    if (!sendAnswerMessageResult.hasError)
                                    {
                                        botCompositeService.CreateBotActionHistory(BotId, EnumBotActionType.RoleMission,
                                                                                       $"Успешная отправка сообщения контактёру {client.Id} ({client.FullName})");
                                        clientCompositeService.SetBotClientRoleConnectionHasNewBotMessages(botDialogsWithNewBotMessages[i].Id, false);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
            return true;
        }

        private async Task<bool> ReadNewMessages(Guid WebDriverId, int RoleId, BotClientRoleConnectorModel botClientRoleConnector, string ClientVkId)
        {
            try
            {
                var newMessages = await vkActionService.GetNewMessagesInDialog(WebDriverId, ClientVkId).ConfigureAwait(false);
                if ((newMessages != null) && (newMessages.Count > 0))
                {
                    var newMessageText = "";
                    for (int i = 0; i < newMessages.Count; i++)
                        newMessageText += newMessages[i].Text + " ";
                    newMessageText = newMessageText.Remove(newMessageText.Length - 1);
                    var mission = missionCompositeService.GetMissionById(botClientRoleConnector.MissionId);
                    if (!mission.isQuiz)
                    {
                        var standartPatterns = missionCompositeService.GetStandartPatterns(RoleId);
                        var nonRoleStandartPatterns = missionCompositeService.GetNonRoleStandartPatterns();
                        standartPatterns.AddRange(nonRoleStandartPatterns);
                        for (int i = 0; i < standartPatterns.Count; i++)
                        {
                            if (isRegexMatch(newMessageText, standartPatterns[i].Text, standartPatterns[i].isRegex, standartPatterns[i].isInclude))
                            {
                                var botMessage = await textService.RandOriginalMessage(standartPatterns[i].AnswerText).ConfigureAwait(false);
                                if (botMessage != null)
                                {
                                    var sendResult = await PrintAnswerMessage(WebDriverId, botMessage, botClientRoleConnector.Id, ClientVkId).ConfigureAwait(false);
                                    var saveResult = await SaveNewMessage(WebDriverId, botClientRoleConnector.Id, sendResult, newMessageText, botMessage.Text).ConfigureAwait(false);
                                    return saveResult;
                                }
                                return false;
                            }
                        }
                    }
                    if (!botClientRoleConnector.isScenarioComplete)
                    {
                        var nodes = missionCompositeService.GetNodes(botClientRoleConnector.MissionId, null, botClientRoleConnector.MissionPath);
                        for (int i = 0; i < nodes.Count; i++)
                        {
                            var nodePatterns = missionCompositeService.GetNodePatterns(botClientRoleConnector.MissionId, nodes[i].PatternId);
                            for (int j = 0; j < nodePatterns.Count; j++)
                            {
                                if (isRegexMatch(newMessageText, nodePatterns[j].PatternText, nodePatterns[j].isRegex, nodePatterns[j].isInclude))
                                {
                                    var patternAction = missionCompositeService.GetNodes(botClientRoleConnector.MissionId, nodePatterns[j].NodeId, null);
                                    if ((patternAction != null) && (patternAction.Count > 0) && (patternAction[0].Type == EnumMissionActionType.SendMessage))
                                    {
                                        var botMessage = await textService.RandOriginalMessage(patternAction[0].Text).ConfigureAwait(false);
                                        if (botMessage != null)
                                        {
                                            var sendResult = await PrintAnswerMessage(WebDriverId, botMessage, botClientRoleConnector.Id, ClientVkId).ConfigureAwait(false);
                                            if (sendResult)
                                            {
                                                var saveResult = await SaveNewMessage(WebDriverId, botClientRoleConnector.Id, sendResult, newMessageText, botMessage.Text, nodes[i].NodeId).ConfigureAwait(false);
                                                if (botClientRoleConnector.MissionPath.Length > 0)
                                                    botClientRoleConnector.MissionPath += ";";
                                                botClientRoleConnector.MissionPath += nodes[i].NodeId + ";" + patternAction[0].NodeId;
                                                clientCompositeService.SetBotClientRoleConnectionMissionPath(botClientRoleConnector.Id, botClientRoleConnector.MissionPath);
                                                var nextNode = missionCompositeService.GetNodes(botClientRoleConnector.MissionId, null, botClientRoleConnector.MissionPath);
                                                if ((nextNode != null) && (nextNode.Count > 0) && (nextNode[0].Type == EnumMissionActionType.End))
                                                {
                                                    clientCompositeService.SetBotClientRoleConnectionMissionPath(botClientRoleConnector.Id, botClientRoleConnector.MissionPath + ";" + nextNode[0].NodeId);
                                                    clientCompositeService.SetBotClientRoleConnectionScenarioComplete(botClientRoleConnector.Id, true);
                                                }
                                                return saveResult;
                                            }
                                        }
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                    await webDriverService.GetScreenshot(WebDriverId, botClientRoleConnector.Id, DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss")).ConfigureAwait(false);
                    return await SaveNewMessage(WebDriverId, botClientRoleConnector.Id, false, newMessageText, null).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
            return false;
        }

        private async Task<bool> SaveNewMessage(Guid WebDriverId, int BotClientRoleConnectorId, bool isSendAnswerMessageResultSuccess, string ClientMessage, string BotAnswer, int NodeId = -1)
        {
            var result = false;
            try
            {
                var createMessageResult = clientCompositeService.CreateMessage(BotClientRoleConnectorId, ClientMessage, false, NodeId);
                if ((!createMessageResult.HasError) && (createMessageResult.Result) && (BotAnswer != null))
                {
                    if (isSendAnswerMessageResultSuccess)
                    {
                        createMessageResult = clientCompositeService.CreateMessage(BotClientRoleConnectorId, BotAnswer);
                        if ((!createMessageResult.HasError) && (createMessageResult.Result))
                            result = true;
                    }
                    else
                        result = clientCompositeService.SetBotClientRoleConnectionHasNewMessage(BotClientRoleConnectorId, true);
                }
            }
            catch
            (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }

        private bool isRegexMatch(string NewMessageText, string RegexString, bool isRegex, bool isInclude)
        {
            try
            {
                var regex = new Regex(@RegexString, RegexOptions.IgnoreCase);
                if (!isRegex)
                {
                    var text = textService.TextToRegex(RegexString);
                    if (text != null)
                        regex = new Regex(@text, RegexOptions.IgnoreCase);
                    else
                        regex = null;
                }
                if ((regex != null) && (regex.IsMatch(NewMessageText) == isInclude))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("BotWorkService", ex);
            }
            return false;
        }

        private async Task<bool> Logout(Guid WebDriverId)
        {
            var result = false;
            try
            {
                result = await vkActionService.Logout(WebDriverId).ConfigureAwait(false);
                await webDriverService.Restart(WebDriverId);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }

        private async Task<string> GetApologies(BotMessageText BotMessageText, int? TextPartIndex = null)
        {
            string result = "";
            try
            {
                if (BotMessageText != null)
                {
                    if (TextPartIndex != null)
                    {
                        if (BotMessageText.TextParts[TextPartIndex.Value].hasMissClickError)
                            result = await textService.GetApologies(BotMessageText.TextParts[TextPartIndex.Value]).ConfigureAwait(false);
                        else if (BotMessageText.TextParts[TextPartIndex.Value].hasCaps)
                            result = await textService.GetCapsApologies().ConfigureAwait(false);
                    }
                    else
                    {
                        if (BotMessageText.hasMultiplyMissClickError)
                            result = await textService.GetApologies().ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }

        private async Task<bool> PrintAnswerMessage(Guid WebDriverId, BotMessageText BotMessage, int BotClientRoleConnectionId, string ClientVkId)
        {
            var result = false;
            try
            {
                string apologies = "";
                for (int j = 0; j < BotMessage.TextParts.Count; j++)
                {
                    var sendAnswerResult = await vkActionService.SendAnswerMessage(WebDriverId,
                                                  BotMessage.TextParts[j].Text, ClientVkId, BotClientRoleConnectionId).ConfigureAwait(false);
                    if ((sendAnswerResult.ActionResultMessage == EnumActionResult.Success) && (!sendAnswerResult.hasError))
                        result = true;
                    if ((result) && (!BotMessage.hasMultiplyMissClickError))
                    {
                        apologies = await GetApologies(BotMessage, j).ConfigureAwait(false);
                        if ((random.Next(0, 100) > 90) && (apologies.Length > 0))
                        await vkActionService.SendAnswerMessage(WebDriverId, apologies, ClientVkId, BotClientRoleConnectionId).ConfigureAwait(false);
                    }
                    if (!result)
                        break;
                }
                if (result)
                {
                    if (apologies.Length > 0)
                    {
                        apologies = await GetApologies(BotMessage).ConfigureAwait(false);
                        if (apologies.Length > 0)
                        {
                            var sendApologiesResult = await vkActionService.SendAnswerMessage(WebDriverId, apologies,
                                                      ClientVkId, BotClientRoleConnectionId).ConfigureAwait(false);
                            if ((sendApologiesResult.ActionResultMessage == EnumActionResult.Success) && (!sendApologiesResult.hasError))
                                clientCompositeService.CreateMessage(BotClientRoleConnectionId, apologies);
                        }
                    }
                }
                await webDriverService.GetScreenshot(WebDriverId, BotClientRoleConnectionId, DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss")).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }

        private void UpdateMissionList(int ServerId)
        {
            var roles = roleServerConnectorService.GetByServerId(ServerId);
            for (int i = 0; i < roles.Count; i++)
            {
                var roleMissions = missionCompositeService.GetRoleMissionConnections(roles[i].RoleId, true);
                for (int j = 0; j < roleMissions.Count; j++)
                    missionsId.Add(roleMissions[j].Id);
            }
        }

        private int GetMissionId(int BotId)
        {
            var botWorkStatus = botsWorkStatus.FirstOrDefault(item => item.BotData.Id == BotId);
            if (missionsId.Count > botWorkStatus.BotWorkMissionsStatus.Count)
            {
                for (int i = 0; i < missionsId.Count; i++)
                {
                    var unusedMission = botWorkStatus.BotWorkMissionsStatus.FirstOrDefault(item => item.MissionId == missionsId[i]);
                    if (unusedMission == null)
                    {
                        botWorkStatus.BotWorkMissionsStatus.Add(new BotWorkMissionStatus()
                        {
                            MissionId = missionsId[i],
                            ActionsCount = 0
                        });
                    }
                }
            }
            var botWorkMissionStatus = botWorkStatus.BotWorkMissionsStatus.OrderBy(item => item.ActionsCount).FirstOrDefault();
            botWorkMissionStatus.ActionsCount++;
            return botWorkMissionStatus.MissionId;
        }

        private async Task<bool> SubscribeRepostGroup(Guid WebDriverId, int BotId)
        {
            var result = false;
            try
            {
                var botData = botCompositeService.GetBotById(BotId);
                var botGroups = botCompositeService.GetBotPlatformGroupConnectionsByBotId(BotId);
                var isSubscribe = false;
                if (botGroups.Count > 0)
                {
                    var groups = platformGroupService.GetByPlatformId((int)EnumSocialPlatform.Vk);
                    for (int i = 0; i < botGroups.Count; i++)
                        groups.RemoveAll(item => item.Id == botGroups[i].PlatformGroupId);
                    //groups.RemoveAll(item => !botGroups.Any(item2 => item2.PlatformGroupId == item.Id));
                    if (groups.Count > 0)
                    {
                        var randomGroupId = random.Next(0, groups.Count);
                        var subscribeResult = await vkActionService.SubscribeToGroup(WebDriverId,
                                              groups[randomGroupId].GroupId, groups[randomGroupId].GroupName).ConfigureAwait(false);
                        if ((!subscribeResult.hasError) && (subscribeResult.ActionResultMessage == EnumActionResult.Success))
                        {
                            isSubscribe = true;
                            botCompositeService.CreateBotPlatformGroupConnection(BotId, groups[randomGroupId].Id);
                            botGroups = botCompositeService.GetBotPlatformGroupConnectionsByBotId(BotId);
                        }
                    }
                }
                /*
                if ((!isSubscribe) && (botGroups.Count > 0))
                {
                    botGroups = botGroups.OrderBy(item => item.RepostCount).ToList();
                    var group = platformGroupService.GetById(botGroups[0].PlatformGroupId);
                    if (group != null)
                    {
                        var goToGroupResult = await vkActionService.GoToGroup(WebDriverId, group.GroupId).ConfigureAwait(false);
                        if (goToGroupResult.ActionResultMessage == EnumActionResult.Success)
                        {
                            var posts = await vkActionService.GetPosts(WebDriverId);
                            var repostCount = botsWorkStatus.FirstOrDefault(item => item.BotData.Id == BotId).RepostCount + 1;
                            if (posts.Count > 0)
                            {
                                var isRepost = false;
                                if ((settings.RepostChancePerDay / repostCount) < random.Next(0, 100))
                                    isRepost = true;
                                var watchResult = await vkActionService.WatchPost(WebDriverId, posts[random.Next(0, posts.Count)], isRepost).ConfigureAwait(false);
                                if ((watchResult.ActionResultMessage == EnumActionResult.Success) && (isRepost))
                                {
                                    botCompositeService.AddRepostCount(botGroups[0].Id, botGroups[0].RepostCount + 1);
                                    botsWorkStatus.FirstOrDefault(item => item.BotData.Id == BotId).RepostCount++;
                                }
                                if (watchResult.ActionResultMessage == EnumActionResult.Success)
                                    result = true;
                            }
                        }
                    }
                }
                */
            }
            catch(Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }

        private async Task<List<EnumBotActionType>> SimplifySchedule(List<EnumBotActionType> BotSchedule, int StartIndex)
        {
            var result = BotSchedule;
            try
            {
                for (int i = StartIndex; i < result.Count; i++)
                {
                    if (result[i] == EnumBotActionType.RoleMission)
                        result[i] = (EnumBotActionType)random.Next(1,5);
                    else
                        break;
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }

        public async Task<List<string>> Test(string Text)
        {
            var y = new List<string>();
            var newMessage = await textService.RandOriginalMessage(Text).ConfigureAwait(false);
            for (int i = 0; i < newMessage.TextParts.Count; i++)
            {
                if ((newMessage.TextParts[i].hasMissClickError) || (newMessage.TextParts[i].hasCaps))
                {
                    var apologies = await GetApologies(newMessage, i).ConfigureAwait(false);
                    y.Add(apologies);
                }
            }
            return y;
        }
    }
}