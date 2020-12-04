using BotClient.Bussines.Interfaces;
using BotClient.Models.Enumerators;
using BotClient.Models.HTMLElements;
using BotClient.Models.Settings;
using BotClient.Models.WebReports;
using BotDataModels.Client;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            screenshotPath = configuration.GetSection("ScreenshotPath").Value;
            CreateConfigurationFile();
            CreateErrorLogFile();
            CreateScreenshotFolder();
        }

        private WebConnectionSettings webConnectionSettings = new WebConnectionSettings();
        private string configurationFilePath = string.Empty;
        private string errorLogFilePath = string.Empty;
        private string algoritmFilePath = string.Empty;
        private string screenshotPath = string.Empty;

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
                AddLog("SettingsService",ex);
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
                AddLog("SettingsService", ex);
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
                AddLog("SettingsService", ex);
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
                AddLog("SettingsService", ex);
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
                AddLog("SettingsService", ex);
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
                AddLog("SettingsService", ex);
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
                AddLog("SettingsService", ex);
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
                AddLog("SettingsService", ex);
                return new SettingsReport()
                {
                    HasError = true,
                    ExceptionMessage = ex.Message
                };
            }
        }

        public async Task<SettingsReport> SetErrorChancePerTenWords(int ErrorChancePerTenWords)
        {
            try
            {
                CreateConfigurationFile();
                webConnectionSettings.ErrorChancePerTenWords = ErrorChancePerTenWords;
                File.WriteAllText(@configurationFilePath, JsonConvert.SerializeObject(webConnectionSettings));
                return new SettingsReport();
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex);
                return new SettingsReport()
                {
                    HasError = true,
                    ExceptionMessage = ex.Message
                };
            }
        }

        public async Task<SettingsReport> SetMusicWaitingTime(int MusicWaitingTime, int MusicWaitingDeltaTime)
        {
            try
            {
                CreateConfigurationFile();
                webConnectionSettings.MusicWaitingTime = MusicWaitingTime;
                webConnectionSettings.MusicWaitingDeltaTime = MusicWaitingDeltaTime;
                File.WriteAllText(@configurationFilePath, JsonConvert.SerializeObject(webConnectionSettings));
                return new SettingsReport();
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex);
                return new SettingsReport()
                {
                    HasError = true,
                    ExceptionMessage = ex.Message
                };
            }
        }

        public async Task<SettingsReport> SetVideoWaitingTime(int VideoWaitingTime, int VideoWaitingDeltaTime)
        {
            try
            {
                CreateConfigurationFile();
                webConnectionSettings.VideoWaitingTime = VideoWaitingTime;
                webConnectionSettings.VideoWaitingDeltaTime = VideoWaitingDeltaTime;
                File.WriteAllText(@configurationFilePath, JsonConvert.SerializeObject(webConnectionSettings));
                return new SettingsReport();
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex);
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

        public async Task<bool> AddLog(string CodeFileName, Exception Ex)
        {
            try
            {
                CreateErrorLogFile();
                if (File.Exists(errorLogFilePath))
                {
                    File.AppendAllText(@errorLogFilePath, Environment.NewLine + DateTime.UtcNow + " --- " + CodeFileName + ".cs --- " + Environment.NewLine + Ex.ToString());
                    return true;
                }
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex);
            }
            return false;
        }

        public async Task<bool> AddLog(string CodeFileName, string Ex)
        {
            try
            {
                CreateErrorLogFile();
                if (File.Exists(errorLogFilePath))
                {
                    File.AppendAllText(@errorLogFilePath, Environment.NewLine + DateTime.UtcNow + " --- " + CodeFileName + ".cs --- " + Environment.NewLine + Ex);
                    return true;
                }
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex);
            }
            return false;
        }

        public async Task<List<string>> GetLogLines()
        {
            try
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
            catch (Exception ex)
            {
                AddLog("SettingsService", ex);
            }
            return new List<string>();
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
                AddLog("SettingsService", ex);
                return new SettingsReport()
                {
                    HasError = true,
                    ExceptionMessage = ex.Message
                };
            }
        }

        public async Task<List<WebHTMLElementModel>> GetAlgoritm(EnumAlgoritmName AlgoritmName, EnumSocialPlatform Platform)
        {
            try
            {
                var algoritmJSONFilePath = algoritmFilePath + Platform.ToString() + "\\" + AlgoritmName.ToString() + ".json";
                if (File.Exists(algoritmJSONFilePath))
                {
                    return JsonConvert.DeserializeObject<List<WebHTMLElementModel>>(File.ReadAllText(algoritmJSONFilePath));
                }
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex);
            }
            return new List<WebHTMLElementModel>();
        }

        public async Task<string> GetScreenshotFolderPath(string BotClientRoleConnectionId)
        {
            try
            {
                var screenshotFolderPath = $"{screenshotPath}{BotClientRoleConnectionId}";
                if (!Directory.Exists(screenshotFolderPath))
                    Directory.CreateDirectory(@screenshotFolderPath);
                return screenshotFolderPath;
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex);
            }
            return null;
        }

        public async Task<bool> DeleteScreenshotFolder(List<DialogScreenshotModel> DialogScreenshots)
        {
            try
            {
                for (int i = 0; i < DialogScreenshots.Count; i++)
                {
                    var path = await GetScreenshotFolderPath(DialogScreenshots[i].BotClientRoleConnectionId.ToString()).ConfigureAwait(false);
                    if(!Directory.Exists(path))
                        Directory.Delete(path, true);
                }
                return true;
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex);
            }
            return false;
        }

        private void CreateErrorLogFile()
        {
            try
            {
                if (!File.Exists(errorLogFilePath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(@errorLogFilePath));
                    File.Create(@errorLogFilePath);
                }
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex);
            }
        }

        private void CreateScreenshotFolder()
        {
            try
            {
                if (!Directory.Exists(screenshotPath))
                    Directory.CreateDirectory(@screenshotPath);
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex);
            }
        }

        private void CreateConfigurationFile()
        {
            try
            {
                if (!File.Exists(configurationFilePath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(configurationFilePath));
                    File.WriteAllText(@configurationFilePath, JsonConvert.SerializeObject(webConnectionSettings));
                }
                webConnectionSettings = JsonConvert.DeserializeObject<WebConnectionSettings>(File.ReadAllText(configurationFilePath));
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex);
            }
        }
    }
}
