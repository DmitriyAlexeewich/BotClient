using BotClient.Bussines.Interfaces;
using BotClient.Models.Enumerators;
using BotClient.Models.HTMLElements;
using BotClient.Models.WebReports;
using BotDataModels.Bot.Enumerators;
using BotDataModels.Client;
using BotDataModels.Settings;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
            errorWebElementLogFilePath = configuration.GetSection("ErrorWebElementLogFilePath").Value;
            passwordConstructorString = configuration.GetSection("PasswordConstructorString").Value;
            CreateConfigurationFile();
            CreateErrorLogFile();
            CreateScreenshotFolder();
        }

        private Random random = new Random();
        private WebConnectionSettings webConnectionSettings = new WebConnectionSettings();
        private string configurationFilePath = string.Empty;
        private string errorLogFilePath = string.Empty;
        private string errorWebElementLogFilePath = string.Empty;
        private string algoritmFilePath = string.Empty;
        private string screenshotPath = string.Empty;
        private string passwordConstructorString = string.Empty;

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
                    File.AppendAllText(@errorLogFilePath, Environment.NewLine + DateTime.Now + " --- " + CodeFileName + ".cs --- " + Environment.NewLine + Ex.ToString());
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
                    File.AppendAllText(@errorLogFilePath, Environment.NewLine + DateTime.Now + " --- " + CodeFileName + ".cs --- " + Environment.NewLine + Ex);
                    return true;
                }
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex);
            }
            return false;
        }

        public async Task<bool> AddWebElementLog(string Selector, string Link)
        {
            try
            {
                CreateErrorWebElementLogFile();
                if (File.Exists(errorWebElementLogFilePath))
                {
                    File.AppendAllText(errorWebElementLogFilePath, Environment.NewLine + DateTime.Now + " --- " + Selector + " ---- " + Link + " --- " + Environment.NewLine);
                    return true;
                }
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex);
            }
            return false;
        }

        //!--
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
        //--!

        public IList<T> Shuffle<T>(IList<T> list)
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
                AddLog("SettingsService", ex);
            }
            return list;
        }

        public List<EnumBotActionType> ShuffleSchedule(List<EnumBotActionType> ScheduleList)
        {
            try
            {
                ScheduleList = Shuffle(ScheduleList).ToList();
                var chillActionCount = 0;
                for (int i = 0; i < ScheduleList.Count; i++)
                {
                    if (ScheduleList[i] == EnumBotActionType.RoleMission)
                    {
                        if (((i > 0) && (ScheduleList[i - 1] == EnumBotActionType.RoleMission))
                            || ((i < ScheduleList.Count - 1) && (ScheduleList[i + 1] == EnumBotActionType.RoleMission)))
                        {
                            ScheduleList[i] = (EnumBotActionType)random.Next(1, 5);
                            ScheduleList = AddEnumBotActionTypeRole(ScheduleList);
                        }
                    }
                    else
                    {
                        while (((i > 0) && (ScheduleList[i - 1] == ScheduleList[i])) || ((i < ScheduleList.Count - 1) && (ScheduleList[i + 1] == ScheduleList[i])))
                            ScheduleList[i] = (EnumBotActionType)random.Next(1, 5);
                    }
                }
                ScheduleList.RemoveAll(item => item == 0);
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex);
            }
            return ScheduleList;
        }

        public IList<T> Split<T>(IList<T> list, int Index)
        {
            try
            {
                List<T> previous = new List<T>();
                for (int i = 0; i < list.Count; i++)
                {
                    if (i <= Index)
                        previous.Add(list[i]);
                    else
                        break;
                }
                list = previous;
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex);
            }
            return list;
        }

        //!--
        public async Task UpdateWebConnectionFile()
        {
            if (File.Exists(configurationFilePath))
                webConnectionSettings = JsonConvert.DeserializeObject<WebConnectionSettings>(File.ReadAllText(configurationFilePath));
        }
        //--!

        public async Task<string> GeneratePassword(int Length)
        {
            var result = "";
            try
            {
                StringBuilder res = new StringBuilder();
                Random rnd = new Random();
                while (0 < Length--)
                {
                    res.Append(passwordConstructorString[rnd.Next(passwordConstructorString.Length)]);
                }
                result = res.ToString();
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex);
            }
            return result;
        }

        private List<EnumBotActionType> AddEnumBotActionTypeRole(List<EnumBotActionType> ScheduleList)
        {
            try
            {
                if (ScheduleList[ScheduleList.Count - 1] == EnumBotActionType.RoleMission)
                {
                    var chillActionsCount = random.Next(2, 5);
                    for (int i = 0; i < chillActionsCount; i++)
                        ScheduleList.Add((EnumBotActionType)random.Next(1, 5));
                }
                ScheduleList.Add(EnumBotActionType.RoleMission);
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex);
            }
            return ScheduleList;
        }

        //!--
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

        private void CreateErrorWebElementLogFile()
        {
            try
            {
                if (!File.Exists(errorWebElementLogFilePath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(errorWebElementLogFilePath));
                    File.Create(errorWebElementLogFilePath);
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
    
        //--!
    
    }
}
