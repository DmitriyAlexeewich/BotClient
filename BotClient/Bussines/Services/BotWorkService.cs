using BotClient.Bussines.Interfaces;
using BotClient.Bussines.Interfaces.Composite;
using BotClient.Models.Bot;
using BotClient.Models.Bot.Enumerators;
using BotClient.Models.Bot.Work;
using BotClient.Models.Bot.Work.Enumerators;
using BotClient.Models.Client;
using BotClient.Models.Enumerators;
using BotClient.Models.HTMLElements.Enumerators;
using BotClient.Models.Role;
using BotClient.Models.Role.Enumerators;
using BotClient.Models.WebReports;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public BotWorkService(IBotCompositeService BotCompositeService,
                              IClientCompositeService ClientCompositeService,
                              IMissionCompositeService MissionCompositeService,
                              IWebDriverService WebDriverService,
                              ISettingsService SettingsService,
                              IVkActionService VkActionService)
        {
            botCompositeService = BotCompositeService;
            clientCompositeService = ClientCompositeService;
            missionCompositeService = MissionCompositeService;
            webDriverService = WebDriverService;
            settingsService = SettingsService;
            vkActionService = VkActionService;
        }

        private List<BotWorkStatusModel> bots = new List<BotWorkStatusModel>();
        private Random random = new Random();

        public async Task<BotStartReport> StartBot(List<int> BotsId, int RoleId)
        {
            var result = new BotStartReport();
            for (int i = 0; i < BotsId.Count; i++)
            {
                var bufferBotData = await botCompositeService.GetBotById(BotsId[i]).ConfigureAwait(false);
                if ((!bufferBotData.isDead) && (!bufferBotData.isPrintBlock))
                {
                    bots.Add(new BotWorkStatusModel()
                    {
                        BotData = bufferBotData,
                        WorkStatus = EnumBotWorkStatus.Free
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
            var webDrivers = await webDriverService.GetWebDrivers().ConfigureAwait(false);
            int driverCounter = 0;
            for (int i = 0; i < bots.Count; i++)
            {
                bots[i].WebDriverId = webDrivers[driverCounter].Id;
                driverCounter++;
                if (driverCounter >= webDrivers.Count)
                    driverCounter = 0;
            }
            for (int i = 0; i < webDrivers.Count; i++)
                RunBot(webDrivers[i].Id);
            return result;
        }

        public async Task<BotStopQueryReport> StopBot(List<int> BotsId)
        {
            var result = new BotStopQueryReport();
            for (int i = 0; i < BotsId.Count; i++)
            {
                var bot = bots.FirstOrDefault(item => item.BotData.Id == BotsId[i]);
                if (bot != null)
                {
                    bot.WorkStatus = EnumBotWorkStatus.StopQuery;
                    result.Success++;
                }
                else
                    result.Error++;
            }
            return result;
        }

        public async Task<List<BotWorkStatusModel>> GetBots()
        {
            return bots;
        }

        private async Task RunBot(Guid WebDriverId)
        {
            var webDriver = await webDriverService.GetWebDriverById(WebDriverId);
            await webDriverService.SetWebDriverStatus(WebDriverId, EnumWebDriverStatus.Blocked).ConfigureAwait(false);
            var webDriverBots = bots.Where(item => item.WebDriverId == WebDriverId).ToList();
            while (webDriverBots.Count > 0)
            {
                for (int i = 0; i < webDriverBots.Count; i++)
                {
                    var loginflag = await vkActionService.Login(WebDriverId, webDriverBots[i].BotData.Login, webDriverBots[i].BotData.Password).ConfigureAwait(false);
                    if (!loginflag.hasError)
                    {
                        UpdateBotWorkStatus(webDriverBots[i].BotData.Id, EnumBotWorkStatus.Run);
                        if (!webDriverBots[i].BotData.isUpdatedCusomizeInfo)
                        {
                            var cusomizeData = await botCompositeService.GetBotCustomize(webDriverBots[i].BotData.Id).ConfigureAwait(false);
                            var cusomizeResult = await vkActionService.Customize(WebDriverId, cusomizeData).ConfigureAwait(false);
                            if (!cusomizeResult.hasError)
                                await botCompositeService.SetIsUpdatedCusomizeInfo(webDriverBots[i].BotData.Id, true).ConfigureAwait(false);
                        }
                        var botClientsRoleConnections = await clientCompositeService.GetBotClientRoleConnection(webDriverBots[i].BotData.Id, webDriverBots[i].RoleId).ConfigureAwait(false);
                        var botSchedule = new List<EnumBotActionType>();
                        if ((botClientsRoleConnections != null) && (botClientsRoleConnections.Count > 0))
                        {
                            var maxClientsRoleConnectionActionsCount = botClientsRoleConnections.Count > 10 ? random.Next(10, 16) : botClientsRoleConnections.Count;
                            for (int j = 0; j < maxClientsRoleConnectionActionsCount; j++)
                                botSchedule.Add(EnumBotActionType.RoleMission);
                        }
                        var maxSeconActionsCount = random.Next(0, (int)(1 + botSchedule.Count / 3));
                        for (int j = 0; j < maxSeconActionsCount; j++)
                            botSchedule.Add((EnumBotActionType)random.Next(1, 4));
                        var maxMessagesActionCount = botSchedule.Count;
                        for (int j = 0; j < maxMessagesActionCount; j++)
                            botSchedule.Add(EnumBotActionType.CheckMessage);
                        botSchedule = Shuffle(botSchedule).ToList();
                        for (int j = 0; j < botSchedule.Count; j++)
                        {
                            var isActionError = false;
                            switch (botSchedule[j])
                            {
                                case EnumBotActionType.ListenMusic:
                                    await vkActionService.ListenMusic(WebDriverId).ConfigureAwait(false);
                                    break;
                                case EnumBotActionType.WatchVideo:
                                    await vkActionService.WatchVideo(WebDriverId).ConfigureAwait(false);
                                    break;
                                case EnumBotActionType.News:
                                    await vkActionService.News(WebDriverId).ConfigureAwait(false);
                                    break;
                                case EnumBotActionType.RoleMission:
                                    if (botClientsRoleConnections.Count > 0)
                                    {
                                        var roleMissionResult = await ExecuteRoleMission(WebDriverId, botClientsRoleConnections[0]).ConfigureAwait(false);
                                        if (roleMissionResult)
                                            clientCompositeService.SetSuccess(botClientsRoleConnections[0].Id, true);
                                        else
                                            clientCompositeService.SetSuccess(botClientsRoleConnections[0].Id, false);
                                        clientCompositeService.SetClientComplete(botClientsRoleConnections[0].Id);
                                        botClientsRoleConnections.RemoveAt(0);
                                    }
                                    break;
                                case EnumBotActionType.CheckMessage:
                                    await CheckMessage(WebDriverId, webDriverBots[i].BotData.Id).ConfigureAwait(false);
                                    break;
                            }
                            if (isActionError)
                            {
                                UpdateBotWorkStatus(webDriverBots[i].BotData.Id, EnumBotWorkStatus.Error);
                                break;
                            }
                            var botData = bots.FirstOrDefault(item => item.BotData.Id == webDriverBots[i].BotData.Id);
                            if ((botData != null) && (botData.WorkStatus == EnumBotWorkStatus.StopQuery))
                                break;
                        }
                        //Execute LogOut
                    }
                    else
                    {
                        
                    }
                }
            }
        }

        private IList<T> Shuffle<T>(IList<T> list)
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
            return list;
        }

        private bool OnlyInteger(string Row)
        {
            for (int i = 0; i < Row.Length; i++)
            {
                if (!Char.IsNumber(Row[i]))
                    return false;
            }
            return true;
        }

        private void UpdateBotWorkStatus(int BotId, EnumBotWorkStatus NewStatus)
        {
            var botData = bots.FirstOrDefault(item => item.BotData.Id == BotId);
            if (botData != null)
                bots[bots.IndexOf(botData)].WorkStatus = NewStatus;
        }

        private async Task<bool> ExecuteRoleMission(Guid WebDriverId, BotClientRoleConnectorModel BotClientRoleConnector)
        {
            var client = await clientCompositeService.GetClientById(BotClientRoleConnector.ClientId).ConfigureAwait(false);
            if (client != null)
            {
                var currentMission = await missionCompositeService.GetRoleMissionConnections(BotClientRoleConnector.RoleId, true);
                if ((currentMission != null) && (currentMission.Count > 0))
                {
                    var nodes = await missionCompositeService.GetNodes(currentMission[0].Id, null, null);
                    if ((nodes != null) && (nodes.Count > 0))
                    {
                        var stepResult = true;
                        for (int i = 0; i < nodes.Count; i++)
                        {
                            if (nodes[i].PatternId == -1)
                            {
                                switch (nodes[i].Type)
                                {
                                    case EnumMissionActionType.GoToProfile:
                                        if (OnlyInteger(nodes[i].Text))
                                            stepResult = await webDriverService.GoToURL(WebDriverId, "id" + nodes[i].Text).ConfigureAwait(false);
                                        else
                                            stepResult = await webDriverService.GoToURL(WebDriverId, nodes[i].Text).ConfigureAwait(false);
                                        break;
                                    case EnumMissionActionType.GoToGroup:
                                        stepResult = await webDriverService.GoToURL(WebDriverId, nodes[i].Text).ConfigureAwait(false);
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
                                        var newMessage = RandMess(nodes[i].Text);
                                        var sendMessageResult = await vkActionService.SendFirstMessage(WebDriverId, newMessage).ConfigureAwait(false);
                                        stepResult = !sendMessageResult.hasError;
                                        if (stepResult)
                                            await clientCompositeService.CreateMessage(BotClientRoleConnector.Id, newMessage).ConfigureAwait(false);
                                        break;
                                    case EnumMissionActionType.End:
                                        stepResult = true;
                                        break;
                                }
                                if ((stepResult) && (nodes[i].isRequired) && (BotClientRoleConnector.MissionId < 0))
                                {
                                    await clientCompositeService.SetMissionId(BotClientRoleConnector.Id, nodes[i].MissionId).ConfigureAwait(false);
                                    BotClientRoleConnector.MissionId = nodes[i].MissionId;
                                }
                                await clientCompositeService.SetClientMissionPath(BotClientRoleConnector.Id, nodes[i].Path).ConfigureAwait(false);
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
            return false;
        }

        private async Task<bool> CheckMessage(Guid WebDriverId, int BotId)
        {
            var dialogs = await vkActionService.GetDialogsWithNewMessages(WebDriverId).ConfigureAwait(false);
            while ((dialogs != null) && (dialogs.Count > 0))
            {
                var botClientRoleConnector = await clientCompositeService.GetBotClientRoleConnection(BotId, dialogs[0].ClientVkId);
                if (botClientRoleConnector != null)
                {
                    var readNewMessagesResult = await ReadNewMessages(WebDriverId, botClientRoleConnector.RoleId, botClientRoleConnector).ConfigureAwait(false);
                    if (!readNewMessagesResult)
                        clientCompositeService.SetSuccess(botClientRoleConnector.Id, false);
                }
                dialogs = await vkActionService.GetDialogsWithNewMessages(WebDriverId).ConfigureAwait(false);
            }
            var botDialogsWithNewBotMessages = await clientCompositeService.GetBotClientRoleConnection(BotId, null, null, null, true).ConfigureAwait(false);
            if ((botDialogsWithNewBotMessages != null) && (botDialogsWithNewBotMessages.Count > 0))
            {
                for (int i = 0; i < botDialogsWithNewBotMessages.Count; i++)
                {
                    var client = await clientCompositeService.GetClientById(botDialogsWithNewBotMessages[i].ClientId).ConfigureAwait(false);
                    if (client != null)
                    {
                        var goToDialogResult = await vkActionService.GoToDialog(WebDriverId, client.VkId).ConfigureAwait(false);
                        if (!goToDialogResult.hasError)
                        {
                            var messages = await clientCompositeService.GetMessagesByConnectionId(botDialogsWithNewBotMessages[i].Id).ConfigureAwait(false);
                            if ((messages != null) && (messages.Count > 0))
                            {
                                var sendAnswerMessageResult = new AlgoritmResult()
                                {
                                    ActionResultMessage = EnumActionResult.ElementError,
                                    hasError = true
                                };
                                for (int j = 0; j < messages.Count; j++)
                                {
                                    var newBotMessage = RandMess(messages[j].Text);
                                    sendAnswerMessageResult = await vkActionService.SendAnswerMessage(WebDriverId, newBotMessage).ConfigureAwait(false);
                                }
                                if (!sendAnswerMessageResult.hasError)
                                {
                                    clientCompositeService.SetHasNewBotMessages(botDialogsWithNewBotMessages[i].Id, false);
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        private async Task<bool> ReadNewMessages(Guid WebDriverId, int RoleId, BotClientRoleConnectorModel botClientRoleConnector)
        {
            var newMessages = await vkActionService.GetNewMessagesInDialog(WebDriverId).ConfigureAwait(false);//
            if ((newMessages != null) && (newMessages.Count > 0))
            {
                var newMessageText = "";
                for (int i = 0; i < newMessages.Count; i++)
                    newMessageText += newMessages[i].Text + " ";
                newMessageText = newMessageText.Remove(newMessageText.Length - 1);
                var standartPatterns = await botCompositeService.GetPatterns(RoleId).ConfigureAwait(false);//
                for (int i = 0; i < standartPatterns.Count; i++)
                {
                    if (isRegexMatch(newMessageText, standartPatterns[i].Text, standartPatterns[i].isRegex, standartPatterns[i].isInclude))
                    {
                        var botMessage = RandMess(standartPatterns[i].AnswerText);
                        var sendAnswerMessageResult = await vkActionService.SendAnswerMessage(WebDriverId, botMessage).ConfigureAwait(false);
                        var saveResult = await SaveNewMessage(botClientRoleConnector.Id, !sendAnswerMessageResult.hasError, newMessageText, botMessage).ConfigureAwait(false);
                        return saveResult;
                    }
                }
                var nodes = await missionCompositeService.GetNodes(botClientRoleConnector.MissionId, null, botClientRoleConnector.MissionPath).ConfigureAwait(false);
                if ((nodes != null) && (nodes.Count > 0))
                {
                    var nodePatterns = await missionCompositeService.GetNodePatterns(botClientRoleConnector.MissionId, nodes[0].PatternId).ConfigureAwait(false);
                    for (int i = 0; i < nodePatterns.Count; i++)
                    {
                        if (isRegexMatch(newMessageText, nodePatterns[i].PatternText, nodePatterns[i].isRegex, nodePatterns[i].isInclude))
                        {
                            var patternAction = await missionCompositeService.GetNodes(botClientRoleConnector.MissionId, nodePatterns[i].PatternId, null).ConfigureAwait(false);
                            if ((patternAction != null) && (patternAction.Count > 0) && (patternAction[0].Type == EnumMissionActionType.SendMessage))
                            {
                                var botMessage = RandMess(patternAction[0].Text);
                                var sendAnswerMessageResult = await vkActionService.SendAnswerMessage(WebDriverId, botMessage).ConfigureAwait(false);
                                var saveResult = await SaveNewMessage(botClientRoleConnector.Id, !sendAnswerMessageResult.hasError, newMessageText, botMessage).ConfigureAwait(false);
                                return saveResult;
                            }
                        }
                    }
                }
                return await SaveNewMessage(botClientRoleConnector.Id, false, newMessageText, null).ConfigureAwait(false);
            }
            return false;
        }

        private async Task<bool> SaveNewMessage(int BotClientRoleConnectorId, bool isSendAnswerMessageResultSuccess, string ClientMessage, string BotAnswer)
        {
            var result = await clientCompositeService.CreateMessage(BotClientRoleConnectorId, ClientMessage, false).ConfigureAwait(false);
            if (result)
            {
                if (isSendAnswerMessageResultSuccess)
                    result = await clientCompositeService.CreateMessage(BotClientRoleConnectorId, BotAnswer).ConfigureAwait(false);
                else
                    result = await clientCompositeService.SetHasNewMessage(BotClientRoleConnectorId).ConfigureAwait(false);
            }
            return result;
        }

        private bool isRegexMatch(string NewMessageText, string RegexString, bool isRegex, bool isInclude)
        {
            var regex = new Regex(@RegexString, RegexOptions.IgnoreCase);
            if (!isRegex)
                regex = new Regex(@TextToRegex(RegexString), RegexOptions.IgnoreCase);
            if (regex.IsMatch(NewMessageText) == isInclude)
            {
                return true;
            }
            return false;
        }

        private string RandMess(string message)
        {
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
                    message = message.Replace(oldstring, Elements[random.Next(0, Elements.Count)]);
                    i = -1;
                }
            }
            return message;
        }
        
        private string TextToRegex(string Text)
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
    }
}