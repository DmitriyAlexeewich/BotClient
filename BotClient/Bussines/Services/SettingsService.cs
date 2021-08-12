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
using System.Threading;
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

        public async Task<string> GetScreenshotFolderPath(string RoleId, string BotClientRoleConnectionId)
        {
            try
            {
                var screenshotFolderPath = $"{screenshotPath}{RoleId}";
                if (!Directory.Exists(screenshotFolderPath))
                    Directory.CreateDirectory(@screenshotFolderPath);
                screenshotFolderPath = $"{screenshotPath}{RoleId}\\{BotClientRoleConnectionId}";
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

        public bool WaitTime(int Milliseconds)
        {
            /*
            var waitingTime = DateTime.Now.AddMilliseconds(Milliseconds / 10);
            while (DateTime.Now < waitingTime) { 
            */
            Thread.Sleep(Milliseconds);
            return true;
        }

        public void ClearChromeDriverFolder()
        {
            try
            {
                if (Directory.Exists("C:\\Users\\Administrator\\AppData\\Local\\Temp\\2"))
                {
                    var directory = new DirectoryInfo("C:\\Users\\Administrator\\AppData\\Local\\Temp\\2");
                    foreach (FileInfo file in directory.GetFiles())
                        file.Delete();
                    foreach (DirectoryInfo subDirectory in directory.GetDirectories())
                        subDirectory.Delete();
                }
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex);
            }
        }

        private bool isSimilarPreviousAction(int OriginalIndex, List<EnumBotActionType> ScheduleList)
        {
            try
            {
                if (OriginalIndex > 0)
                    return ScheduleList[OriginalIndex - 1] == ScheduleList[OriginalIndex];

            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex);
            }
            return false;
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
