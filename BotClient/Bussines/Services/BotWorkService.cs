﻿using BotClient.Bussines.Interfaces;
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
                            WebDriverId = browsers[i].Id
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

                        if (!bot.isLogin)
                            botCompositeService.UpdatePassword(botId, await botActionService.GenerateAndUpdatePassword(WebDriverId, bot.Password).ConfigureAwait(false));

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
                        {
                            bot.FullName = await botActionService.GetBotFullName(WebDriverId).ConfigureAwait(false);
                            if (bot.FullName.Length > 0)
                                botCompositeService.UpdateFullName(botId, bot.FullName);
                        }

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


                        await Chill(WebDriverId, botId, false).ConfigureAwait(false);


                        var currentDay = DateTime.Now;
                        var startTime = new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, 8, 0, 0);
                        var endTime = new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, 23, 0, 0);

                        int roleActionCount = random.Next(settings.MinRoleActionCountPerDay, settings.MaxRoleActionCountPerDay);
                        var complitedRoleActions = clientCompositeService.GetBotClientRoleConnectionsByBotId(botId);
                        complitedRoleActions.RemoveAll(item => item.UpdateDate < currentDay);
                        roleActionCount -= complitedRoleActions.Count(item => item.isSuccess);
                        var missions = missionCompositeService.GetRoleMissionConnections(RoleId, true);
                        
                        while ((DateTime.Now > startTime) && (DateTime.Now < endTime) && (roleActionCount > 0))
                        {
                            botCompositeService.CreateBotActionHistory(bot.Id, EnumBotActionType.RoleMission, $"Начало выполнение роли");

                            var botRoleNews = missionCompositeService.GetBotNewsMissionConnectionsByBotId(RoleId, botId);
                            var botNewsCount = random.Next(settings.MinNewsRoleActions, settings.MaxNewsRoleActions);
                            botNewsCount -= botRoleNews.Count(item => item.UpdateDate > currentDay);
                            if (botNewsCount > 0)
                            {
                                var botNewsInWaiting = botRoleNews.FirstOrDefault(item => (item.isWaiting) && (!item.isComplete));
                                if (botNewsInWaiting == null)
                                {
                                    var freeNews = missionCompositeService.GetBotNewsMissionConnectionsByBotId(RoleId, -1, null, true);
                                    freeNews.RemoveAll(item => item.isDelayed && (endTime < item.CreateDate.AddDays(item.NextDateCommentInDays)));
                                    for (int i = 0; i < freeNews.Count; i++)
                                    {
                                        if (botRoleNews.FirstOrDefault(item => item.MissionId == freeNews[i].MissionId) == null)
                                        {
                                            await ExecuteNewsMission(WebDriverId, freeNews[i], botId).ConfigureAwait(false);
                                            break;
                                        }
                                    }
                                }
                                else
                                    await ExecuteNewsMission(WebDriverId, botNewsInWaiting, botId).ConfigureAwait(false);

                            }

                            var roleAttept = random.Next(settings.MinRoleAtteptCount, settings.MaxRoleAtteptCount);
                            while (roleAttept > 0)
                            {
                                var setBotClientsRoleConnection = clientCompositeService.SetClientToBot(bot.Id, missions[random.Next(0, missions.Count)].MissionId);
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

                            if (roleActionCount > 0)
                                await Chill(WebDriverId, botId, bot.isSkipSecondAction).ConfigureAwait(false);
                        }
                        if (roleActionCount <= 0)
                            await Chill(WebDriverId, botId, bot.isSkipSecondAction).ConfigureAwait(false);
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
                                                        //await vkActionService.CheckIsSended(WebDriverId, client.VkId, BotClientRoleConnector.RoleId, BotClientRoleConnector.Id, 50 < random.Next(0, 100)).ConfigureAwait(false);
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

        public async Task ExecuteNewsMission(Guid WebDriverId, BotNewsMissionConnectorModel BotNewsMissionConnection, int BotId)
        {
            try
            {
                var vkNews = await botActionService.GetVkNewPost(WebDriverId, BotNewsMissionConnection.VkLink).ConfigureAwait(false);
                if (BotNewsMissionConnection.BotId == -1)
                    missionCompositeService.SetBotNewsMissionConnectionBotId(BotNewsMissionConnection.Id, BotId);
                BotNewsMissionConnection = missionCompositeService.GetBotNewsMissionConnectionsById(BotNewsMissionConnection.Id);
                if (BotNewsMissionConnection.BotId == BotId)
                {
                    var missionNodes = missionCompositeService.GetNodes(BotNewsMissionConnection.MissionId, BotNewsMissionConnection.NodeId);
                    for (int i = 0; i < missionNodes.Count; i++)
                    {
                        if (missionNodes[i].Type == EnumMissionActionType.SendMessageToPost)
                        {
                            if (missionNodes[i].Text.Length < 1)
                                missionNodes[i].Text = BotNewsMissionConnection.SendedText;
                            missionNodes[i].Text = await textService.RandMessage(missionNodes[i].Text).ConfigureAwait(false);
                            var sendResult = await vkActionService.SendMessageToPostNews(missionNodes[i].Text, vkNews.CommentInput, vkNews.SendBtn);
                            if (sendResult)
                            {
                                missionCompositeService.SetBotNewsMissionConnectionSendedText(BotNewsMissionConnection.Id, missionNodes[i].Text);
                                missionCompositeService.SetBotNewsMissionConnectionIsComplete(BotNewsMissionConnection.Id, true);
                                missionCompositeService.SetBotNewsMissionConnectionIsWaiting(BotNewsMissionConnection.Id, false);
                                var missionPath = missionNodes[i].Path;
                                if (missionPath.Length > 0)
                                    missionPath += ";";
                                missionPath += missionNodes[i].NodeId;
                                var nextNodes = missionCompositeService.GetNodes(BotNewsMissionConnection.MissionId, null, missionPath);

                                while (nextNodes.Count > 0)
                                {
                                    switch (nextNodes[0].Type)
                                    {
                                        case EnumMissionActionType.LikeNewsPost:
                                            await vkActionService.LikePostNews(WebDriverId, vkNews.NewsVkId).ConfigureAwait(false);
                                            break;
                                        default:
                                            break;
                                    }
                                    missionPath += ";" + nextNodes[0].NodeId;
                                    nextNodes = missionCompositeService.GetNodes(BotNewsMissionConnection.MissionId, null, missionPath);

                                    if ((nextNodes.Count > 0) && (nextNodes[0].Type == EnumMissionActionType.SetNextNewsStep))
                                    {
                                        var nextRoleType = -1;
                                        if (int.TryParse(nextNodes[0].Text, out nextRoleType))
                                        {
                                            var usedBotId = -1;
                                            var usedRoleTypesInNews = missionCompositeService.GetBotNewsMissionConnectionsByBotRoleType(nextRoleType, nextNodes[0].MissionId, BotNewsMissionConnection.NewsId);
                                            if (usedRoleTypesInNews.Count > 0)
                                                usedBotId = usedRoleTypesInNews[0].BotId;
                                            nextNodes = missionCompositeService.GetNodes(BotNewsMissionConnection.MissionId, null, missionPath + ";" + nextNodes[0].NodeId);
                                            if ((nextNodes.Count > 0) && (nextNodes[0].Type == EnumMissionActionType.SendMessageToPost))
                                            {
                                                missionCompositeService.CreateBotNewsMissionConnection(BotNewsMissionConnection.RoleId, BotNewsMissionConnection.MissionId, BotNewsMissionConnection.NewsId,
                                                                                                   usedBotId, nextRoleType, BotNewsMissionConnection.VkLink, nextNodes[0].NodeId,
                                                                                                   BotNewsMissionConnection.NextDateCommentInDays, true, false, BotNewsMissionConnection.isDelayed);
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
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

        private async Task Chill(Guid WebDriverId, int BotId, bool IsSkipSecondAction)
        {
            try
            {
                var settings = settingsService.GetServerSettings();
                var nextTime = DateTime.Now.AddMinutes(random.Next(settings.MinRoleWaitingTimeInMinutes, settings.MaxRoleWaitingTimeInMinutes));
                var currentDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                var chillActionCount = 1;

                if ((DateTime.Now.Hour < 8) && (DateTime.Now.Hour > 23))
                    chillActionCount = random.Next(settings.MinNightSecondActionCount, settings.MaxNightSecondActionCount);

                for (int i = 0; i < chillActionCount; i++)
                {
                    while (DateTime.Now < nextTime)
                    {
                        if (!IsSkipSecondAction)
                        {
                            var waitAction = EnumBotActionType.News;//(EnumBotActionType)random.Next(1, 4);
                            int startDialogCount = await vkActionService.GetNewDialogsCount(WebDriverId).ConfigureAwait(false);
                            switch (waitAction)
                            {
                                case EnumBotActionType.ListenMusic:
                                    botCompositeService.CreateBotActionHistory(BotId, EnumBotActionType.ListenMusic, $"Переход к музыкальному каталогу");
                                    var botMusic = botCompositeService.GetBotMusic(BotId);
                                    var botVkMusic = await botActionService.StartMusic(WebDriverId, botMusic).ConfigureAwait(false);
                                    if (botVkMusic != null)
                                    {
                                        botCompositeService.CreateBotMusic(BotId, botVkMusic.Artist, botVkMusic.SongName);
                                        await botActionService.StopMusic(WebDriverId, startDialogCount).ConfigureAwait(false);
                                    }
                                    break;
                                case EnumBotActionType.WatchVideo:
                                    botCompositeService.CreateBotActionHistory(BotId, EnumBotActionType.WatchVideo, $"Переход к видеокаталогу");
                                    var searchWordId = botActionService.GetSearchVideoWordId(botCompositeService.GetBotVideos(BotId), videoDictionaryService.GetMaxId());
                                    var searchWords = videoDictionaryService.GetAll(searchWordId, 1);
                                    var botVkVideo = await botActionService.StartVideo(WebDriverId, searchWords).ConfigureAwait(false);
                                    if (botVkVideo != null)
                                    {
                                        if (random.Next(0, 100) < settings.VideoAddChancePerDay)
                                            botVkVideo.BotVideo.isAdded = true;
                                        if (random.Next(0, 100) < settings.VideoSubscribePerDay)
                                            botVkVideo.BotVideo.isSubscribed = true;
                                        botCompositeService.CreateBotVideos(BotId, searchWords[0].Id, botVkVideo.BotVideo.URL, botVkVideo.BotVideo.isAdded, botVkVideo.BotVideo.isSubscribed);
                                        await botActionService.StopVideo(WebDriverId, botVkVideo, startDialogCount).ConfigureAwait(false);
                                    }
                                    break;
                                case EnumBotActionType.News:
                                    botCompositeService.CreateBotActionHistory(BotId, EnumBotActionType.WatchVideo, $"Просмотр новостей");
                                    var botNews = botCompositeService.GetBotNews(BotId);
                                    var botVkNews = await botActionService.StartReadNews(WebDriverId, botNews).ConfigureAwait(false);
                                    botNews.RemoveAll(item => item.UpdateDate < currentDay);
                                    if (botVkNews != null)
                                    {
                                        if (random.Next(0, 100) < settings.LikeChancePerDay)
                                            botVkNews.BotNews.isLiked = true;
                                        if (random.Next(0, 100) < settings.RepostChancePerDay)
                                            botVkNews.BotNews.isReposted = true;
                                        if (random.Next(0, 100) < settings.SubscribeChancePerDay)
                                            botVkNews.BotNews.isSubscribe = true;
                                        await botActionService.StopReadNews(WebDriverId, botVkNews, startDialogCount).ConfigureAwait(false);
                                    }
                                    break;
                            }
                        }
                    }
                    await CheckMessage(WebDriverId, BotId).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("BotWorkService", ex);
            }
        }

        public async Task<List<string>> Test(string Text)
        {
            
            return new List<string>();
        }
    }
}