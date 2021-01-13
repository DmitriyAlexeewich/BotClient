using BotClient.Bussines.Interfaces;
using BotClient.Models.Bot;
using BotClient.Models.Bot.Enumerators;
using BotClient.Models.Bot.Work;
using BotClient.Models.Bot.Work.Enumerators;
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


        public BotWorkService(IBotCompositeService BotCompositeService,
                              IClientCompositeService ClientCompositeService,
                              IMissionCompositeService MissionCompositeService,
                              IWebDriverService WebDriverService,
                              ISettingsService SettingsService,
                              IVkActionService VkActionService,
                              IDialogScreenshotService DialogScreenshotService,
                              IVideoDictionaryService VideoDictionaryService,
                              ITextService TextService,
                              IRoleServerConnectorService RoleServerConnectorService)
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
        }

        private Random random = new Random();
        private List<BotRoleActionsDaySchedule> botsRoleActions = new List<BotRoleActionsDaySchedule>();
        private List<string> randomMessages = new List<string>();
        private List<BotWorkStatusModel> botsWorkStatus = new List<BotWorkStatusModel>();
        private List<HTMLWebDriver> browsers = new List<HTMLWebDriver>();
        private List<MissionModel> missions = new List<MissionModel>();

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

        public async Task<List<BotRoleActionsDaySchedule>> GetBotRoleActions()
        {
            return botsRoleActions;
        }

        public async Task<List<string>> GetRandomMessages()
        {
            return randomMessages;
        }

        private async Task RunBot(Guid WebDriverId, int ServerId)
        {
            try
            {
                while (true)
                {
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
                            var nowTime = DateTime.Now;
                            var startTime = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, 8, 0, 0);
                            var endTime = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, 23, 0, 0);
                            var maxSecondActionsCount = random.Next(1, 4);
                            var botRoleActions = botsRoleActions.FirstOrDefault(item => item.BotId == bot.BotData.Id);
                            if (botRoleActions == null)
                            {
                                botsRoleActions.Add(new BotRoleActionsDaySchedule()
                                {
                                    BotId = bot.BotData.Id,
                                    RoleActionCount = random.Next(8, 13)
                                });
                                botRoleActions = botsRoleActions.FirstOrDefault(item => item.BotId == bot.BotData.Id);
                            }
                            if ((nowTime > startTime) && (nowTime < endTime))
                            {
                                if (botRoleActions.RoleActionCount > 0)
                                {
                                    var maxClientsRoleConnectionActionsCount = random.Next(1, 2);
                                    botRoleActions.RoleActionCount -= maxClientsRoleConnectionActionsCount;
                                    botsRoleActions[botsRoleActions.IndexOf(botRoleActions)] = botRoleActions;
                                    for (int j = 0; j < maxClientsRoleConnectionActionsCount; j++)
                                        botSchedule.Add(EnumBotActionType.RoleMission);
                                }
                            }
                            else
                                maxSecondActionsCount = random.Next(10, 20);
                            for (int j = 0; j < maxSecondActionsCount; j++)
                                botSchedule.Add((EnumBotActionType)random.Next(1, 4));
                            botSchedule = Shuffle(botSchedule).ToList();
                            for (int j = 0; j < botSchedule.Count; j++)
                            {
                                if ((j > 0) && (botSchedule[j] != EnumBotActionType.RoleMission))
                                {
                                    while (botSchedule[j] == botSchedule[j - 1])
                                        botSchedule[j] = (EnumBotActionType)random.Next(1, 4);
                                }
                            }
                            await CheckMessage(WebDriverId, bot.BotData.Id).ConfigureAwait(false);
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
                                    case EnumBotActionType.RoleMission:

                                        var setBotClientsRoleConnection = clientCompositeService.SetClientToBot(bot.BotData.Id, GetMissionId(bot.BotData.Id));
                                        if ((!setBotClientsRoleConnection.HasError) && (setBotClientsRoleConnection.Result != null))
                                        {
                                            var botClientsRoleConnection = setBotClientsRoleConnection.Result;
                                            var roleMissionResult = await ExecuteRoleMission(WebDriverId, botClientsRoleConnection).ConfigureAwait(false);
                                            if (roleMissionResult)
                                                clientCompositeService.SetBotClientRoleConnectionSuccess(botClientsRoleConnection.Id, true);
                                            else
                                                clientCompositeService.SetBotClientRoleConnectionSuccess(botClientsRoleConnection.Id, false);
                                            clientCompositeService.SetBotClientRoleConnectionComplete(botClientsRoleConnection.Id);
                                        }
                                        break;
                                }
                                if (isActionError)
                                    break;
                                await CheckMessage(WebDriverId, bot.BotData.Id).ConfigureAwait(false);
                            }
                            botCompositeService.CreateBotActionHistory(bot.BotData.Id, EnumBotActionType.RoleMission, $"Выход из профиля");
                        }
                        else
                            botCompositeService.SetIsDead(bot.BotData.Id, true);
                        var logoutresult = await Logout(WebDriverId).ConfigureAwait(false);
                        botCompositeService.SetIsOnline(bot.BotData.Id, false);
                        botsWorkStatus.Remove(bot);
                        if (browsers.Count > 1)
                        {
                            while (true)
                            {
                                int randomBrowserIndex = random.Next(0, browsers.Count);
                                if (browsers[randomBrowserIndex].Id != bot.WebDriverId)
                                {
                                    bot.WebDriverId = browsers[randomBrowserIndex].Id;
                                    for (int i = 0; i < browsers.Count; i++)
                                    {
                                        if (botsWorkStatus.FirstOrDefault(item => item.WebDriverId == browsers[i].Id) == null)
                                        {
                                            bot.WebDriverId = browsers[i].Id;
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        botsWorkStatus.Add(bot);
                        if (currentDay > DateTime.Now)
                        {
                            currentDay = DateTime.Now;
                            botsRoleActions = new List<BotRoleActionsDaySchedule>();
                            randomMessages = new List<string>();
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
                        }
                        UpdateMissionList(ServerId);
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
        }

        private IList<T> Shuffle<T>(IList<T> list)
        {
            try
            {
                int n = list.Count;
                while (n > 1)
                {
                    n--;
                    int k = random.Next(n + 1);
                    T value = list[k];
                    list[k] = list[n];
                    list[n] = value;
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("BotWorkService", ex);
            }
            return list;
        }

        private IList<T> Split<T>(IList<T> list, int Index)
        {
            List<T> previous = new List<T>();
            for (int i = 0; i < list.Count; i++)
            {
                if (i <= Index)
                    previous.Add(list[i]);
                else
                    break;
            }
            return previous;
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
                    Task.Run(() => { vkActionService.StopMusic(WebDriverId).ConfigureAwait(false); });
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
                                                                       $"Отправка сообщения {newMessage} контактёру {BotClientRoleConnector.ClientId} ({client.FullName})");
                                                string apologies = null;
                                                stepResult = false;
                                                for (int j = 0; j < newMessage.TextParts.Count; j++)
                                                {
                                                    var sendResult = await vkActionService.SendFirstMessage(WebDriverId, newMessage.TextParts[j].Text).ConfigureAwait(false);
                                                    clientCompositeService.CreateMessage(BotClientRoleConnector.Id, newMessage.TextParts[j].Text);
                                                    if ((sendResult.ActionResultMessage == EnumActionResult.Success) && (!sendResult.hasError))
                                                    {
                                                        stepResult = true;
                                                        if (apologies != null)
                                                        {
                                                            apologies = await GetApologies(newMessage, j).ConfigureAwait(false);
                                                            if (apologies != null)
                                                            {
                                                                var sendApologiesResult = await vkActionService.SendFirstMessage(WebDriverId, apologies).ConfigureAwait(false);
                                                                if((!sendApologiesResult.hasError) && (sendApologiesResult.ActionResultMessage == EnumActionResult.Success))
                                                                    clientCompositeService.CreateMessage(BotClientRoleConnector.Id, apologies);
                                                            }
                                                        }
                                                    }
                                                    else
                                                        break;
                                                }
                                                if (stepResult)
                                                {
                                                    if (apologies == null)
                                                    {
                                                        apologies = await GetApologies(newMessage).ConfigureAwait(false);
                                                        if (apologies != null)
                                                        {
                                                            var sendApologiesResult = await vkActionService.SendFirstMessage(WebDriverId, apologies).ConfigureAwait(false);
                                                            clientCompositeService.CreateMessage(BotClientRoleConnector.Id, apologies);
                                                            if ((!sendApologiesResult.hasError) && (sendApologiesResult.ActionResultMessage == EnumActionResult.Success))
                                                                clientCompositeService.CreateMessage(BotClientRoleConnector.Id, newMessage.Text);
                                                        }
                                                    }
                                                    botCompositeService.CreateBotActionHistory(BotClientRoleConnector.BotId, EnumBotActionType.RoleMission,
                                                                           $"Успешная отправка сообщения {newMessage} контактёру {BotClientRoleConnector.ClientId} ({client.FullName})");
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
            catch
            (Exception ex)
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
                                            string apologies = null;
                                            var sendResult = false;
                                            for (int k = 0; k < newBotMessage.TextParts.Count; k++)
                                            {
                                                sendAnswerMessageResult = await vkActionService.SendAnswerMessage(WebDriverId, newBotMessage.TextParts[k].Text, 
                                                    client.VkId, botDialogsWithNewBotMessages[i].Id).ConfigureAwait(false);

                                                if ((!sendAnswerMessageResult.hasError) && (sendAnswerMessageResult.ActionResultMessage == EnumActionResult.Success))
                                                {
                                                    sendResult = true;
                                                    if (apologies != null)
                                                    {
                                                        apologies = await GetApologies(newBotMessage, k).ConfigureAwait(false);
                                                        if (apologies != null)
                                                        {
                                                            var sendApologiesResult = await vkActionService.SendAnswerMessage(WebDriverId, apologies,
                                                                                      client.VkId, botDialogsWithNewBotMessages[i].Id).ConfigureAwait(false);
                                                            if((sendApologiesResult.ActionResultMessage == EnumActionResult.Success) && (!sendApologiesResult.hasError))
                                                                clientCompositeService.CreateMessage(botDialogsWithNewBotMessages[i].Id, apologies);
                                                        }
                                                    }
                                                }
                                                else
                                                    break;
                                            }
                                            if (sendResult)
                                            {
                                                if (apologies != null)
                                                {
                                                    apologies = await GetApologies(newBotMessage).ConfigureAwait(false);
                                                    if (apologies != null)
                                                    {
                                                        var sendApologiesResult = await vkActionService.SendAnswerMessage(WebDriverId, apologies,
                                                                                  client.VkId, botDialogsWithNewBotMessages[i].Id).ConfigureAwait(false);
                                                        if ((sendApologiesResult.ActionResultMessage == EnumActionResult.Success) && (!sendApologiesResult.hasError))
                                                            clientCompositeService.CreateMessage(botDialogsWithNewBotMessages[i].Id, apologies);
                                                    }
                                                }
                                            }
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
                    var standartPatterns = missionCompositeService.GetStandartPatterns(RoleId);
                    for (int i = 0; i < standartPatterns.Count; i++)
                    {
                        if (isRegexMatch(newMessageText, standartPatterns[i].Text, standartPatterns[i].isRegex, standartPatterns[i].isInclude))
                        {
                            var botMessage = await textService.RandOriginalMessage(standartPatterns[i].AnswerText).ConfigureAwait(false);
                            if (botMessage != null)
                            {
                                string apologies = null;
                                var sendResult = await PrintAnswerMessage(WebDriverId, botMessage, botClientRoleConnector.Id, ClientVkId).ConfigureAwait(false);
                                var saveResult = await SaveNewMessage(WebDriverId, botClientRoleConnector.Id, sendResult, newMessageText, botMessage.Text).ConfigureAwait(false);
                                return saveResult;
                            }
                            return false;
                        }
                    }
                    if (!botClientRoleConnector.isScenarioComplete)
                    {
                        var nodes = missionCompositeService.GetNodes(botClientRoleConnector.MissionId, null, botClientRoleConnector.MissionPath);
                        if ((nodes != null) && (nodes.Count > 0))
                        {
                            var nodePatterns = missionCompositeService.GetNodePatterns(botClientRoleConnector.MissionId, nodes[0].PatternId);
                            for (int i = 0; i < nodePatterns.Count; i++)
                            {
                                if (isRegexMatch(newMessageText, nodePatterns[i].PatternText, nodePatterns[i].isRegex, nodePatterns[i].isInclude))
                                {
                                    var patternAction = missionCompositeService.GetNodes(botClientRoleConnector.MissionId, nodePatterns[i].NodeId, null);
                                    if ((patternAction != null) && (patternAction.Count > 0) && (patternAction[0].Type == EnumMissionActionType.SendMessage))
                                    {
                                        var botMessage = await textService.RandOriginalMessage(patternAction[0].Text).ConfigureAwait(false);
                                        if (botMessage != null)
                                        {
                                            var sendResult = await PrintAnswerMessage(WebDriverId, botMessage, botClientRoleConnector.Id, ClientVkId).ConfigureAwait(false);
                                            if (sendResult)
                                            {
                                                var saveResult = await SaveNewMessage(WebDriverId, botClientRoleConnector.Id, sendResult, newMessageText, botMessage.Text).ConfigureAwait(false);
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
                    return await SaveNewMessage(WebDriverId, botClientRoleConnector.Id, false, newMessageText, null).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
            return false;
        }

        private async Task<bool> SaveNewMessage(Guid WebDriverId, int BotClientRoleConnectorId, bool isSendAnswerMessageResultSuccess, string ClientMessage, string BotAnswer)
        {
            var result = false;
            try
            {
                var createMessageResult = clientCompositeService.CreateMessage(BotClientRoleConnectorId, ClientMessage, false);
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
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }

        private async Task<string> GetApologies(BotMessageText BotMessageText, int? TextPartIndex = null)
        {
            string result = null;
            try
            {
                if ((BotMessageText.hasMultiplyMissClickError) && (random.Next(0, 100) > 50))
                    result = await textService.GetApologies().ConfigureAwait(false);
                else if ((TextPartIndex != null) && (random.Next(0, 100) > 50))
                    result = await textService.GetApologies(BotMessageText.TextParts[TextPartIndex.Value]).ConfigureAwait(false);
                else if ((BotMessageText.TextParts.FirstOrDefault(item => item.hasCaps == true) != null) && (random.Next(0, 100) > 50))
                    result = await textService.GetCapsApologies().ConfigureAwait(false);

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
                string apologies = null;
                for (int j = 0; j < BotMessage.TextParts.Count; j++)
                {
                    var sendAnswerResult = await vkActionService.SendAnswerMessage(WebDriverId,
                                                  BotMessage.TextParts[j].Text, ClientVkId, BotClientRoleConnectionId).ConfigureAwait(false);
                    if ((!sendAnswerResult.hasError) && (!result) && (!BotMessage.hasMultiplyMissClickError))
                    {
                        apologies = await GetApologies(BotMessage, j).ConfigureAwait(false);
                        if ((random.Next(0, 100) > 90) && (apologies != null))
                        {
                            var sendApologiesResult = await vkActionService.SendAnswerMessage(WebDriverId, apologies,
                                                      ClientVkId, BotClientRoleConnectionId).ConfigureAwait(false);
                            result = !sendApologiesResult.hasError;
                            if (result)
                                clientCompositeService.CreateMessage(BotClientRoleConnectionId, apologies);
                        }
                    }
                    if (!sendAnswerResult.hasError)
                        break;
                }
                if (result)
                {
                    if (apologies != null)
                    {
                        apologies = await GetApologies(BotMessage).ConfigureAwait(false);
                        if (apologies != null)
                        {
                            var sendApologiesResult = await vkActionService.SendAnswerMessage(WebDriverId, apologies,
                                                      ClientVkId, BotClientRoleConnectionId).ConfigureAwait(false);
                            if ((sendApologiesResult.ActionResultMessage == EnumActionResult.Success) && (!sendApologiesResult.hasError))
                                clientCompositeService.CreateMessage(BotClientRoleConnectionId, apologies);
                        }
                    }
                }
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
                var missions = missionCompositeService.GetRoleMissionConnections(roles[i].Id, true);
                for (int j = 0; j < missions.Count; j++)
                    missions.Add(missions[j]);
            }
        }

        private int GetMissionId(int BotId)
        {
            var botWorkStatus = botsWorkStatus.FirstOrDefault(item => item.BotData.Id == BotId);
            if (missions.Count > botWorkStatus.BotWorkMissionsStatus.Count)
            {
                for (int i = 0; i < missions.Count; i++)
                {
                    var unusedMission = botWorkStatus.BotWorkMissionsStatus.FirstOrDefault(item => item.MissionId == missions[i].Id);
                    if (unusedMission == null)
                    {
                        botWorkStatus.BotWorkMissionsStatus.Add(new BotWorkMissionStatus()
                        {
                            MissionId = unusedMission.MissionId,
                            ActionsCount = 0
                        });
                    }
                }
            }
            var botWorkMissionStatus = botWorkStatus.BotWorkMissionsStatus.OrderBy(item => item.ActionsCount).FirstOrDefault();
            botWorkMissionStatus.ActionsCount++;
            return botWorkMissionStatus.MissionId;
        }
    }
}