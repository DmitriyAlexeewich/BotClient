using BotClient.Models.Enumerators;
using BotClient.Models.HTMLElements;
using BotClient.Models.WebReports;
using BotDataModels.Bot.Enumerators;
using BotDataModels.Client;
using BotDataModels.Role.Enumerators;
using BotDataModels.Settings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces
{
    public interface ISettingsService
    {
        WebConnectionSettings GetServerSettings();
        void AddLog(string CodeFileName, Exception ExceptionText);
        void AddElementLog(string Selector, string Link);
        void AddMissionLog(EnumMissionActionType MissionActionType, string Text);
        string GetScreenshotFolderPath(int RoleId, int MissionId, int ConnectionId);
        void ClearChromeDriverFolder();
        Task<string> GeneratePassword(int Length);
        bool WaitTime(int Milliseconds);
        IList<T> Shuffle<T>(IList<T> list);
        IList<T> Split<T>(IList<T> list, int Index);
    }
}
