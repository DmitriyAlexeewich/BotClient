using BotClient.Models.Enumerators;
using BotClient.Models.HTMLElements;
using BotClient.Models.Settings;
using BotClient.Models.WebReports;
using BotDataModels.Bot.Enumerators;
using BotDataModels.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces
{
    public interface ISettingsService
    {
        Task<SettingsReport> CreateLink(WebConnectionSettings settings);

        Task<SettingsReport> SetServerId(Guid ServerId);

        Task<SettingsReport> SetParentServerIP(string ParentServerIP);

        Task<SettingsReport> SetBrowserOptions(List<string> Options);

        Task<SettingsReport> SetKeyWaitingTime(int KeyWaitingTimeMin, int KeyWaitingTimeMax);

        Task<SettingsReport> SetHTMLPageWaitingTime(int HTMLPageWaitingTime);

        Task<SettingsReport> SetHTMLElementWaitingTime(int HTMLElementWaitingTime);

        Task<SettingsReport> SetScrollCount(int ScrollCount);

        Task<SettingsReport> SetErrorChancePerTenWords(int ErrorChancePerTenWords);

        Task<SettingsReport> SetCapsChancePerThousandWords(int CapsChancePerThousandWords);

        Task<SettingsReport> SetNumberChancePerHundredWords(int NumberChancePerHundredWords);

        Task<SettingsReport> SetMusicWaitingTime(int MusicWaitingTime, int MusicWaitingDeltaTime);

        Task<SettingsReport> SetVideoWaitingTime(int VideoWaitingTime, int VideoWaitingDeltaTime);

        Task<SettingsReport> SetVideoLoadingWaitingTime(int VideoLoadingWaitingTime);

        WebConnectionSettings GetServerSettings();

        Task<bool> AddLog(string CodeFileName, Exception Ex);

        Task<bool> AddLog(string CodeFileName, string Ex);

        Task<bool> AddWebElementLog(string Selector, string Link);

        Task<List<string>> GetLogLines();

        Task<SettingsReport> AddUpdateAlgoritm(EnumAlgoritmName AlgoritmName, EnumSocialPlatform Platform, List<WebHTMLElementModel> Algoritm);

        Task<List<WebHTMLElementModel>> GetAlgoritm(EnumAlgoritmName AlgoritmName, EnumSocialPlatform Platform);

        Task<string> GetScreenshotFolderPath(string BotClientRoleConnectionId);

        Task<bool> DeleteScreenshotFolder(List<DialogScreenshotModel> DialogScreenshots);

        IList<T> Shuffle<T>(IList<T> list);
        
        List<EnumBotActionType> ShuffleSchedule(List<EnumBotActionType> ScheduleList);
        
        IList<T> Split<T>(IList<T> list, int Index);

        Task UpdateWebConnectionFile();
    }
}
