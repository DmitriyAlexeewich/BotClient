using BotClient.Bussines.Interfaces;
using BotClient.Models.Enumerators;
using BotClient.Models.HTMLElements;
using BotClient.Models.Settings;
using BotClient.Models.WebReports;
using BotDataModels.Bot.Enumerators;
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
            errorWebElementLogFilePath = configuration.GetSection("ErrorWebElementLogFilePath").Value;
            CreateConfigurationFile();
            CreateErrorLogFile();
            CreateScreenshotFolder();
        }

        private Random random = new Random(DateTime.Now.Millisecond);
        private WebConnectionSettings webConnectionSettings = new WebConnectionSettings();
        private string configurationFilePath = string.Empty;
        private string errorLogFilePath = string.Empty;
        private string errorWebElementLogFilePath = string.Empty;
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
        
        public async Task<SettingsReport> SetCapsChancePerThousandWords(int CapsChancePerThousandWords)
        {
            try
            {
                CreateConfigurationFile();
                webConnectionSettings.CapsChancePerThousandWords = CapsChancePerThousandWords;
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

        public async Task<SettingsReport> SetNumberChancePerHundredWords(int NumberChancePerHundredWords)
        {
            try
            {
                CreateConfigurationFile();
                webConnectionSettings.NumberChancePerHundredWords = NumberChancePerHundredWords;
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

        public async Task<SettingsReport> SetPlotCommaSplitChance(int PlotCommaSplitChance)
        {
            try
            {
                CreateConfigurationFile();
                webConnectionSettings.PlotCommaSplitChance = PlotCommaSplitChance;
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

        public async Task<SettingsReport> SetMinAtteptCountToRandMessage(int MinAtteptCountToRandMessage)
        {
            try
            {
                CreateConfigurationFile();
                webConnectionSettings.MinAtteptCountToRandMessage = MinAtteptCountToRandMessage;
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

        public async Task<SettingsReport> SetMaxAtteptCountToRandMessage(int MaxAtteptCountToRandMessage)
        {
            try
            {
                CreateConfigurationFile();
                webConnectionSettings.MaxAtteptCountToRandMessage = MaxAtteptCountToRandMessage;
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

        public async Task<SettingsReport> SetUseDateTimeHelloPhraseChance(int UseDateTimeHelloPhraseChance)
        {
            try
            {
                CreateConfigurationFile();
                webConnectionSettings.UseDateTimeHelloPhraseChance = UseDateTimeHelloPhraseChance;
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

        public async Task<SettingsReport> SetVideoLoadingWaitingTime(int VideoLoadingWaitingTime)
        {
            try
            {
                CreateConfigurationFile();
                webConnectionSettings.VideoLoadingWaitingTime = VideoLoadingWaitingTime;
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
                var settings = GetServerSettings();
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
                for (int i = 0; i < ScheduleList.Count; i++)
                {
                    if (ScheduleList[i] != EnumBotActionType.RoleMission)
                    {
                        chillActionCount++;
                        if (chillActionCount >= settings.MaxChillQueue)
                            ScheduleList[i] = 0;
                    }
                    else
                        chillActionCount = 0;
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

        private List<EnumBotActionType> AddEnumBotActionTypeRole(List<EnumBotActionType> ScheduleList)
        {
            try
            {
                if (ScheduleList[ScheduleList.Count - 1] == EnumBotActionType.RoleMission)
                {
                    var chillActionsCount = random.Next(1, 3);
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
    }
}
