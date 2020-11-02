﻿using BotClient.Models.Bot;
using BotClient.Models.Bot.Work;
using BotClient.Models.Bot.Work.Enumerators;
using BotClient.Models.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces
{
    public interface IVkActionService
    {
        Task<AlgoritmResult> Login(Guid WebDriverId, string Username, string Password);
        Task<bool> isLoginSuccess(Guid WebDriverId);
        Task<AlgoritmResult> Customize(Guid WebDriverId, BotCustomizeModel CustomizeData);
        Task<AlgoritmResult> ListenMusic(Guid WebDriverId);
        Task<AlgoritmResult> WatchVideo(Guid WebDriverId);
        Task<AlgoritmResult> News(Guid WebDriverId);
        Task<AlgoritmResult> AvatarLike(Guid WebDriverId);
        Task<AlgoritmResult> NewsLike(Guid WebDriverId, EnumNewsLikeType NewsLikeType);
        Task<AlgoritmResult> Subscribe(Guid WebDriverId);
        Task<AlgoritmResult> SubscribeToGroup(Guid WebDriverId);
        Task<AlgoritmResult> Repost(Guid WebDriverId, EnumRepostType RepostType);
        Task<AlgoritmResult> SendFirstMessage(Guid WebDriverId, string MessageText);
        Task<AlgoritmResult> GoToDialog(Guid WebDriverId, string ClientVkId);
        Task<List<DialogWithNewMessagesModel>> GetDialogsWithNewMessages(Guid WebDriverId);
        Task<List<NewMessageModel>> GetNewMessagesInDialog(Guid WebDriverId);
        Task<AlgoritmResult> SendAnswerMessage(Guid WebDriverId, string MessageText);
    }
}
