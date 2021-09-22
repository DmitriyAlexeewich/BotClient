using BotClient.Bussines.Interfaces;
using BotClient.Models.FileSystem.Enumerators;
using BotDataModels.Role.Enumerators;
using BotDataModels.Settings;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly IConfiguration configuration;
        private readonly IFileSystemService fileSystemService;

        public SettingsService(IConfiguration Configuration, IFileSystemService FileSystemService)
        {
            configuration = Configuration;
            fileSystemService = FileSystemService;
        }

        private Random random = new Random();

        public WebConnectionSettings GetServerSettings()
        {
            var webConnectionSettingsFilePath = fileSystemService.GetBasicFilePath(EnumBasicDirectoryType.Configuration, "Configuration.json");
            var webConnectionSettings = new WebConnectionSettings();
            if (webConnectionSettingsFilePath.isFileExist)
                webConnectionSettings = JsonConvert.DeserializeObject<WebConnectionSettings>(File.ReadAllText(webConnectionSettingsFilePath.FilePath));
            else
            {
                fileSystemService.CreateFile(webConnectionSettingsFilePath.FilePath, JsonConvert.SerializeObject(webConnectionSettings));
                webConnectionSettingsFilePath = fileSystemService.GetBasicFilePath(EnumBasicDirectoryType.Configuration, "Configuration.json");
                if(webConnectionSettingsFilePath.isFileExist)
                    webConnectionSettings = JsonConvert.DeserializeObject<WebConnectionSettings>(File.ReadAllText(webConnectionSettingsFilePath.FilePath));
            }
            return webConnectionSettings;
        }

        public async void AddLog(string CodeFileName, Exception ExceptionText)
        {
            try
            {
                var fileName = DateTime.Now.ToString("yyyy_MM_dd") + ".txt";
                var debugLogFilePath = fileSystemService.GetBasicFilePath(EnumBasicDirectoryType.DebugLog, fileName);
                fileSystemService.AddTextToFile(debugLogFilePath.FilePath, 
                                                Environment.NewLine + DateTime.Now.ToString("HH:mm:ss") + " ---> " + CodeFileName + ".cs --- " +
                                                Environment.NewLine + ExceptionText.ToString() +
                                                Environment.NewLine + "--------------------" + Environment.NewLine + Environment.NewLine);
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex);
            }
        }

        public async void AddElementLog(string Selector, string Link)
        {
            try
            {
                var fileName = DateTime.Now.ToString("yyyy_MM_dd") + ".txt";
                var elementLogDirectoryPath = fileSystemService.GetBasicDirectoriesPath(EnumBasicDirectoryType.ElementLog);
                if (elementLogDirectoryPath.isDirectoryExist)
                {
                    var elementLogFiles = fileSystemService.GetFiles(elementLogDirectoryPath.DirectoryPath, "*.txt", null, DateTime.Now.AddDays(-7));
                    for (int i = 0; i < elementLogFiles.Count; i++)
                        fileSystemService.DeleteFile(elementLogFiles[i].FilePath);
                    var elementLogFilePath = fileSystemService.GetFile(elementLogDirectoryPath + fileName);
                    fileSystemService.AddTextToFile(elementLogFilePath.FilePath, 
                                                    Environment.NewLine + DateTime.Now.ToString("HH:mm:ss") + " ---> " + Selector + " --- " +
                                                    Environment.NewLine + Link +
                                                    Environment.NewLine + "--------------------" + Environment.NewLine + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex);
            }
        }

        public async void AddMissionLog(EnumMissionActionType MissionActionType, string Text)
        {
            try
            {
                var fileName = DateTime.Now.ToString("yyyy_MM_dd") + ".txt";
                var elementLogDirectoryPath = fileSystemService.GetBasicDirectoriesPath(EnumBasicDirectoryType.MissionLog);
                if (elementLogDirectoryPath.isDirectoryExist)
                {
                    var elementLogFiles = fileSystemService.GetFiles(elementLogDirectoryPath.DirectoryPath, "*.txt", null, DateTime.Now.AddDays(-7));
                    for (int i = 0; i < elementLogFiles.Count; i++)
                        fileSystemService.DeleteFile(elementLogFiles[i].FilePath);
                    var elementLogFilePath = fileSystemService.GetFile(elementLogDirectoryPath + fileName);
                    fileSystemService.AddTextToFile(elementLogFilePath.FilePath, 
                                                    Environment.NewLine + DateTime.Now.ToString("HH:mm:ss") + " ---> " + MissionActionType.ToString("g") + " --- " +
                                                    Environment.NewLine + Text +
                                                    Environment.NewLine + "--------------------" + Environment.NewLine + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex);
            }
        }

        public string GetScreenshotFolderPath(int RoleId, int MissionId, int ConnectionId)
        {
            var result = "";
            try
            {
                var screenshotFolderPath = fileSystemService.GetBasicDirectoriesPath(EnumBasicDirectoryType.Screenshot);
                if (screenshotFolderPath.isDirectoryExist)
                {
                    screenshotFolderPath = fileSystemService.GetDiretory(screenshotFolderPath.DirectoryPath + $"{RoleId}\\{MissionId}\\{ConnectionId}");
                    if (screenshotFolderPath.isDirectoryExist)
                        result = screenshotFolderPath.DirectoryPath;
                }
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex);
            }
            return result;
        }

        public void ClearChromeDriverFolder()
        {
            try
            {
                var chromeDriverDirectories = fileSystemService.GetDirectories("C:\\Users\\Administrator\\AppData\\Local\\Temp");
                for (int i = 0; i < chromeDriverDirectories.Count; i++)
                {
                    var directoryName = Path.GetDirectoryName(chromeDriverDirectories[i].DirectoryPath);
                    fileSystemService.DeleteDirectory(chromeDriverDirectories[i].DirectoryPath);
                    fileSystemService.CreateDirectory(chromeDriverDirectories[i].DirectoryPath);
                }
            }
            catch (Exception ex)
            {
                AddLog("SettingsService", ex);
            }
        }

        public async Task<string> GeneratePassword(int Length)
        {
            var result = "";
            try
            {
                StringBuilder generationResult = new StringBuilder();
                var passwordConstructorString = configuration.GetSection("PasswordConstructorString").Value;
                while (0 < Length--)
                {
                    generationResult.Append(passwordConstructorString[random.Next(passwordConstructorString.Length)]);
                }
                result = generationResult.ToString();
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
    }
}
