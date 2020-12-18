using BotClient.Bussines.Interfaces;
using BotClient.Models.Bot;
using BotClient.Models.Bot.Enumerators;
using BotClient.Models.Bot.Work;
using BotClient.Models.Bot.Work.Enumerators;
using BotClient.Models.Enumerators;
using BotClient.Models.HTMLWebDriver;
using BotClient.Models.WebReports;
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


        public BotWorkService(IBotCompositeService BotCompositeService,
                              IClientCompositeService ClientCompositeService,
                              IMissionCompositeService MissionCompositeService,
                              IWebDriverService WebDriverService,
                              ISettingsService SettingsService,
                              IVkActionService VkActionService,
                              IDialogScreenshotService DialogScreenshotService,
                              IVideoDictionaryService VideoDictionaryService)
        {
            botCompositeService = BotCompositeService;
            clientCompositeService = ClientCompositeService;
            missionCompositeService = MissionCompositeService;
            webDriverService = WebDriverService;
            settingsService = SettingsService;
            vkActionService = VkActionService;
            dialogScreenshotService = DialogScreenshotService;
            videoDictionaryService = VideoDictionaryService;
        }

        private List<BotWorkStatusModel> bots = new List<BotWorkStatusModel>();
        private Random random = new Random();
        private List<BotRoleActionsDaySchedule> botRoleActions = new List<BotRoleActionsDaySchedule>();
        private List<string> randomMessages = new List<string>();

        public async Task<BotStartReport> StartBot(List<int> BotsId)
        {
            var result = new BotStartReport();

            //Check bots data

            try
            {
                var noBots = false;
                if (bots.Count < 1)
                    noBots = true;
                for (int i = 0; i < BotsId.Count; i++)
                {
                    var bufferBotData = botCompositeService.GetBotById(BotsId[i]);
                    if ((bufferBotData != null) && (!bufferBotData.isDead) && (!bufferBotData.isPrintBlock))
                    {
                        bots.Add(new BotWorkStatusModel()
                        {
                            BotData = bufferBotData,
                            WorkStatus = EnumBotWorkStatus.Free,
                            RoleId = bufferBotData.RoleId
                        });
                        result.BotCount++;
                        result.Bots.Add(bufferBotData);
                    }
                    else
                    {
                        result.ErrorBotCount++;
                        result.ErrorBots.Add(bufferBotData);
                    }
                }

                //Set WebDriver to success checked bots

                var webDrivers = await webDriverService.GetWebDrivers().ConfigureAwait(false);
                int driverCounter = 0;
                for (int i = 0; i < bots.Count; i++)
                {
                    bots[i].WebDriverId = webDrivers[driverCounter].Id;
                    driverCounter++;
                    if (driverCounter >= webDrivers.Count)
                        driverCounter = 0;
                }

                //Start bots

                await SetBotSchedule(BotsId).ConfigureAwait(false);
                if (noBots)
                {
                    for (int i = 0; i < webDrivers.Count; i++)
                    {
                        var j = i;
                        Task.Run(() => { RunBot(webDrivers[j].Id); });
                    }
                }
            }
            catch(Exception ex)
            {
                result.ExceptionMessage = ex.Message;
                settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }

        public async Task<BotStopQueryReport> StopBot(List<int> BotsId)
        {
            var result = new BotStopQueryReport();
            try
            {

                for (int i = 0; i < BotsId.Count; i++)
                {

                    //Check bot data

                    var bot = bots.FirstOrDefault(item => item.BotData.Id == BotsId[i]);
                    if (bot != null)
                    {

                        //Set bot StopQuery status

                        bots[bots.IndexOf(bot)].WorkStatus = EnumBotWorkStatus.StopQuery;
                        result.BotCount++;
                    }
                    else
                        result.ErrorBotCount++;
                }
            }
            catch (Exception ex)
            {
                result.Exception = ex.Message;
                settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }

        public async Task<List<BotWorkStatusModel>> GetBots()
        {
            try
            {
                if (bots == null)
                    return new List<BotWorkStatusModel>();
            }
            catch (Exception ex)
            {
                settingsService.AddLog("BotWorkService", ex);
            }
            return bots;
        }

        public async Task<List<BotRoleActionsDaySchedule>> GetBotRoleActions()
        {
            return botRoleActions;
        }

        public async Task<List<string>> GetRandomMessages()
        {
            return randomMessages;
        }

        private async Task RunBot(Guid WebDriverId)
        {
            try
            {
                var currentDay = DateTime.Now;
                var webDriverBots = bots.Where(item => item.WebDriverId == WebDriverId).ToList();
                while (webDriverBots.Count > 0)
                {
                    for (int i = 0; i < webDriverBots.Count; i++)
                    {
                        var loginflag = await vkActionService.Login(WebDriverId, webDriverBots[i].BotData.Login, webDriverBots[i].BotData.Password).ConfigureAwait(false);
                        if ((!loginflag.hasError) && (await vkActionService.isLoginSuccess(WebDriverId).ConfigureAwait(false)))
                        {
                            var updateOnlineDateResult = botCompositeService.UpdateOnlineDate(webDriverBots[i].BotData.Id, DateTime.Now);
                            if (updateOnlineDateResult.HasError)
                                await settingsService.AddLog("BotWorkService", updateOnlineDateResult.ExceptionMessage).ConfigureAwait(false);

                            UpdateBotWorkStatus(webDriverBots[i].BotData.Id, EnumBotWorkStatus.Run);
                            if (webDriverBots[i].BotData.isUpdatedCustomizeInfo)
                            {
                                var cusomizeData = botCompositeService.GetBotCustomizeByBotId(webDriverBots[i].BotData.Id);
                                var cusomizeResult = await vkActionService.Customize(WebDriverId, cusomizeData).ConfigureAwait(false);
                                if (!cusomizeResult.hasError)
                                    botCompositeService.SetIsUpdatedCustomizeInfo(webDriverBots[i].BotData.Id, false);
                            }
                            var botSchedule = new List<EnumBotActionType>();
                            var botData = bots.FirstOrDefault(item => item.BotData.Id == webDriverBots[i].BotData.Id);
                            var nowTime = DateTime.Now;
                            var startTime = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, 8, 0, 0);
                            var endTime = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, 23, 0, 0);
                            var maxSecondActionsCount = random.Next(1, 4);
                            if ((nowTime > startTime) && (nowTime < endTime))
                            {
                                var maxClientsRoleConnectionActions = botRoleActions.FirstOrDefault(item => item.BotId == botData.BotData.Id);
                                if ((maxClientsRoleConnectionActions != null) && (maxClientsRoleConnectionActions.RoleActionCount > 0))
                                {
                                    var maxClientsRoleConnectionActionsCount = random.Next(1, 2);
                                    maxClientsRoleConnectionActions.RoleActionCount -= maxClientsRoleConnectionActionsCount;
                                    botRoleActions[botRoleActions.IndexOf(maxClientsRoleConnectionActions)] = maxClientsRoleConnectionActions;
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
                            await CheckMessage(WebDriverId, webDriverBots[i].BotData.Id).ConfigureAwait(false);
                            for (int j = 0; j < botSchedule.Count; j++)
                            {
                                var isActionError = false;
                                switch (botSchedule[j])
                                {
                                    case EnumBotActionType.ListenMusic:
                                        await ListenMusic(WebDriverId, webDriverBots[i].BotData.Id).ConfigureAwait(false);
                                        break;
                                    case EnumBotActionType.WatchVideo:
                                        await WatchVideo(WebDriverId, webDriverBots[i].BotData.Id).ConfigureAwait(false);
                                        break;
                                    case EnumBotActionType.News:
                                        await vkActionService.News(WebDriverId).ConfigureAwait(false);
                                        break;
                                    case EnumBotActionType.RoleMission:
                                        var setBotClientsRoleConnection = clientCompositeService.SetClientToBot(webDriverBots[i].BotData.Id);
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
                                {
                                    UpdateBotWorkStatus(webDriverBots[i].BotData.Id, EnumBotWorkStatus.Error);
                                    break;
                                }
                                await CheckMessage(WebDriverId, webDriverBots[i].BotData.Id).ConfigureAwait(false);
                                botData = bots.FirstOrDefault(item => item.BotData.Id == webDriverBots[i].BotData.Id);
                                if ((botData != null) && (botData.WorkStatus == EnumBotWorkStatus.StopQuery))
                                    break;
                            }
                            if ((botData != null) && (botData.WorkStatus == EnumBotWorkStatus.StopQuery))
                                UpdateBotWorkStatus(webDriverBots[i].BotData.Id, EnumBotWorkStatus.Stop);
                            else
                                UpdateBotWorkStatus(webDriverBots[i].BotData.Id, EnumBotWorkStatus.Free);
                            var logoutresult = await Logout(WebDriverId).ConfigureAwait(false);
                            if (!logoutresult)
                                UpdateBotWorkStatus(webDriverBots[i].BotData.Id, EnumBotWorkStatus.Error);
                        }
                        else
                        {
                            UpdateBotWorkStatus(webDriverBots[i].BotData.Id, EnumBotWorkStatus.Error);
                        }
                    }
                    webDriverBots = bots.Where(item => item.WebDriverId == WebDriverId && item.WorkStatus == EnumBotWorkStatus.Free).ToList();
                    if (currentDay > DateTime.Now)
                    {
                        currentDay = DateTime.Now;
                        await SetBotSchedule(webDriverBots.Select(item => item.BotData.Id).ToList()).ConfigureAwait(false);
                        randomMessages = new List<string>();
                        for (int i = 0; i < webDriverBots.Count; i++)
                        {
                            var deleteDay = DateTime.Now;
                            deleteDay = deleteDay.AddDays(-2);
                            var screenshots = dialogScreenshotService.GetByBotId(webDriverBots[i].BotData.Id, false);
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
                    }
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("BotWorkService", ex);
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

        private void UpdateBotWorkStatus(int BotId, EnumBotWorkStatus NewStatus)
        {
            try
            {
                var botData = bots.FirstOrDefault(item => item.BotData.Id == BotId);
                if (botData != null)
                    bots[bots.IndexOf(botData)].WorkStatus = NewStatus;
            }
            catch (Exception ex)
            {
                settingsService.AddLog("BotWorkService", ex);
            }
        }

        private async Task ListenMusic(Guid WebDriverId, int BotId)
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
                var stopResult = await vkActionService.StopMusic(WebDriverId, hasBotMusic).ConfigureAwait(false);
                if ((!stopResult.hasError) && (stopResult.ActionResultMessage == EnumActionResult.Success) && (!hasBotMusic))
                    botCompositeService.CreateBotMusic(BotId, music.Artist, music.SongName);
            }
        }

        private async Task WatchVideo(Guid WebDriverId, int BotId)
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
                                var stopVideo = await vkActionService.CloseVideo(WebDriverId).ConfigureAwait(false);
                                botCompositeService.CreateBotVideos(BotId, word[0].Id, videos[randomVideoIndex].URL);
                            }
                        }
                    }
                }
            }
        }

        private async Task<bool> ExecuteRoleMission(Guid WebDriverId, BotClientRoleConnectorModel BotClientRoleConnector)
        {
            try
            {
                var client = clientCompositeService.GetClientById(BotClientRoleConnector.ClientId);
                if (client != null)
                {
                    var currentMission = missionCompositeService.GetRoleMissionConnections(BotClientRoleConnector.RoleId, true);
                    if ((currentMission != null) && (currentMission.Count > 0))
                    {
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
                                            if (OnlyInteger(client.VkId))
                                                stepResult = await vkActionService.GoToProfile(WebDriverId, "id" + client.VkId).ConfigureAwait(false);
                                            else
                                                stepResult = await vkActionService.GoToProfile(WebDriverId, client.VkId).ConfigureAwait(false);
                                            if (stepResult)
                                            {
                                                client.FullName = await vkActionService.GetClientName(WebDriverId);
                                                clientCompositeService.UpdateClientData(client);
                                            }
                                            break;
                                        case EnumMissionActionType.GoToGroup:
                                            stepResult = await vkActionService.GoToProfile(WebDriverId, nodes[i].Text).ConfigureAwait(false);
                                            break;
                                        case EnumMissionActionType.AvatarLike:
                                            var avatarResult = await vkActionService.AvatarLike(WebDriverId).ConfigureAwait(false);
                                            stepResult = !avatarResult.hasError;
                                            break;
                                        case EnumMissionActionType.NewsLike:
                                            var newsLikeResult = await vkActionService.NewsLike(WebDriverId, EnumNewsLikeType.LikeFirstNews).ConfigureAwait(false);
                                            stepResult = !newsLikeResult.hasError;
                                            break;
                                        case EnumMissionActionType.Subscribe:
                                            var subscribeResult = await vkActionService.Subscribe(WebDriverId).ConfigureAwait(false);
                                            stepResult = !subscribeResult.hasError;
                                            break;
                                        case EnumMissionActionType.SubscribeToGroup:
                                            var subscribeToGroupResult = await vkActionService.SubscribeToGroup(WebDriverId).ConfigureAwait(false);
                                            stepResult = !subscribeToGroupResult.hasError;
                                            break;
                                        case EnumMissionActionType.Repost:
                                            var repostResult = await vkActionService.Repost(WebDriverId, EnumRepostType.First).ConfigureAwait(false);
                                            stepResult = !repostResult.hasError;
                                            break;
                                        case EnumMissionActionType.SendMessage:
                                            var newMessage = await RandOriginalMessageAsync(nodes[i].Text).ConfigureAwait(false);
                                            if (newMessage != null)
                                            {
                                                var sendMessageResult = await vkActionService.SendFirstMessage(WebDriverId, newMessage).ConfigureAwait(false);
                                                stepResult = !sendMessageResult.hasError;
                                                if (stepResult)
                                                {
                                                    clientCompositeService.CreateMessage(BotClientRoleConnector.Id, newMessage);
                                                    
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
                settingsService.AddLog("BotWorkService", ex);
            }
            return false;
        }

        private async Task<bool> CheckMessage(Guid WebDriverId, int BotId)
        {
            try
            {
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
                    for (int i = 0; i < botDialogsWithNewBotMessages.Count; i++)
                    {
                        var client = clientCompositeService.GetClientById(botDialogsWithNewBotMessages[i].ClientId);
                        if (client != null)
                        {
                            var goToDialogResult = await vkActionService.GoToDialog(WebDriverId, client.VkId).ConfigureAwait(false);
                            if (!goToDialogResult.hasError)
                            {
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
                                        var newBotMessage = await RandOriginalMessageAsync(messages[j].Text).ConfigureAwait(false);
                                        if(newBotMessage != null)
                                            sendAnswerMessageResult = await vkActionService.SendAnswerMessage(WebDriverId, newBotMessage, client.VkId, botDialogsWithNewBotMessages[i].Id).ConfigureAwait(false);
                                    }
                                    if (!sendAnswerMessageResult.hasError)
                                    {
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
                settingsService.AddLog("BotWorkService", ex);
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
                            var botMessage = await RandOriginalMessageAsync(standartPatterns[i].AnswerText).ConfigureAwait(false);
                            if (botMessage != null)
                            {
                                var sendAnswerMessageResult = await vkActionService.SendAnswerMessage(WebDriverId, botMessage, ClientVkId, botClientRoleConnector.Id).ConfigureAwait(false);
                                var saveResult = await SaveNewMessage(WebDriverId, botClientRoleConnector.Id, !sendAnswerMessageResult.hasError, newMessageText, botMessage).ConfigureAwait(false);
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
                                        var botMessage = await RandOriginalMessageAsync(patternAction[0].Text).ConfigureAwait(false);
                                        if (botMessage != null)
                                        {
                                            var sendAnswerMessageResult = await vkActionService.SendAnswerMessage(WebDriverId, botMessage, ClientVkId, botClientRoleConnector.Id).ConfigureAwait(false);
                                            var saveResult = await SaveNewMessage(WebDriverId, botClientRoleConnector.Id, !sendAnswerMessageResult.hasError, newMessageText, botMessage).ConfigureAwait(false);
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
                                }
                            }
                        }
                    }
                    return await SaveNewMessage(WebDriverId, botClientRoleConnector.Id, false, newMessageText, null).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("BotWorkService", ex);
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
                settingsService.AddLog("BotWorkService", ex);
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
                    var text = TextToRegex(RegexString);
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

        private async Task<string> RandOriginalMessageAsync(string message)
        {
            try
            {
                var maxAttept = random.Next(3, 6);
                for (int i = 0; i < maxAttept; i++)
                {
                    var randomMessage = await RandMessage(message).ConfigureAwait(false);
                    if (randomMessages.FirstOrDefault(item => item == randomMessage) == null)
                    {
                        randomMessages.Add(randomMessage);
                        return randomMessage;
                    }
                }
                return await RandMessage(message).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                settingsService.AddLog("BotWorkService", ex);
            }
            return null;
        }

        private async Task<string> RandMessage(string message)
        {
            try
            {
                var RandomMessageConstantTextes = new List<MessageConstantText>();
                var random = new Random();
                int LastOpen = 0;
                for (int i = 0; i < message.Length; i++)
                {
                    if (message[i] == '(')
                        LastOpen = i;
                    if ((message[i] == ')') && (LastOpen != -1))
                    {
                        var Elements = new List<string>();
                        Elements.Add("");
                        for (int j = LastOpen + 1; j < i; j++)
                        {
                            if (message[j] != '_')
                                Elements[Elements.Count - 1] += message[j];
                            else
                                Elements.Add("");
                        }
                        string oldstring = "(";
                        for (int j = 0; j < Elements.Count; j++)
                        {
                            oldstring += Elements[j] + "_";
                        }
                        oldstring = oldstring.Remove(oldstring.Length - 1);
                        oldstring += ")";
                        if (Elements.Count > 1)
                            message = message.Replace(oldstring, Elements[random.Next(0, Elements.Count)]);
                        else
                        {
                            var randomMessageConstantText = new MessageConstantText()
                            {
                                TextGuid = Guid.NewGuid(),
                                Text = $"({Elements[0]})"
                            };
                            RandomMessageConstantTextes.Add(randomMessageConstantText);
                            message = message.Replace(oldstring, randomMessageConstantText.TextGuid.ToString());
                        }
                        i = -1;
                    }
                }
                for (int i = 0; i < RandomMessageConstantTextes.Count; i++)
                    message = message.Replace(RandomMessageConstantTextes[i].TextGuid.ToString(), RandomMessageConstantTextes[i].Text);
                var isSetedMessageKeyboardError = false;
                var errorChancePerTenWords = settingsService.GetServerSettings().ErrorChancePerTenWords;
                if (random.Next(0, 100) < ((message.Split(' ').Length / 10) * errorChancePerTenWords))
                {
                    isSetedMessageKeyboardError = true;
                    message = await SetMessageKeyboardError(message).ConfigureAwait(false);
                }
                message = await SetCaps(message, isSetedMessageKeyboardError).ConfigureAwait(false);
                message = await ReplaceNumberToWord(message).ConfigureAwait(false);
                message = RemoveBrackets(message);
                return message;
            }
            catch (Exception ex)
            {
                settingsService.AddLog("BotWorkService", ex);
            }
            return null;
        }
        
        private string TextToRegex(string Text)
        {
            try
            {
                var regexstres = Text.Split('\n');
                Text = "";
                for (int k = 0; k < regexstres.Length; k++)
                {
                    if (regexstres[k].Length > 0)
                    {
                        Text += "\\b" + regexstres[k] + "\\b|";
                    }
                }
                if (Text.Length > 0)
                    Text = Text.Remove(Text.Length - 1);
                return Text;
            }
            catch (Exception ex)
            {
                settingsService.AddLog("BotWorkService", ex);
            }
            return null;
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
                settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }

        private async Task SetBotSchedule(List<int> BotsId)
        {
            try
            {
                var ourRoleActions = BotsId.Count * random.Next(8, 13);
                for (int i = 0; i < BotsId.Count; i++)
                {
                    var actionsCount = random.Next(8, 13);
                    if (actionsCount > (ourRoleActions - actionsCount))
                    {
                        if (ourRoleActions <= 10)
                            ourRoleActions = random.Next(10, 13);
                        actionsCount = random.Next(8, ourRoleActions);
                    }
                    var connection = botRoleActions.FirstOrDefault(item => item.BotId == BotsId[i]);
                    if (connection != null)
                        botRoleActions[botRoleActions.IndexOf(connection)].RoleActionCount = actionsCount;
                    else
                    {
                        botRoleActions.Add(new BotRoleActionsDaySchedule()
                        {
                            BotId = BotsId[i],
                            RoleActionCount = actionsCount
                        });
                    }
                    ourRoleActions -= actionsCount;
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("BotWorkService", ex);
            }
        }

        private async Task<string> SetMessageKeyboardError(string Text)
        {
            var result = Text;
            try
            {
                var constantText = new List<MessageConstantText>();
                constantText = GetConstantTexts(Text, new Regex(@"(\(\w*\))"));
                Text = ReplaceConstantStringToKey(Text, constantText);
                var words = Text.Split(" ");
                var settingsErrorChancePerTenWords = settingsService.GetServerSettings().ErrorChancePerTenWords;
                char[][] keyboard = new char[3][]
                    {
                    new char[12] { 'й', 'ц', 'у', 'к', 'е', 'н', 'г', 'ш', 'щ', 'з', 'х', 'ъ' },
                    new char[11] { 'ф', 'ы', 'в', 'а', 'п', 'р', 'о', 'л', 'д', 'ж', 'э' },
                    new char[9] { 'я', 'ч', 'с', 'м', 'и', 'т', 'ь', 'б', 'ю' }
                    };
                var wordsIndexes = new List<int>();
                var regex = new Regex(@"\b[А-Яа-я]+\b", RegexOptions.IgnoreCase);
                if (words.Length >= 10)
                {
                    for (int i = 0; i < words.Length; i++)
                    {
                        if ((i > 0) && (i % 10 == 0) && (random.Next(0, 100) < settingsErrorChancePerTenWords))
                        {
                            var randomIndex = random.Next(i - 10, i);
                            if (regex.IsMatch(words[randomIndex]))
                                wordsIndexes.Add(randomIndex);
                        }
                    }
                }
                else if(random.Next(0, 100) < words.Length / settingsErrorChancePerTenWords)
                {
                    var randomIndex = random.Next(0, words.Length);
                    if (regex.IsMatch(words[randomIndex]))
                        wordsIndexes.Add(randomIndex);
                }
                for (int i = 0; i < wordsIndexes.Count; i++)
                {
                    if (regex.IsMatch(words[wordsIndexes[i]]))
                    {
                        var word = words[wordsIndexes[i]];
                        var characterIndex = random.Next(0, word.Length - 1);
                        for (int j = 0; j < keyboard.Length; j++)
                        {
                            for (int k = 0; k < keyboard[j].Length; k++)
                            {
                                if (word[characterIndex] == keyboard[j][k])
                                {
                                    var x = 0;
                                    var y = 0;
                                    if ((j + k) == 0)
                                    {
                                        x = random.Next(0, 2);
                                        y = random.Next(0, 2);
                                        if ((y == 0) && (x == 0))
                                            x = 1;
                                    }
                                    else if ((j == 0) && (k == 11))
                                    {
                                        y = random.Next(0, 2);
                                        x = 10;
                                    }
                                    else if ((j == 1) && (k == 0))
                                    {
                                        x = random.Next(0, 2);
                                        y = random.Next(0, 3);
                                    }
                                    else if ((j == 1) && (k == 10))
                                    {
                                        x = random.Next(9, 12);
                                        y = random.Next(0, 2);
                                        if ((y == 1) && (x != 9))
                                            x = 9;
                                    }
                                    else if ((j == 1) && (k == 9))
                                    {
                                        x = random.Next(8, 11);
                                        y = random.Next(0, 3);
                                        if ((y == 2) && (x > 8))
                                            x = 8;
                                    }
                                    else if ((j == 2) && (k == 0))
                                    {
                                        y = random.Next(1, 3);
                                        x = random.Next(0, 2);
                                    }
                                    else if ((j == 2) && (k == 8))
                                    {
                                        y = random.Next(1, 3);
                                        x = random.Next(7, 10);
                                        if (y == 2)
                                            x = 7;
                                    }
                                    else if (j == 0)
                                    {
                                        y = random.Next(0, 2);
                                        x = random.Next(k - 1, k + 2);
                                    }
                                    else if (j == 2)
                                    {
                                        y = random.Next(1, 3);
                                        x = random.Next(k - 1, k + 2);
                                    }
                                    else
                                    {
                                        y = random.Next(0, 3);
                                        x = random.Next(k - 1, k + 2);
                                    }
                                    StringBuilder sb = new StringBuilder(word);
                                    sb[characterIndex] = keyboard[y][x];
                                    word = sb.ToString();
                                }
                            }
                        }
                        words[wordsIndexes[i]] = word;
                    }
                }
                result = ConstructMessage(words.ToList());
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }

        private async Task<string> SetCaps(string Text, bool isSetedMessageKeyboardError)
        {
            var result = Text;
            try
            {
                var constantText = GetConstantTexts(Text, new Regex(@"(\(\w*\))"));
                Text = ReplaceConstantStringToKey(Text, constantText);
                var words = Text.Split(" ");
                var settingsCapsChancePerThousandWords = settingsService.GetServerSettings().CapsChancePerThousandWords;
                if (isSetedMessageKeyboardError)
                    settingsCapsChancePerThousandWords /= 2;
                if (settingsCapsChancePerThousandWords < 1)
                    settingsCapsChancePerThousandWords = 1;
                var regex = new Regex(@"\b[А-Яа-я]+\b");
                if (words.Length >= 100)
                {
                    for (int i = 0; i < words.Length; i++)
                    {
                        if ((i > 0) && (i % 100 == 0) && (random.Next(0, 100) < settingsCapsChancePerThousandWords))
                        {
                            var randomIndex = random.Next(0, words.Length);
                            if (regex.IsMatch(words[randomIndex]))
                                words[randomIndex] = words[randomIndex].ToUpper();
                        }
                    }
                }
                else if(random.Next(0, 100) < words.Length / settingsCapsChancePerThousandWords)
                {
                    var randomIndex = random.Next(0, words.Length);
                    if (regex.IsMatch(words[randomIndex]))
                        words[randomIndex] = words[randomIndex].ToUpper();
                }
                result = ConstructMessage(words.ToList());
            }
            catch (Exception ex)
            {
                settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }
        
        private async Task<string> ReplaceNumberToWord(string Text)
        {
            var result = Text;
            try
            {
                var constantText = GetConstantTexts(Text, new Regex(@"(\(\w*\))"));
                var textNumbers = GetConstantTexts(Text, new Regex(@"\b[0-9]{12}\b"));
                Text = ReplaceConstantStringToKey(Text, constantText);
                Text = ReplaceConstantStringToKey(Text, textNumbers);
                if (textNumbers.Count > 0)
                {
                    var numberChancePerHundredWords = settingsService.GetServerSettings().NumberChancePerHundredWords;
                    string[,] NumArray = {
                        {"один", "два", "три", "четыре", "пять", "шесть", "семь", "восемь", "девять"},
                        {"десять", "двадцать", "тридцать", "сорок", "пятьдесят", "шестьдесят", "семьдесят", "восемьдесят", "девяносто"},
                        {"сто", "двести", "триста", "четыреста", "пятьсот", "шестьсот", "семьсот", "восемьсот", "девятьсот"},
                        {"одиннадцать", "двенадцать", "тринадцать", "четырнадцать", "пятнадцать", "шестнадцать", "семнадцать", "восемнадцать", "девятнадцать" } };
                    string[,] TenArray = {
                        {"тысяча", "тысячи", "тысяч"},
                        {"миллион", "миллиона", "миллионов"},
                        {"миллиард", "миллиарда", "миллиардов"}};
                    for (int i = 0; i < numberChancePerHundredWords && textNumbers.Count > 0; i++)
                    {
                        var randomIndex = random.Next(0, textNumbers.Count);
                        var words = new List<string>();
                        for (int j = 0; j < textNumbers[randomIndex].Text.Length; j++)
                        {
                            var intValue = -1;
                            if ((int.TryParse(textNumbers[randomIndex].Text[j].ToString(), out intValue)) && (intValue > 0))
                            {
                                var numI = 2 - (j - (j / 3) * 3);
                                var tenI = 2 - (j / 3);
                                var tenJ = 0;
                                if ((numI == 0) && (words.Count > 0) && (words[words.Count - 1] == "десять"))
                                {
                                    words[words.Count - 1] = NumArray[3, intValue - 1];
                                    tenJ = 2;
                                }
                                else
                                {
                                    words.Add(NumArray[numI, intValue - 1]);
                                    if ((tenI == 0) && (numI == 0))
                                    {
                                        if (intValue == 1)
                                            words[words.Count - 1] = "одна";
                                        else if (intValue == 2)
                                            words[words.Count - 1] = "две";
                                    }
                                }
                                if ((j <= textNumbers[randomIndex].Text.Length - 3) && ((j + 1) % 3 == 0))
                                {
                                    if ((intValue >= 2) && (intValue <= 4) && (numI < 1))
                                        tenJ = 1;
                                    else if (intValue >= 5)
                                        tenJ = 2;
                                    words.Add(TenArray[tenI, tenJ]);
                                }
                            }
                        }
                        Text = Text.Replace(textNumbers[i].TextGuid.ToString(), String.Join(' ', words));
                        textNumbers.RemoveAll(item => item.TextGuid == textNumbers[i].TextGuid);
                    }
                }
                Text = ConstructMessage(Text, constantText);
                Text = ConstructMessage(Text, textNumbers);
                return Text;
            }
            catch(Exception ex)
            {
                settingsService.AddLog("BotWorkService", ex);
            }
            return result;
        }

        private List<MessageConstantText> GetConstantTexts(string Text, Regex RegulerExtension)
        {
            var result = new List<MessageConstantText>();
            var matches = RegulerExtension.Matches(Text);
            for (int i = 0; i < matches.Count; i++)
            {
                if (matches[i].Length > 0)
                {
                    var messageConstantText = new MessageConstantText()
                    {
                        Text = matches[i].Value,
                        TextGuid = Guid.NewGuid()
                    };
                    result.Add(messageConstantText);
                }
            }
            return result;
        }

        private string ReplaceConstantStringToKey(string Text, List<MessageConstantText> ConstantTexts)
        {
            for (int i = 0; i < ConstantTexts.Count; i++)
            {
                var regex = new Regex(@ConstantTexts[i].Text);
                Text = regex.Replace(Text, ConstantTexts[i].TextGuid.ToString(), 1);
            }
            return Text;
        }

        private string ConstructMessage(string Text, List<MessageConstantText> ConstantTexts)
        {
            for (int i = 0; i < ConstantTexts.Count; i++)
                Text = Text.Replace(ConstantTexts[i].TextGuid.ToString(), ConstantTexts[i].Text);
            return Text;
        }

        private string ConstructMessage(List<string> Words)
        {
            var result = "";
            for (int i = 0; i < Words.Count; i++)
                result += Words[i] + " ";
            result = result.Remove(result.Length - 1);
            return result;
        }

        private string RemoveBrackets(string Text)
        {
            Text = Text.Replace("(", "");
            Text = Text.Replace(")", "");
            return Text;
        }

        public async Task<string> Test(string Text)
        {
            return await RandMessage(Text).ConfigureAwait(false);
        }
    }
}