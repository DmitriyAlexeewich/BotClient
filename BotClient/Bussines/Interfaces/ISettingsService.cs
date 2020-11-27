using BotClient.Models.Enumerators;
using BotClient.Models.HTMLElements;
using BotClient.Models.Settings;
using BotClient.Models.WebReports;
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

        WebConnectionSettings GetServerSettings();

        Task<bool> AddLog(string CodeFileName, string Error);

        Task<List<string>> GetLogLines();

        Task<SettingsReport> AddUpdateAlgoritm(EnumAlgoritmName AlgoritmName, EnumSocialPlatform Platform, List<WebHTMLElementModel> Algoritm);

        Task<List<WebHTMLElementModel>> GetAlgoritm(EnumAlgoritmName AlgoritmName, EnumSocialPlatform Platform);
    }
}
