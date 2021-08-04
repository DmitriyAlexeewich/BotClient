using BotClient.Bussines.Interfaces;
using BotClient.Models.Bot;
using BotClient.Models.Bot.Enumerators;
using BotClient.Models.Bot.Work;
using BotClient.Models.Bot.Work.Enumerators;
using BotClient.Models.Enumerators;
using BotClient.Models.HTMLWebDriver;
using BotClient.Models.WebReports;
using BotDataModels.Bot;
using BotDataModels.Bot.Enumerators;
using BotDataModels.Role;
using BotDataModels.Role.Enumerators;
using BotMySQL.Bussines.Interfaces.Composite;
using BotMySQL.Bussines.Interfaces.MySQL;
using Newtonsoft.Json;
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
        private readonly IParsedClientService parsedClientService;
        private readonly IBotActionService botActionService;

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
                              IPlatformGroupService PlatformGroupService,
                              IParsedClientService ParsedClientService,
                              IBotActionService BotActionService)
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
            parsedClientService = ParsedClientService;
            botActionService = BotActionService;
        }

        private Random random = new Random();
        private List<BotWorkStatusModel> botsWorkStatus = new List<BotWorkStatusModel>();
        private List<HTMLWebDriver> browsers = new List<HTMLWebDriver>();
        private List<int> missionsId = new List<int>();
        private List<Task> BotWorkThreads = new List<Task>();

        public async Task<List<BotWorkStatusModel>> StartBot(int ServerId)
        {
            try
            {
                if (botsWorkStatus.Count < 1)
                {
                    var bots = botCompositeService.GetBotsByServerId(ServerId, null, null);
                    if (bots.Count > 0)
                    {
                        var settings = settingsService.GetServerSettings();
                        browsers = await webDriverService.GetWebDrivers().ConfigureAwait(false);
                        var isUseSessionCount = true;
                        if (bots.Count <= browsers.Count)
                            isUseSessionCount = false;
                        for (int i = 0; i < browsers.Count; i++)
                        {
                            if (bots.Count > 0)
                            {
                                var bot = bots[random.Next(0, bots.Count)];
                                var missionInitializations = random.Next(settings.MinRoleActionCountPerSession, settings.MaxRoleActionCountPerSession);
                                if (!isUseSessionCount)
                                    missionInitializations = random.Next(settings.MinRoleActionCountPerDay, settings.MaxRoleActionCountPerDay);
                                botsWorkStatus.Add(new BotWorkStatusModel()
                                {
                                    BotData = bot,
                                    WebDriverId = browsers[i].Id,
                                    CompletedMissionInitializations = missionInitializations
                                });
                                bots.Remove(bot);
                                if ((i == browsers.Count - 1) && (bots.Count > 0))
                                    i = -1;
                            }
                            else
                                break;
                        }
                        UpdateMissionList(ServerId);
                        for (int i = 0; i < browsers.Count; i++)
                        {
                            var browserId = browsers[i].Id;
                            if (botsWorkStatus.FirstOrDefault(item => item.WebDriverId == browserId) != null)
                                BotWorkThreads.Add(Task.Run(() => { RunBot(browserId, ServerId); }));
                            else
                                await webDriverService.Stop(browserId).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
            return botsWorkStatus;
        }

        public async Task StopBot()
        {
            if ((DateTime.Now.Hour > 0) && (DateTime.Now.Hour < 3))
            {
                for (int i = 0; i < BotWorkThreads.Count; i++)
                    BotWorkThreads[i].Dispose();
                for (int i = 0; i < browsers.Count; i++)
                    await webDriverService.Stop(browsers[i].Id).ConfigureAwait(false);
            }
        }

        public async Task StartQuizBot(int ServerId, int RoleId)
        {
            try
            {
                var bots = botCompositeService.GetBotsByServerId(ServerId, null, null);
                var settings = settingsService.GetServerSettings();
                var browsers = await webDriverService.GetWebDrivers().ConfigureAwait(false);
                bots.RemoveAll(item => item.isDead || item.isPrintBlock);
                if (bots.Count > 0)
                {
                    for (int i = 0; i < browsers.Count; i++)
                    {
                        if (bots.Count < 1)
                            break;
                        botsWorkStatus.Add(new BotWorkStatusModel()
                        {
                            BotData = bots[0],
                            WebDriverId = browsers[i].Id,
                            NextDayOnline = DateTime.Now
                        });
                        bots.RemoveAt(0);
                        if (i >= browsers.Count - 1)
                            i = -1;
                    }
                    for (int i = 0; i < browsers.Count; i++)
                    {
                        var browserId = browsers[i].Id;
                        await Task.Delay(10000);
                        Task.Run(() => { RunQuiz(browserId, RoleId); });
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
        }

        private async void RunQuiz(Guid WebDriverId, int RoleId)
        {
            try
            {
                var botStatus = botsWorkStatus.FirstOrDefault(item => item.WebDriverId == WebDriverId);
                while(botStatus != null)
                {
                    var botId = botStatus.BotData.Id;
                    var settings = settingsService.GetServerSettings();
                    var bot = botCompositeService.GetBotById(botId);
                    var loginflag = await vkActionService.Login(WebDriverId, bot.Login, bot.Password).ConfigureAwait(false);
                    var isLoginSuccess = await vkActionService.isLoginSuccess(WebDriverId).ConfigureAwait(false);
                    if ((!loginflag.hasError) && (isLoginSuccess))
                    {
                        botCompositeService.UpdateOnlineDate(botId, DateTime.Now);
                        await UpdateLoginPassword(WebDriverId, botId).ConfigureAwait(false);
                        if (bot.VkId.Length < 1)
                        {
                            var vkId = await vkActionService.GetVkId(WebDriverId).ConfigureAwait(false);
                            if (vkId.Length > 0)
                            {
                                botCompositeService.SetIsVkId(botId, vkId);
                                bot = botCompositeService.GetBotById(botId);
                            }
                        }
                        if (bot.FullName.Length < 1)
                            await GetFullName(WebDriverId, botId).ConfigureAwait(false);
                        /*
                        if (bot.isUpdatedCustomizeInfo)
                        {
                            var cusomizeData = botCompositeService.GetBotCustomizeByBotId(botId);
                            var botCustomizeSttings = botCompositeService.GetBotCustomizeSettings(botId);
                            botCustomizeSttings.RemoveAll(item => item.isComplete);
                            var cusomizeResult = await botActionService.CustomizeBot(WebDriverId, bot, botCustomizeSttings, cusomizeData).ConfigureAwait(false);
                            if (cusomizeResult)
                            {
                                botCompositeService.SetIsUpdatedCustomizeInfo(botId, false);
                                botCompositeService.SetBotCustomizeIsComplete(botId, true);
                                for (int i = 0; i < botCustomizeSttings.Count; i++)
                                    botCompositeService.SetBotCustomizeSettingsIsComplete(botCustomizeSttings[i].Id, true);
                            }
                        }
                        */
                        var currentDay = DateTime.Now;
                        var startTime = new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, 8, 0, 0);
                        var endTime = new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, 23, 0, 0);

                        int roleActionCount = random.Next(settings.MinRoleActionCountPerDay, settings.MaxRoleActionCountPerDay);
                        var complitedRoleActions = clientCompositeService.GetBotClientRoleConnectionsByBotId(botId);
                        complitedRoleActions.RemoveAll(item => item.UpdateDate < currentDay);
                        roleActionCount -= complitedRoleActions.Count(item => item.isSuccess);
                        var missions = missionCompositeService.GetRoleMissionConnections(RoleId, true);
                        
                        if (botStatus.NextDayOnline > DateTime.Now)
                            roleActionCount = 0;
                        else
                        {
                            botStatus.NextDayOnline = new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, 8, 0, 0);
                            botStatus.NextDayOnline.AddDays(1);
                        }
                        
                        while ((DateTime.Now > startTime) && (DateTime.Now < endTime) && (roleActionCount > 0))
                        {
                            botCompositeService.CreateBotActionHistory(bot.Id, EnumBotActionType.RoleMission, $"Начало выполнение роли");
                            var roleAttept = random.Next(settings.MinRoleAtteptCount, settings.MaxRoleAtteptCount);
                            while (roleAttept > 0)
                            {
                                var randomMissionId = missions[random.Next(0, missions.Count)].MissionId;
                                var botNews = missionCompositeService.GetBotNewsMissionConnectionsByBotId(RoleId, botId);
                                if (botNews.FirstOrDefault(item => item.isWaiting) == null)
                                {
                                    var freeNews = missionCompositeService.GetBotNewsMissionConnectionsByBotId(RoleId, -1);
                                    for (int i = 0; i < freeNews.Count; i++)
                                    {
                                        if (botNews.FirstOrDefault(item => item.MissionId == freeNews[i].MissionId) == null)
                                        {

                                            //freeNews[i].VkLink перход по новости
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    
                                }

                                var setBotClientsRoleConnection = clientCompositeService.SetClientToBot(bot.Id, randomMissionId);
                                if ((!setBotClientsRoleConnection.HasError) && (setBotClientsRoleConnection.Result != null))
                                {
                                    bot = botCompositeService.GetBotById(botId);
                                    if (bot.isPrintBlock)
                                    {
                                        roleAttept = 0;
                                        roleActionCount = 0;
                                        break;
                                    }
                                    var botClientsRoleConnection = setBotClientsRoleConnection.Result;
                                    var roleMissionResult = await ExecuteRoleMission(WebDriverId, botClientsRoleConnection).ConfigureAwait(false);
                                    clientCompositeService.SetBotClientRoleConnectionComplete(botClientsRoleConnection.Id);
                                    if (roleMissionResult)
                                    {
                                        clientCompositeService.SetBotClientRoleConnectionSuccess(botClientsRoleConnection.Id, true);
                                        roleActionCount--;
                                        break;
                                    }
                                    else
                                    {
                                        clientCompositeService.SetBotClientRoleConnectionSuccess(botClientsRoleConnection.Id, false);
                                        roleAttept--;
                                    }
                                }
                            }
                            
                            var nextTime = DateTime.Now.AddMinutes(random.Next(settings.MinQuizRoleWaitingTimeInMinutes, settings.MaxQuizRoleWaitingTimeInMinutes));
                            while ((DateTime.Now < nextTime) && (roleActionCount > 0) && (!bot.isSkipSecondAction))
                            {
                                var waitAction = (EnumBotActionType)random.Next(1, 5);
                                switch (waitAction)
                                {
                                    case EnumBotActionType.WatchVideo:
                                        var searchWordId = botActionService.GetSearchVideoWordId(botCompositeService.GetBotVideos(botId), videoDictionaryService.GetMaxId());
                                        var searchWords = videoDictionaryService.GetAll(searchWordId, 1);
                                        var botVkVideo = await botActionService.StartVideo(WebDriverId, searchWords).ConfigureAwait(false);
                                        if (botVkVideo != null)
                                        {
                                            botCompositeService.CreateBotVideos(botId, searchWords[0].Id, botVkVideo.URL);
                                        }
                                        break;
                                        case EnumBotActionType.ListenMusic:
                                            break;
                                        case EnumBotActionType.News:
                                            break;
                                }
                                await CheckMessage(WebDriverId, botId).ConfigureAwait(false);
                            }
                        }
                        if (roleActionCount <= 0)
                        {
                            var nextTime = DateTime.Now.AddMinutes(random.Next(settings.MinQuizRoleWaitingTimeInMinutes, settings.MaxQuizRoleWaitingTimeInMinutes));
                            while (DateTime.Now < nextTime)
                                await CheckMessage(WebDriverId, botId).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        botCompositeService.SetIsDead(botId, true);
                        botsWorkStatus.RemoveAll(item => item.BotData.Id == botId);
                    }
                    await vkActionService.Logout(WebDriverId).ConfigureAwait(false);
                    await webDriverService.Restart(WebDriverId).ConfigureAwait(false);
                    botCompositeService.UpdateOnlineDate(botId, DateTime.Now);

                    botsWorkStatus.Remove(botStatus);
                    if(!botCompositeService.GetBotById(botStatus.BotData.Id).isPrintBlock)
                        botsWorkStatus.Add(botStatus);
                    botStatus = botsWorkStatus.FirstOrDefault(item => item.WebDriverId == WebDriverId);
                }
                await webDriverService.Stop(WebDriverId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
        }

        private async Task RunBot(Guid WebDriverId, int ServerId)
        {
            try
            {
                var dayUpdate = false;
                while (true)
                {
                    await settingsService.UpdateWebConnectionFile().ConfigureAwait(false);
                    var setting = settingsService.GetServerSettings();
                    var bots = botsWorkStatus.Where(item => item.WebDriverId == WebDriverId).ToList();
                    bots.RemoveAll(item => item.BotData.isDead);
                    if (bots.Count < 1)
                        break;
                    for (int i = 0; i < bots.Count; i++)
                        bots[i].BotData = botCompositeService.GetBotById(bots[i].BotData.Id);
                    var bot = botsWorkStatus.OrderBy(item => item.BotData.NextOnlineDate).ToList().FirstOrDefault(item => item.WebDriverId == WebDriverId);
                    if (bot != null)
                    {
                        var currentDay = DateTime.Now;
                        if ((bot.BotData.isPrintBlock) && (bot.BotData.PrintBlockDate.AddDays(2) > currentDay))
                        {
                            botCompositeService.SetIsPrintBlock(bot.BotData.Id, false);
                            bot.BotData = botCompositeService.GetBotById(bot.BotData.Id);
                        }
                        var loginflag = await vkActionService.Login(WebDriverId, bot.BotData.Login, bot.BotData.Password).ConfigureAwait(false);
                        if ((!loginflag.hasError) && (await vkActionService.isLoginSuccess(WebDriverId).ConfigureAwait(false)))
                        {
                            await UpdateLoginPassword(WebDriverId, bot.BotData.Id).ConfigureAwait(false);
                            bot.SubscribeCount = random.Next(setting.MinSubscribeCount, setting.MaxSubscribeCount);
                            if (bot.BotData.VkId.Length < 1)
                            {
                                var vkId = await vkActionService.GetVkId(WebDriverId).ConfigureAwait(false);
                                if (vkId.Length > 0)
                                {
                                    botCompositeService.SetIsVkId(bot.BotData.Id, vkId);
                                    bot.BotData = botCompositeService.GetBotById(bot.BotData.Id);
                                }
                            }
                            if (random.Next(0, 100) < 20)
                                await GetFullName(WebDriverId, bot.BotData.Id).ConfigureAwait(false);
                            botCompositeService.UpdateOnlineDate(bot.BotData.Id, DateTime.Now);
                            if (bot.BotData.isUpdatedCustomizeInfo)
                            {
                                var cusomizeData = botCompositeService.GetBotCustomizeByBotId(bot.BotData.Id);
                                var botCustomizeSttings = botCompositeService.GetBotCustomizeSettings(bot.BotData.Id);
                                var cusomizeResult = await botActionService.CustomizeBot(WebDriverId, bot.BotData, botCustomizeSttings, cusomizeData).ConfigureAwait(false);
                                if (cusomizeResult)
                                    botCompositeService.SetIsUpdatedCustomizeInfo(bot.BotData.Id, false);
                            }
                            var botSchedule = new List<EnumBotActionType>();
                            var startTime = new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, 8, 0, 0);
                            var endTime = new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, 23, 0, 0);
                            if (!bot.BotData.isSkipSecondAction)
                            {
                                var maxSecondActionsCount = 0;
                                if ((currentDay > startTime) && (currentDay < endTime))
                                {
                                    if ((!bot.BotData.isPrintBlock) && (!bot.BotData.isChill) && (bot.CompletedMissionInitializations > 0))
                                    {
                                        var botClientRoleConnections = clientCompositeService.GetBotClientRoleConnectionsByBotId(bot.BotData.Id);
                                        if ((botClientRoleConnections.Count < 1) || ((botClientRoleConnections.Count > 0) && (botClientRoleConnections[botClientRoleConnections.Count - 1].UpdateDate.Day != DateTime.Now.Day)))
                                        {
                                            for (int j = 0; j < bot.CompletedMissionInitializations; j++)
                                            {
                                                botSchedule.Add(EnumBotActionType.RoleMission);
                                                maxSecondActionsCount += 2;
                                            }
                                        }
                                        else
                                            maxSecondActionsCount = random.Next(1, 3);
                                    }
                                    else
                                    {
                                        maxSecondActionsCount = random.Next(1, 3);
                                        if(bot.BotData.isChill)
                                            maxSecondActionsCount = random.Next(5, 10);
                                    }
                                }
                                else
                                    maxSecondActionsCount = random.Next(setting.MinNightSecondActionCountPerSession, setting.MaxNightSecondActionCountPerSession);
                                for (int j = 0; j < maxSecondActionsCount; j++)
                                    botSchedule.Add((EnumBotActionType)random.Next(1, 5));
                                botSchedule = settingsService.ShuffleSchedule(botSchedule);
                                botCompositeService.CreateBotActionHistory(bot.BotData.Id, EnumBotActionType.RoleMission, $"Рассписание: {JsonConvert.SerializeObject(botSchedule)}");
                                await CheckMessage(WebDriverId, bot.BotData.Id).ConfigureAwait(false);
                            }
                            else
                            {
                                for (int i = 0; i < 65000; i++)
                                    botSchedule.Add(EnumBotActionType.RoleMission);
                            }
                            botSchedule[0] = EnumBotActionType.RoleMission;
                            for (int j = 0; j < botSchedule.Count; j++)
                            {
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
                                        //await vkActionService.News(WebDriverId).ConfigureAwait(false);
                                        break;
                                    case EnumBotActionType.Group:
                                        botCompositeService.CreateBotActionHistory(bot.BotData.Id, botSchedule[j], $"Просмотр или подписка на группу");
                                        await SubscribeRepostGroup(WebDriverId, bot);
                                        break;
                                    case EnumBotActionType.RoleMission:
                                        if (!bot.BotData.isPrintBlock)
                                        {
                                            botCompositeService.CreateBotActionHistory(bot.BotData.Id, botSchedule[j], $"Начало выполнение роли");
                                            var roleAttept = random.Next(setting.MinRoleAtteptCount, setting.MinRoleAtteptCount);
                                            for (int k = 0; k < roleAttept; k++)
                                            {
                                                settingsService.WaitTime(random.Next(setting.MinRoleWaitingTime, setting.MaxRoleWaitingTime));
                                                var setBotClientsRoleConnection = clientCompositeService.SetClientToBot(bot.BotData.Id, GetMissionId(bot.BotData.Id));
                                                if ((!setBotClientsRoleConnection.HasError) && (setBotClientsRoleConnection.Result != null))
                                                {
                                                    var botClientsRoleConnection = setBotClientsRoleConnection.Result;
                                                    var roleMissionResult = await ExecuteRoleMission(WebDriverId, botClientsRoleConnection).ConfigureAwait(false);
                                                    clientCompositeService.SetBotClientRoleConnectionComplete(botClientsRoleConnection.Id);
                                                    if (roleMissionResult)
                                                    {
                                                        bot.CompletedMissionInitializations--;
                                                        clientCompositeService.SetBotClientRoleConnectionSuccess(botClientsRoleConnection.Id, true);
                                                        break;
                                                    }
                                                    else
                                                        clientCompositeService.SetBotClientRoleConnectionSuccess(botClientsRoleConnection.Id, false);
                                                }
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                await CheckMessage(WebDriverId, bot.BotData.Id).ConfigureAwait(false);

                                bot.BotData = botCompositeService.GetBotById(bot.BotData.Id);
                            }
                            await CheckMessage(WebDriverId, bot.BotData.Id).ConfigureAwait(false);
                            var dialogsCount = await vkActionService.GetNewDialogsCount(WebDriverId).ConfigureAwait(false);
                            for (int j = 0; j < 100; j++)
                            {
                                settingsService.WaitTime(1000);
                                var newDialogsCount = await vkActionService.GetNewDialogsCount(WebDriverId).ConfigureAwait(false);
                                if (dialogsCount < newDialogsCount)
                                {
                                    await CheckMessage(WebDriverId, bot.BotData.Id).ConfigureAwait(false);
                                    j = 0;
                                }
                            }
                            await CheckMessage(WebDriverId, bot.BotData.Id).ConfigureAwait(false);
                            botCompositeService.CreateBotActionHistory(bot.BotData.Id, EnumBotActionType.RoleMission, $"Выход из профиля");
                            
                        }
                        else
                            botCompositeService.SetIsDead(bot.BotData.Id, true);
                        var logoutresult = await Logout(WebDriverId).ConfigureAwait(false);
                        botCompositeService.SetIsOnline(bot.BotData.Id, false);
                        botsWorkStatus.Remove(bot);
                        //botsWorkStatus.Add(bot);
                        UpdateMissionList(ServerId);
                        botCompositeService.UpdateOnlineDate(bot.BotData.Id, DateTime.Now);
                        botCompositeService.UpdateNextOnlineDate(bot.BotData.Id, currentDay.AddDays(random.Next(1, 3)).AddHours(random.Next(1, 10)).AddMinutes(random.Next(1, 60)));
                        //UpdateBotList(ServerId);
                        if ((DateTime.Now.Hour > 6) && (DateTime.Now.Hour < 7) && (!dayUpdate))
                        {
                            await UpdateBotsList(ServerId).ConfigureAwait(false);
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
                await webDriverService.Stop(WebDriverId).ConfigureAwait(false);
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
                var newDialogsCount = await vkActionService.GetNewDialogsCount(WebDriverId).ConfigureAwait(false);
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
                    var MusicWaitingDeltaTime = settingsService.GetServerSettings().MusicWaitingDeltaTime;
                    MusicWaitingDeltaTime = settingsService.GetServerSettings().MusicWaitingTime + random.Next(-MusicWaitingDeltaTime, MusicWaitingDeltaTime);
                    MusicWaitingDeltaTime /= 1000;
                    if (random.Next(0, 100) > 30)
                    {
                        await ScanningNewDialogs(MusicWaitingDeltaTime, WebDriverId, newDialogsCount).ConfigureAwait(false);
                        await vkActionService.StopMusic(WebDriverId).ConfigureAwait(false);
                    }
                    else
                        Task.Run(async () => 
                        {
                            settingsService.WaitTime(MusicWaitingDeltaTime);
                            await vkActionService.StopMusic(WebDriverId).ConfigureAwait(false);
                        });
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
                var newDialogsCount = await vkActionService.GetNewDialogsCount(WebDriverId).ConfigureAwait(false);
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
                            if (videos.Count > 0)
                            {
                                var randomVideoIndex = random.Next(0, videos.Count);
                                var clickVideoResult = await vkActionService.ClickVideo(WebDriverId, videos[randomVideoIndex].HTMLElement).ConfigureAwait(false);
                                if ((!clickVideoResult.hasError) && (clickVideoResult.ActionResultMessage == EnumActionResult.Success))
                                {
                                    botCompositeService.CreateBotActionHistory(BotId, EnumBotActionType.ListenMusic,
                                                                                   $"Просмотр {videos[randomVideoIndex].URL}");
                                    var videoWaitingDeltaTime = settingsService.GetServerSettings().VideoWaitingDeltaTime;
                                    videoWaitingDeltaTime = settingsService.GetServerSettings().VideoWaitingTime + random.Next(-videoWaitingDeltaTime, videoWaitingDeltaTime);
                                    if (!((videoWaitingDeltaTime > 0) && (videoWaitingDeltaTime < Int32.MaxValue)))
                                        videoWaitingDeltaTime = settingsService.GetServerSettings().VideoWaitingTime;
                                    videoWaitingDeltaTime /= 1000;
                                    await ScanningNewDialogs(videoWaitingDeltaTime, WebDriverId, newDialogsCount).ConfigureAwait(false);
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
                        if (BotClientRoleConnector.MissionId != -1)
                            mission = missionCompositeService.GetMissionById(BotClientRoleConnector.MissionId);
                        botCompositeService.CreateBotActionHistory(BotClientRoleConnector.BotId, EnumBotActionType.RoleMission, 
                                                                       $"Выполнение миссии {mission.Id} ({mission.Title}, {role.UpdateDate}) " +
                                                                       $"с контактёром {BotClientRoleConnector.ClientId}");
                        var nodes = missionCompositeService.GetNodes(mission.Id, null, null);
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
                                                client.FullName = await vkActionService.GetClientName(WebDriverId).ConfigureAwait(false);
                                                client.canRecievedMessage = await vkActionService.GetCanRecievedMessage(WebDriverId).ConfigureAwait(false);
                                                botCompositeService.CreateBotActionHistory(BotClientRoleConnector.BotId, EnumBotActionType.RoleMission,
                                                                    $"Успешный переход на страницу контактёра {BotClientRoleConnector.ClientId} ({client.FullName})");
                                                clientCompositeService.UpdateClientData(client);
                                                await webDriverService.GetScreenshot(WebDriverId, BotClientRoleConnector.RoleId, BotClientRoleConnector.Id, DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss")).ConfigureAwait(false);
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
                                            var botData = botCompositeService.GetBotById(BotClientRoleConnector.BotId);
                                            if (!botData.isPrintBlock)
                                            {
                                                var messageText = await textService.InsertText(nodes[i].Text, BotClientRoleConnector.Id.ToString()).ConfigureAwait(false);
                                                var newMessage = await textService.RandOriginalMessage(messageText).ConfigureAwait(false);
                                                if (newMessage != null)
                                                {
                                                    botCompositeService.CreateBotActionHistory(BotClientRoleConnector.BotId, EnumBotActionType.RoleMission,
                                                                           $"Отправка сообщения {newMessage.Text} контактёру {BotClientRoleConnector.ClientId} ({client.FullName})");
                                                    string apologies = "";
                                                    stepResult = false;
                                                    for (int j = 0; j < newMessage.TextParts.Count; j++)
                                                    {
                                                        var sendResult = await vkActionService.SendFirstMessage(WebDriverId, newMessage.TextParts[j].Text, BotClientRoleConnector.RoleId, BotClientRoleConnector.Id, stepResult).ConfigureAwait(false);
                                                        if (await vkActionService.hasCaptcha(WebDriverId).ConfigureAwait(false))
                                                        {
                                                            botCompositeService.SetIsPrintBlock(BotClientRoleConnector.BotId, true);
                                                            break;
                                                        }
                                                        if ((sendResult.ActionResultMessage == EnumActionResult.Success) && (!sendResult.hasError))
                                                        {
                                                            clientCompositeService.CreateMessage(BotClientRoleConnector.Id, newMessage.TextParts[j].Text);
                                                            stepResult = true;
                                                            if (apologies.Length < 1)
                                                            {
                                                                apologies = await GetApologies(newMessage, j).ConfigureAwait(false);
                                                                if (apologies.Length > 0)
                                                                {
                                                                    var sendApologiesResult = await vkActionService.SendFirstMessage(WebDriverId, apologies, BotClientRoleConnector.RoleId, BotClientRoleConnector.Id).ConfigureAwait(false);
                                                                    if ((!sendApologiesResult.hasError) && (sendApologiesResult.ActionResultMessage == EnumActionResult.Success))
                                                                        clientCompositeService.CreateMessage(BotClientRoleConnector.Id, apologies);
                                                                }
                                                            }
                                                        }
                                                        else
                                                            break;
                                                    }
                                                    if (stepResult)
                                                    {
                                                        if (apologies.Length < 1)
                                                        {
                                                            apologies = await GetApologies(newMessage).ConfigureAwait(false);
                                                            if (apologies.Length > 0)
                                                            {
                                                                var sendApologiesResult = await vkActionService.SendFirstMessage(WebDriverId, apologies, BotClientRoleConnector.RoleId, BotClientRoleConnector.Id).ConfigureAwait(false);
                                                                clientCompositeService.CreateMessage(BotClientRoleConnector.Id, apologies);
                                                                if ((!sendApologiesResult.hasError) && (sendApologiesResult.ActionResultMessage == EnumActionResult.Success))
                                                                    clientCompositeService.CreateMessage(BotClientRoleConnector.Id, newMessage.Text);
                                                            }
                                                        }
                                                        botCompositeService.CreateBotActionHistory(BotClientRoleConnector.BotId, EnumBotActionType.RoleMission,
                                                                               $"Успешная отправка сообщения {newMessage.Text} контактёру {BotClientRoleConnector.ClientId} ({client.FullName})");
                                                        await vkActionService.CheckIsSended(WebDriverId, client.VkId, BotClientRoleConnector.RoleId, BotClientRoleConnector.Id, 50 < random.Next(0, 100)).ConfigureAwait(false);
                                                    }                                                   
                                                }
                                                else
                                                    stepResult = false;
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
                                        case EnumMissionActionType.ParseContacts:
                                            var parsedClients = await vkActionService.GetContacts(WebDriverId).ConfigureAwait(false);
                                            parsedClients = parsedClients.Select(item => { item.RoleId = BotClientRoleConnector.RoleId; return item; }).ToList();
                                            parsedClientService.CreateParsedClients(parsedClients);
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
                var bot = botCompositeService.GetBotById(BotId);
                var dialogsCount = await vkActionService.GetNewDialogsCount(WebDriverId).ConfigureAwait(false);
                while (!bot.isPrintBlock)
                {
                    var dialogs = await vkActionService.GetDialogsWithNewMessages(WebDriverId).ConfigureAwait(false);
                    for (int i = 0; i < dialogs.Count; i++)
                    {
                        var botClientRoleConnector = clientCompositeService.GetBotClientRoleConnectionByBotClientVkId(BotId, dialogs[i].ClientVkId, true);
                        if ((botClientRoleConnector != null) && (!botClientRoleConnector.isChatBlocked))
                        {
                            clientCompositeService.SetBotClientRoleConnectionPlatformLastMessageDate(botClientRoleConnector.Id, dialogs[i].PlatformLastMessageDate);
                            var readNewMessagesResult = false;
                            var mission = missionCompositeService.GetMissionById(botClientRoleConnector.MissionId);
                            if (mission.isQuiz)
                            {
                                if (!botClientRoleConnector.isScenarioComplete)
                                    readNewMessagesResult = await ReadNewMessages(WebDriverId, botClientRoleConnector.RoleId, botClientRoleConnector, dialogs[i].ClientVkId).ConfigureAwait(false);
                            }
                            else
                                readNewMessagesResult = await ReadNewMessages(WebDriverId, botClientRoleConnector.RoleId, botClientRoleConnector, dialogs[i].ClientVkId).ConfigureAwait(false);
                        }
                        await vkActionService.CloseDialog(WebDriverId).ConfigureAwait(false);
                        bot = botCompositeService.GetBotById(BotId);
                    }
                    var botDialogsWithNewBotMessages = clientCompositeService.GetBotClientRoleConnectionWithNewBotMessages(BotId);
                    if ((botDialogsWithNewBotMessages != null) && (botDialogsWithNewBotMessages.Count > 0) && (!bot.isPrintBlock))
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
                                    messages = messages.FindAll(item => item.isBotMessage && item.isSended);
                                    if ((messages != null) && (messages.Count > 0))
                                    {
                                        var sendAnswerMessageResult = new AlgoritmResult()
                                        {
                                            ActionResultMessage = EnumActionResult.ElementError,
                                            hasError = true
                                        };
                                        for (int j = 0; j < messages.Count; j++)
                                        {
                                            var messageText = await textService.InsertText(messages[j].Text, botDialogsWithNewBotMessages[i].Id.ToString()).ConfigureAwait(false);
                                            //var newBotMessage = await textService.RandOriginalMessage(messageText).ConfigureAwait(false);
                                            botCompositeService.CreateBotActionHistory(BotId, EnumBotActionType.RoleMission,
                                                                                           $"Отправка сообщения контактёру {client.Id} ({client.FullName})");
                                            if (messageText != null)
                                            {
                                                sendAnswerMessageResult = await vkActionService.SendAnswerMessage(WebDriverId, messageText,
                                                        client.VkId, botDialogsWithNewBotMessages[i].Id).ConfigureAwait(false);
                                                var hasCaptcha = await vkActionService.hasCaptcha(WebDriverId).ConfigureAwait(false);
                                                if (!hasCaptcha)
                                                {
                                                    if ((!sendAnswerMessageResult.hasError) && (sendAnswerMessageResult.ActionResultMessage == EnumActionResult.Success))
                                                        clientCompositeService.UpdateMessageSendResult(messages[j].Id, false);
                                                    else
                                                        break;
                                                }
                                                else
                                                    botCompositeService.SetIsPrintBlock(BotId, true);
                                                await webDriverService.GetScreenshot(WebDriverId, botDialogsWithNewBotMessages[i].RoleId, botDialogsWithNewBotMessages[i].Id, DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss")).ConfigureAwait(false);
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
                            bot = botCompositeService.GetBotById(BotId);
                        }
                    }
                    var newDialogsCount = await vkActionService.GetNewDialogsCount(WebDriverId).ConfigureAwait(false);
                    if (dialogsCount >= newDialogsCount)
                        break;
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
                    if (newMessages.FirstOrDefault(item => item.hasChatBlocked) == null)
                    {
                        var newMessageText = "";
                        for (int i = 0; i < newMessages.Count; i++)
                            newMessageText += newMessages[i].Text + " ";
                        newMessageText = newMessageText.Remove(newMessageText.Length - 1);
                        if (true)//newMessages.FirstOrDefault(item => item.hasAudio) == null)
                        {
                            var mission = missionCompositeService.GetMissionById(botClientRoleConnector.MissionId);
                            if ((!mission.isQuiz) && (!mission.isIgnorePattern))
                            {
                                var standartPatterns = missionCompositeService.GetStandartPatterns(RoleId);
                                var nonRoleStandartPatterns = missionCompositeService.GetNonRoleStandartPatterns();
                                standartPatterns.AddRange(nonRoleStandartPatterns);
                                for (int i = 0; i < standartPatterns.Count; i++)
                                {
                                    if (isRegexMatch(newMessageText, standartPatterns[i].Text, standartPatterns[i].isRegex, standartPatterns[i].isInclude))
                                    {
                                        var messageText = await textService.InsertText(standartPatterns[i].AnswerText, botClientRoleConnector.Id.ToString()).ConfigureAwait(false);
                                        var botMessage = await textService.RandOriginalMessage(messageText).ConfigureAwait(false);
                                        if (botMessage != null)
                                        {
                                            var sendResult = await PrintAnswerMessage(WebDriverId, botMessage, botClientRoleConnector, ClientVkId).ConfigureAwait(false);
                                            var saveResult = await SaveNewMessage(WebDriverId, botClientRoleConnector.Id, sendResult, newMessageText, botMessage.Text).ConfigureAwait(false);
                                            return saveResult;
                                        }
                                        return false;
                                    }
                                }
                            }
                            if ((!botClientRoleConnector.isScenarioComplete) && (!botClientRoleConnector.isChatBlocked))
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
                                            if ((patternAction != null) && (patternAction.Count > 0))
                                            {
                                                if (patternAction[0].Type == EnumMissionActionType.SendMessage)
                                                {
                                                    var messageText = await textService.InsertText(patternAction[0].Text, botClientRoleConnector.Id.ToString()).ConfigureAwait(false);
                                                    var botMessage = await textService.RandOriginalMessage(messageText).ConfigureAwait(false);
                                                    if (botMessage != null)
                                                    {
                                                        var sendResult = await PrintAnswerMessage(WebDriverId, botMessage, botClientRoleConnector, ClientVkId).ConfigureAwait(false);
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
                                                    return false;
                                                }
                                                if (patternAction[0].Type == EnumMissionActionType.End)
                                                {
                                                    if (botClientRoleConnector.MissionPath.Length > 0)
                                                        botClientRoleConnector.MissionPath += ";";
                                                    botClientRoleConnector.MissionPath += nodes[i].NodeId + ";" + patternAction[0].NodeId;
                                                    clientCompositeService.SetBotClientRoleConnectionMissionPath(botClientRoleConnector.Id, botClientRoleConnector.MissionPath);
                                                    clientCompositeService.SetBotClientRoleConnectionScenarioComplete(botClientRoleConnector.Id, true);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            await webDriverService.GetScreenshot(WebDriverId, botClientRoleConnector.RoleId, botClientRoleConnector.Id, DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss")).ConfigureAwait(false);
                        }
                        /*else
                            await vkActionService.SendAnswerMessage(WebDriverId, await textService.AudioReaction().ConfigureAwait(false), ClientVkId, botClientRoleConnector.Id).ConfigureAwait(false);
                        */return await SaveNewMessage(WebDriverId, botClientRoleConnector.Id, false, newMessageText, null).ConfigureAwait(false);
                    }
                    else
                        clientCompositeService.SetIsChatBlocked(botClientRoleConnector.Id, true);
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
                if ((!createMessageResult.HasError) && (createMessageResult.Result))
                {
                    if (isSendAnswerMessageResultSuccess)
                    {
                        if (BotAnswer != null)
                        {
                            createMessageResult = clientCompositeService.CreateMessage(BotClientRoleConnectorId, BotAnswer);
                            if ((!createMessageResult.HasError) && (createMessageResult.Result))
                                result = true;
                        }
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

        private async Task<bool> PrintAnswerMessage(Guid WebDriverId, BotMessageText BotMessage, BotClientRoleConnectorModel BotClientRoleConnector, string ClientVkId)
        {
            var result = false;
            try
            {
                string apologies = "";
                for (int j = 0; j < BotMessage.TextParts.Count; j++)
                {
                    var sendAnswerResult = await vkActionService.SendAnswerMessage(WebDriverId,
                                                  BotMessage.TextParts[j].Text, ClientVkId, BotClientRoleConnector.Id).ConfigureAwait(false);
                    if ((sendAnswerResult.ActionResultMessage == EnumActionResult.Success) && (!sendAnswerResult.hasError) && (!result))
                        result = true;
                    if ((result) && (!BotMessage.hasMultiplyMissClickError))
                    {
                        apologies = await GetApologies(BotMessage, j).ConfigureAwait(false);
                        if ((random.Next(0, 100) > 90) && (apologies.Length > 0))
                        await vkActionService.SendAnswerMessage(WebDriverId, apologies, ClientVkId, BotClientRoleConnector.Id).ConfigureAwait(false);
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
                                                      ClientVkId, BotClientRoleConnector.Id).ConfigureAwait(false);
                            if ((sendApologiesResult.ActionResultMessage == EnumActionResult.Success) && (!sendApologiesResult.hasError))
                                clientCompositeService.CreateMessage(BotClientRoleConnector.Id, apologies);
                        }
                    }
                }
                if (!await vkActionService.hasChatBlock(WebDriverId).ConfigureAwait(false))
                    await webDriverService.GetScreenshot(WebDriverId, BotClientRoleConnector.RoleId, BotClientRoleConnector.Id, DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss")).ConfigureAwait(false);
                else
                    clientCompositeService.SetIsChatBlocked(BotClientRoleConnector.Id, true);
                if (await vkActionService.hasCaptcha(WebDriverId).ConfigureAwait(false))
                    botCompositeService.SetIsPrintBlock(BotClientRoleConnector.BotId, true);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }

        private async Task RememberMission(Guid WebDriverId, int BotId)
        {
            try
            {
                var rememberDate = DateTime.Now.AddHours(-12);
                var missions = missionCompositeService.GetMissionsByBotId(BotId);
                var missionsNodes = new List<List<MissionNodeModel>>();
                for (int i = 0; i < missions.Count; i++)
                {
                    var missionNodes = missionCompositeService.GetNodes(missions[i].Id);
                    missionNodes.RemoveAll(item => !item.isRemember);
                    if (missionNodes.Count > 0)
                        missionsNodes.Add(missionNodes);
                }
                if (missionsNodes.Count > 0)
                {
                    for (int i = 0; i < missionsNodes.Count; i++)
                    {
                        var botClientConnections = clientCompositeService.GetBotClientRoleConnectionsByBotId(BotId);
                        botClientConnections.RemoveAll(item => item.UpdateDate > rememberDate || item.isScenarioComplete || item.MissionId != missionsNodes[i][0].MissionId || item.isChatBlocked);
                        for (int j = 0; j < botClientConnections.Count; j++)
                        {
                            var missionPathNodes = botClientConnections[j].MissionPath.Split(';').ToList();
                            var missionNodeId = -1;
                            if ((Int32.TryParse(missionPathNodes[missionPathNodes.Count - 1], out missionNodeId)) && (missionNodeId > 0))
                            {
                                if (missionsNodes[i].FirstOrDefault(item => item.NodeId == missionNodeId) != null)
                                {
                                    var client = clientCompositeService.GetClientById(botClientConnections[j].ClientId);
                                    var newClientMessages = await vkActionService.GetNewMessagesInDialog(WebDriverId, client.VkId).ConfigureAwait(false);
                                    if (newClientMessages.Count < 1)
                                    {
                                        var rememberMessage = await textService.GetRememberMessage(missionNodeId, missionsNodes[j]).ConfigureAwait(false);
                                        var sendResult = await vkActionService.SendAnswerMessage(WebDriverId, rememberMessage, client.VkId, botClientConnections[j].Id).ConfigureAwait(false);
                                        clientCompositeService.CreateMessage(botClientConnections[j].Id, rememberMessage);
                                    }
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

        private void UpdateMissionList(int ServerId)
        {
            var roles = roleServerConnectorService.GetByServerId(ServerId);
            for (int i = 0; i < roles.Count; i++)
            {
                var roleMissions = missionCompositeService.GetRoleMissionConnections(roles[i].RoleId, true);
                for (int j = 0; j < roleMissions.Count; j++)
                    missionsId.Add(roleMissions[j].MissionId);
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

        private async Task<bool> SubscribeRepostGroup(Guid WebDriverId, BotWorkStatusModel Bot)
        {
            var result = false;
            try
            {
                var settings = settingsService.GetServerSettings();
                var botData = botCompositeService.GetBotById(Bot.BotData.Id);
                var botGroups = botCompositeService.GetBotPlatformGroupConnectionsByBotId(Bot.BotData.Id);
                var isSubscribe = false;
                if ((botGroups.Count > 0) && (Bot.SubscribeCount > 0))
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
                            botCompositeService.CreateBotPlatformGroupConnection(Bot.BotData.Id, groups[randomGroupId].Id);
                            botGroups = botCompositeService.GetBotPlatformGroupConnectionsByBotId(Bot.BotData.Id);
                            Bot.SubscribeCount--;
                        }
                    }
                }
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
                            if (posts.Count > 0)
                            {
                                var isRepost = false;
                                if (settings.RepostChancePerDay < random.Next(0, 100))
                                    isRepost = true;
                                settingsService.WaitTime(random.Next(1000, 60000));
                                var watchResult = await vkActionService.WatchPost(WebDriverId, posts[random.Next(0, posts.Count)], isRepost).ConfigureAwait(false);
                                settingsService.WaitTime(random.Next(1000, 60000));
                                if ((watchResult.ActionResultMessage == EnumActionResult.Success) && (isRepost))
                                {
                                    botCompositeService.AddRepostCount(botGroups[0].Id, botGroups[0].RepostCount + 1);
                                }
                                if (watchResult.ActionResultMessage == EnumActionResult.Success)
                                    result = true;
                            }
                        }
                    }
                }
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
        
        private async Task UpdateBotsList(int ServerId)
        {
            try
            {
                var settings = settingsService.GetServerSettings();
                var bots = botCompositeService.GetBotsByServerId(ServerId, null, null);
                for (int i = 0; i < botsWorkStatus.Count; i++)
                    bots.RemoveAll(item => item.Id == botsWorkStatus[i].BotData.Id);
                var sessionBotsCount = bots.Count / browsers.Count;
                for (int i = 0; i < browsers.Count; i++)
                {
                    for (int j = 0; j < sessionBotsCount; j++)
                    {
                        var bot = bots[random.Next(0, bots.Count)];
                        var missionInitializations = random.Next(settings.MinRoleActionCountPerSession, settings.MaxRoleActionCountPerSession);
                        if (sessionBotsCount < 2)
                            missionInitializations = random.Next(settings.MinRoleActionCountPerDay, settings.MaxRoleActionCountPerDay);
                        botsWorkStatus.Add(new BotWorkStatusModel()
                        {
                            BotData = bot,
                            WebDriverId = browsers[i].Id,
                            CompletedMissionInitializations = missionInitializations
                        });
                        bots.Remove(bot);
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
        }

        private bool isMassSetPrintBlock(int ServerId, DateTime CurrentDate)
        {
            var bots = botCompositeService.GetBotsByServerId(ServerId, null, null);
            if (bots.Select(item => item.PrintBlockDate > CurrentDate.AddDays(-2)).ToList().Count > bots.Count / 3)
                return true;
            return false;
        }

        private async Task UpdateLoginPassword(Guid WebDriverId, int BotId)
        {
            try
            {
                var botData = botCompositeService.GetBotById(BotId);
                if (!botData.isLogin)
                {
                    var newPassword = await settingsService.GeneratePassword(random.Next(10, 16)).ConfigureAwait(false);
                    if (newPassword.Length > 0)
                    {
                        var goToSettingsResult = await vkActionService.GoToSettings(WebDriverId).ConfigureAwait(false);
                        if ((!goToSettingsResult.hasError) && (goToSettingsResult.ActionResultMessage == EnumActionResult.Success))
                        {
                            var changePasswordResult = await vkActionService.ChangePassword(WebDriverId, botData.Password, newPassword).ConfigureAwait(false);
                            if ((!changePasswordResult.hasError) && (changePasswordResult.ActionResultMessage == EnumActionResult.Success))
                            {
                                var updateResult = botCompositeService.UpdatePassword(botData.Id, newPassword);
                                if ((updateResult.HasError) || (!updateResult.Result))
                                    await vkActionService.ChangePassword(WebDriverId, newPassword, botData.Password).ConfigureAwait(false);
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

        private async Task GetFullName(Guid WebDriverId, int BotId)
        {
            try
            {
                var botData = botCompositeService.GetBotById(BotId);
                if (botData.FullName.Length < 1)
                {
                    var goToSlefPageResult = await vkActionService.GoToSelfPage(WebDriverId).ConfigureAwait(false);
                    if ((!goToSlefPageResult.hasError) && (goToSlefPageResult.ActionResultMessage == EnumActionResult.Success))
                    {
                        var fullName = await vkActionService.GetPageName(WebDriverId).ConfigureAwait(false);
                        if (fullName.Length > 0)
                            botCompositeService.UpdateFullName(botData.Id, fullName);
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
        }

        

        private async Task ScanningNewDialogs(int WaitingTime, Guid WebDriverId, int? DialogCount = null)
        {
            try
            {
                var newDialogsCount = await vkActionService.GetNewDialogsCount(WebDriverId).ConfigureAwait(false);
                if (DialogCount != null)
                    newDialogsCount = DialogCount.Value;
                for (int i=0; i < WaitingTime; i++)
                {
                    settingsService.WaitTime(1000);
                    if (newDialogsCount != await vkActionService.GetNewDialogsCount(WebDriverId).ConfigureAwait(false))
                        break;
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
        }

        private void UpdateBotList(int ServerId)
        {
            var botList = botCompositeService.GetBotsByServerId(ServerId, null, null);
            for (int i = 0; i < botsWorkStatus.Count; i++)
            {
                var bot = botList.FirstOrDefault(item => item.Id == botsWorkStatus[i].BotData.Id);
                if (bot != null)
                {
                    botsWorkStatus[i].BotData.isDead = bot.isDead;
                    botList.Remove(bot);
                }
            }
            for (int i = 0; i < botList.Count; i++)
            {
                var webDriverId = GetMinBotBrowserId();
                botsWorkStatus.Add(new BotWorkStatusModel()
                {
                    BotData = botList[i],
                    WebDriverId = webDriverId
                });
            }
        }

        private Guid GetMinBotBrowserId()
        {
            var webDriversBotCount = new List<WebDriverBotCountModel>();
            for (int i = 0; i < browsers.Count; i++)
            {
                var browserBots = botsWorkStatus.Select(item => item.WebDriverId == browsers[i].Id).ToList();
                webDriversBotCount.Add(new WebDriverBotCountModel()
                {
                    WebDriverId = browsers[i].Id,
                    BotCount = browserBots.Count
                });
            }
            webDriversBotCount = webDriversBotCount.OrderBy(item => item.BotCount).ToList();
            return webDriversBotCount[0].WebDriverId;
        }


        private async Task<bool> ExecuteNewsRole()
        {
        
        }


        public async Task<List<string>> Test(string Text)
        {
            var botSchedule = new List<EnumBotActionType>();
            var maxSecondActionsCount = 3;
            for (int j = 0; j < maxSecondActionsCount; j++)
                botSchedule.Add((EnumBotActionType)random.Next(1, 5));
            botSchedule = settingsService.ShuffleSchedule(botSchedule);
            return new List<string>();
        }
    }
}