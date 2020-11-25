using BotClient.Bussines.Interfaces;
using BotClient.Models.Enumerators;
using BotClient.Models.HTMLElements;
using BotClient.Models.Settings;
using BotClient.Models.WebReports;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly IConfiguration configuration;

        public SettingsService(IConfiguration Configuration)
        {
            configuration = Configuration;
            configurationFilePath = configuration.GetSection("ConfigurationFilePath").Value;
            errorLogFilePath = configuration.GetSection("ErrorLogFilePath").Value;
            algoritmFilePath = configuration.GetSection("AlgoritmFilePath").Value;
            CreateConfigurationFile();
            CreateErrorLogFile();
        }

        private WebConnectionSettings webConnectionSettings = new WebConnectionSettings();
        private string configurationFilePath = string.Empty;
        private string errorLogFilePath = string.Empty;
        private string algoritmFilePath = string.Empty;

        public async Task<SettingsReport> CreateLink(WebConnectionSettings settings)
        {
            try
            {
                CreateConfigurationFile();
                webConnectionSettings = settings;
                File.WriteAllText(@configurationFilePath, JsonConvert.SerializeObject(webConnectionSettings));
                return new SettingsReport();
            }
            catch (Exception ex)
            {
                AddLog("SettingsService",ex.Message);
                return new SettingsReport()
                {
                    HasError = true,
                    ExceptionMessage = ex.Message
                };
            }
        }

        public async Task<SettingsReport> SetServerId(Guid ServerId)
        {
            try
            {
                CreateConfigurationFile();
                webConnectionSettings.ServerId = ServerId;
                File.WriteAllText(@configurationFilePath, JsonConvert.SerializeObject(webConnectionSettings));
                return new SettingsReport();
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex.Message);
                return new SettingsReport()
                {
                    HasError = true,
                    ExceptionMessage = ex.Message
                };
            }
        }

        public async Task<SettingsReport> SetParentServerIP(string ParentServerIP)
        {
            try
            {
                CreateConfigurationFile();
                webConnectionSettings.ParentServerIP = ParentServerIP;
                File.WriteAllText(@configurationFilePath, JsonConvert.SerializeObject(webConnectionSettings));
                return new SettingsReport();
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex.Message);
                return new SettingsReport()
                {
                    HasError = true,
                    ExceptionMessage = ex.Message
                };
            }
        }

        public async Task<SettingsReport> SetBrowserOptions(List<string> Options)
        {
            try
            {
                CreateConfigurationFile();
                webConnectionSettings.Options = Options;
                File.WriteAllText(@configurationFilePath, JsonConvert.SerializeObject(webConnectionSettings));
                return new SettingsReport();
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex.Message);
                return new SettingsReport()
                {
                    HasError = true,
                    ExceptionMessage = ex.Message
                };
            }
        }

        public async Task<SettingsReport> SetKeyWaitingTime(int KeyWaitingTimeMin, int KeyWaitingTimeMax)
        {
            try
            {
                CreateConfigurationFile();
                webConnectionSettings.KeyWaitingTimeMin = KeyWaitingTimeMin;
                webConnectionSettings.KeyWaitingTimeMax = KeyWaitingTimeMax;
                File.WriteAllText(@configurationFilePath, JsonConvert.SerializeObject(webConnectionSettings));
                return new SettingsReport();
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex.Message);
                return new SettingsReport()
                {
                    HasError = true,
                    ExceptionMessage = ex.Message
                };
            }
        }

        public async Task<SettingsReport> SetHTMLPageWaitingTime(int HTMLPageWaitingTime)
        {
            try
            {
                CreateConfigurationFile();
                webConnectionSettings.HTMLPageWaitingTime = HTMLPageWaitingTime;
                File.WriteAllText(@configurationFilePath, JsonConvert.SerializeObject(webConnectionSettings));
                return new SettingsReport();
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex.Message);
                return new SettingsReport()
                {
                    HasError = true,
                    ExceptionMessage = ex.Message
                };
            }
        }

        public async Task<SettingsReport> SetHTMLElementWaitingTime(int HTMLElementWaitingTime)
        {
            try
            {
                CreateConfigurationFile();
                webConnectionSettings.HTMLElementWaitingTime = HTMLElementWaitingTime;
                File.WriteAllText(@configurationFilePath, JsonConvert.SerializeObject(webConnectionSettings));
                return new SettingsReport();
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex.Message);
                return new SettingsReport()
                {
                    HasError = true,
                    ExceptionMessage = ex.Message
                };
            }
        }

        public async Task<SettingsReport> SetScrollCount(int ScrollCount)
        {
            try
            {
                CreateConfigurationFile();
                webConnectionSettings.ScrollCount = ScrollCount;
                File.WriteAllText(@configurationFilePath, JsonConvert.SerializeObject(webConnectionSettings));
                return new SettingsReport();
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex.Message);
                return new SettingsReport()
                {
                    HasError = true,
                    ExceptionMessage = ex.Message
                };
            }
        }

        public WebConnectionSettings GetServerSettings()
        {
            CreateConfigurationFile();
            return webConnectionSettings;
        }

        public async Task<bool> AddLog(string CodeFileName, string Error)
        {
            CreateErrorLogFile();
            if (!File.Exists(errorLogFilePath))
            {
                File.AppendAllText(@errorLogFilePath, Environment.NewLine + DateTime.UtcNow + " --- " + CodeFileName + ".cs --- " + Error);
                return true;
            }
            return false;
        }

        public async Task<List<string>> GetLogLines()
        {
            CreateErrorLogFile();
            var result = new List<string>();
            if (!File.Exists(errorLogFilePath))
            {
                result = File.ReadAllLines(@errorLogFilePath).ToList();
                return result;
            }
            return result;
        }

        public async Task<SettingsReport> AddUpdateAlgoritm(EnumAlgoritmName AlgoritmName, EnumSocialPlatform Platform, List<WebHTMLElementModel> Algoritm)
        {
            try
            {

                var algoritmJSONFilePath = algoritmFilePath + Platform.ToString() + "\\" + AlgoritmName.ToString() + ".json";
                if (!File.Exists(algoritmJSONFilePath))
                    Directory.CreateDirectory(Path.GetDirectoryName(algoritmJSONFilePath));
                File.WriteAllText(@algoritmJSONFilePath, JsonConvert.SerializeObject(Algoritm));
                return new SettingsReport();
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex.Message);
                return new SettingsReport()
                {
                    HasError = true,
                    ExceptionMessage = ex.Message
                };
            }
        }

        public async Task<List<WebHTMLElementModel>> GetAlgoritm(EnumAlgoritmName AlgoritmName, EnumSocialPlatform Platform)
        {
            var algoritmJSONFilePath = algoritmFilePath + Platform.ToString() + "\\" + AlgoritmName.ToString() + ".json";
            if (File.Exists(algoritmJSONFilePath))
            {
                return JsonConvert.DeserializeObject<List<WebHTMLElementModel>>(File.ReadAllText(algoritmJSONFilePath));
            }
            return new List<WebHTMLElementModel>();
        }

        private void CreateErrorLogFile()
        {
            if (!File.Exists(errorLogFilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(errorLogFilePath));
                File.Create(@errorLogFilePath);
            }
        }

        private void CreateConfigurationFile()
        {
            if (!File.Exists(configurationFilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(configurationFilePath));
                File.WriteAllText(@configurationFilePath, JsonConvert.SerializeObject(webConnectionSettings));
            }
            webConnectionSettings = JsonConvert.DeserializeObject<WebConnectionSettings>(File.ReadAllText(configurationFilePath));
        }
    }
}
