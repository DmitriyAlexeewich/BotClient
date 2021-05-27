using BotClient.Models.Enumerators;
using BotClient.Models.HTMLElements;
using BotClient.Models.WebReports;
using BotDataModels.Bot.Enumerators;
using BotDataModels.Client;
using BotDataModels.Settings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces
{
    public interface ISettingsService
    {

        WebConnectionSettings GetServerSettings();

        Task<bool> AddLog(string CodeFileName, Exception Ex);

        Task<bool> AddLog(string CodeFileName, string Ex);

        Task<bool> AddWebElementLog(string Selector, string Link);

        Task<List<string>> GetLogLines();

        Task<SettingsReport> AddUpdateAlgoritm(EnumAlgoritmName AlgoritmName, EnumSocialPlatform Platform, List<WebHTMLElementModel> Algoritm);

        Task<List<WebHTMLElementModel>> GetAlgoritm(EnumAlgoritmName AlgoritmName, EnumSocialPlatform Platform);

        Task<string> GetScreenshotFolderPath(string RoleId, string BotClientRoleConnectionId);

        IList<T> Shuffle<T>(IList<T> list);
        
        List<EnumBotActionType> ShuffleSchedule(List<EnumBotActionType> ScheduleList);
        
        IList<T> Split<T>(IList<T> list, int Index);

        Task UpdateWebConnectionFile();
        Task<string> GeneratePassword(int Length);

        bool WaitTime(int Milliseconds);
    }
}
