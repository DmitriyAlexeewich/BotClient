using BotClient.Bussines.Interfaces;
using BotClient.Models.Enumerators;
using BotClient.Models.HTMLElements;
using BotClient.Models.HTMLElements.Enumerators;
using BotClient.Models.HTMLWebDriver;
using BotClient.Models.WebReports;
using BotDataModels.Client;
using BotMySQL.Bussines.Interfaces.MySQL;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services
{
    public class WebDriverService : IWebDriverService
    {
        private readonly ISettingsService settingsService;
        private readonly IDialogScreenshotService dialogScreenshotService;

        public WebDriverService(ISettingsService SettingsService,
                                IDialogScreenshotService DialogScreenshotService)
        {
            settingsService = SettingsService;
            dialogScreenshotService = DialogScreenshotService;
        }

        List<HTMLWebDriver> webDrivers = new List<HTMLWebDriver>();

        public async Task Start(int BrowserCount, EnumSocialPlatform SocialPlatform)
        {
            try
            {
                for (int i = 0; i < webDrivers.Count; i++)
                {
                    webDrivers[i].WebDriver.Quit();
                }
                webDrivers = new List<HTMLWebDriver>();
                List<DriverReport> result = new List<DriverReport>();
                for (int i = 0; i < BrowserCount; i++)
                {
                    var startResult = StartWebDriver(SocialPlatform);
                    if (startResult != null)
                    {
                        webDrivers.Add(startResult.Item1);
                        result.Add(startResult.Item2);
                        if (startResult.Item2.HasError)
                        {
                            webDrivers = new List<HTMLWebDriver>();
                            await settingsService.AddLog("WebDriver", startResult.Item2.ExceptionMessage).ConfigureAwait(false);
                            break;
                        }
                    }
                }
                for (int i = 0; i < webDrivers.Count; i++)
                {
                    webDrivers[i].Status = EnumWebDriverStatus.Ready;
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("WebDriverService", ex).ConfigureAwait(false);
            }
        }

        public async Task Restart(Guid WebDriverId)
        {
            try
            {
                DriverReport result = new DriverReport();
                var webDriver = webDrivers.FirstOrDefault(item => item.Id == WebDriverId);
                if (webDriver != null)
                {
                    webDrivers[webDrivers.IndexOf(webDriver)].Status = EnumWebDriverStatus.Start;
                    webDriver.WebDriver.Quit();
                    var startResult = StartWebDriver(webDriver.WebDriverPlatform);
                    webDrivers[webDrivers.IndexOf(webDriver)] = startResult.Item1;
                    result = startResult.Item2;
                }
                if(result.HasError)
                    await settingsService.AddLog("WebDriver", result.ExceptionMessage).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("WebDriverService", ex).ConfigureAwait(false);
            }
        }
        /*
        //Not working
        public async Task ExecuteAlgoritm(Guid WebDriverId, List<WebHTMLElementModel> WebHTMLElementModels)
        {
            var webDriver = webDrivers.FirstOrDefault(item => item.Id == WebDriverId);
            var driverReport = new DriverReport()
            {
                ServerId = settingsService.GetServerSettings().ServerId
            };
            try
            {
                if ((webDriver != null) && (webDriver.Status == EnumWebDriverStatus.Ready))
                {
                    webDrivers[webDrivers.IndexOf(webDriver)].Status = EnumWebDriverStatus.Blocked;
                    driverReport.DriverId = webDriver.Id;
                    for (int i = 0; i < WebHTMLElementModels.Count; i++)
                    {
                        var webHTMLElement = new WebHTMLElement(webDriver.WebDriver, WebHTMLElementModels[i], settingsService.GetServerSettings());
                        var errorFlag = webHTMLElement.isAvailable;
                        if (!errorFlag)
                        {
                            for (int j = 0; j < WebHTMLElementModels[i].Actions.Count; j++)
                            {
                                switch (WebHTMLElementModels[i].Actions[j].ActionType)
                                {
                                    case EnumWebHTMLElementActionType.WaitPageLoading:
                                        driverReport.ActionsReport.Add(new ActionReport()
                                        {
                                            Id = WebHTMLElementModels[i].Actions[j].Id,
                                            Type = EnumWebHTMLElementActionType.WaitPageLoading,
                                            ResultBool = webHTMLElement.WaitPageLoading() != EnumWebHTMLPageStatus.Ready ? false : true
                                        });
                                        break;
                                    case EnumWebHTMLElementActionType.Click:
                                        driverReport.ActionsReport.Add(new ActionReport()
                                        {
                                            Id = WebHTMLElementModels[i].Actions[j].Id,
                                            Type = EnumWebHTMLElementActionType.Click,
                                            ResultBool = webHTMLElement.Click(WebHTMLElementModels[i].Actions[j].ClickType)
                                        });
                                        break;
                                    case EnumWebHTMLElementActionType.PrintText:
                                        driverReport.ActionsReport.Add(new ActionReport()
                                        {
                                            Id = WebHTMLElementModels[i].Actions[j].Id,
                                            Type = EnumWebHTMLElementActionType.PrintText,
                                            ResultBool = webHTMLElement.PrintText(WebHTMLElementModels[i].Actions[j].Text)
                                        });
                                        break;
                                    case EnumWebHTMLElementActionType.Scroll:
                                        driverReport.ActionsReport.Add(new ActionReport()
                                        {
                                            Id = WebHTMLElementModels[i].Actions[j].Id,
                                            Type = EnumWebHTMLElementActionType.Scroll,
                                            ResultBool = webHTMLElement.Sroll()
                                        });
                                        break;
                                    case EnumWebHTMLElementActionType.WaitWebHTMLElement:
                                        driverReport.ActionsReport.Add(new ActionReport()
                                        {
                                            Id = WebHTMLElementModels[i].Actions[j].Id,
                                            Type = EnumWebHTMLElementActionType.WaitWebHTMLElement,
                                            ResultBool = webHTMLElement.WaitWebHTMLElement(WebHTMLElementModels[i].Actions[j].isParentElementIsDriver, WebHTMLElementModels[i].Actions[j].ElementModel)
                                        });
                                        break;
                                    case EnumWebHTMLElementActionType.CheckAttribute:
                                        driverReport.ActionsReport.Add(new ActionReport()
                                        {
                                            Id = WebHTMLElementModels[i].Actions[j].Id,
                                            Type = EnumWebHTMLElementActionType.CheckAttribute,
                                            ResultBool = webHTMLElement.CheckAttribute(WebHTMLElementModels[i].Actions[j].AttributeName, WebHTMLElementModels[i].Actions[j].AttributeValue)
                                        });
                                        break;
                                    case EnumWebHTMLElementActionType.GetINNERText:
                                        var innerText = webHTMLElement.GetINNERText(WebHTMLElementModels[i].Actions[j].removeHTMLTags);
                                        driverReport.ActionsReport.Add(new ActionReport()
                                        {
                                            Id = WebHTMLElementModels[i].Actions[j].Id,
                                            Type = EnumWebHTMLElementActionType.GetINNERText,
                                            ResultString = innerText,
                                            ResultBool = innerText != null ? true : false
                                        });
                                        break;
                                    case EnumWebHTMLElementActionType.GetAttributeValue:
                                        var attributeValue = webHTMLElement.GetINNERText(WebHTMLElementModels[i].Actions[j].removeHTMLTags);
                                        driverReport.ActionsReport.Add(new ActionReport()
                                        {
                                            Id = WebHTMLElementModels[i].Actions[j].Id,
                                            Type = EnumWebHTMLElementActionType.GetAttributeValue,
                                            ResultString = attributeValue,
                                            ResultBool = attributeValue != null ? true : false
                                        });
                                        break;
                                }
                                if (driverReport.ActionsReport.FirstOrDefault(item => item.ResultBool == false) != null)
                                {
                                    errorFlag = true;
                                    break;
                                }
                            }
                        }
                        if ((errorFlag)&&(webHTMLElement.HTMLElementModel.isRequired))
                        {
                            driverReport.HasError = true;
                            break;
                        }
                    }
                    if (driverReport.HasError)
                    {
                        switch (webDriver.WebDriverPlatform)
                        {
                            case EnumSocialPlatform.Vk:
                                webDriver.WebDriver.Navigate().GoToUrl("https://vk.com/");
                                break;
                            default:
                                break;
                        }
                        webDrivers[webDrivers.IndexOf(webDriver)].Status = WaitPageLoading(webDriver) == EnumWebHTMLPageStatus.Ready ?
                                                                                                            EnumWebDriverStatus.Ready : EnumWebDriverStatus.Error;
                    }
                }
                else
                {
                    driverReport.DriverId = WebDriverId;
                    driverReport.HasError = true;
                    driverReport.ExceptionMessage = "Wrong WebDriverId";
                }
                if (webDrivers[webDrivers.IndexOf(webDriver)].Status == EnumWebDriverStatus.Error)
                {
                    driverReport.DriverId = WebDriverId;
                    driverReport.HasError = true;
                    driverReport.ExceptionMessage = "Driver HTML loading error";
                }
                else
                {
                    driverReport.DriverId = WebDriverId;
                    driverReport.HasError = false;
                }
            }
            catch(Exception ex)
            {
                driverReport.DriverId = WebDriverId;
                driverReport.HasError = true;
                driverReport.ExceptionMessage = ex.Message;
            }
        }
        
        public async Task<List<ActionReport>> ExecuteCommand(Guid WebDriverId, WebHTMLElementModel WebHTMLElement)
        {
            var result = new List<ActionReport>();
            try
            {
                var webDriver = webDrivers.FirstOrDefault(item => item.Id == WebDriverId);
                if ((webDriver != null) && (webDriver.Status == EnumWebDriverStatus.Ready))
                {
                    webDrivers[webDrivers.IndexOf(webDriver)].Status = EnumWebDriverStatus.Blocked;
                    var webHTMLElement = new WebHTMLElement(webDriver.WebDriver, WebHTMLElement, settingsService.GetServerSettings());
                    var errorFlag = webHTMLElement.isAvailable;
                    if (!errorFlag)
                    {
                        for (int i = 0; i < WebHTMLElement.Actions.Count; i++)
                        {
                            switch (WebHTMLElement.Actions[i].ActionType)
                            {
                                case EnumWebHTMLElementActionType.WaitPageLoading:
                                    result.Add(new ActionReport()
                                    {
                                        Id = WebHTMLElement.Actions[i].Id,
                                        Type = EnumWebHTMLElementActionType.WaitPageLoading,
                                        ResultBool = webHTMLElement.WaitPageLoading() != EnumWebHTMLPageStatus.Ready ? false : true
                                    });
                                    break;
                                case EnumWebHTMLElementActionType.Click:
                                    result.Add(new ActionReport()
                                    {
                                        Id = WebHTMLElement.Actions[i].Id,
                                        Type = EnumWebHTMLElementActionType.Click,
                                        ResultBool = webHTMLElement.Click(WebHTMLElement.Actions[i].ClickType)
                                    });
                                    break;
                                case EnumWebHTMLElementActionType.PrintText:
                                    result.Add(new ActionReport()
                                    {
                                        Id = WebHTMLElement.Actions[i].Id,
                                        Type = EnumWebHTMLElementActionType.PrintText,
                                        ResultBool = webHTMLElement.PrintText(WebHTMLElement.Actions[i].Text)
                                    });
                                    break;
                                case EnumWebHTMLElementActionType.Scroll:
                                    result.Add(new ActionReport()
                                    {
                                        Id = WebHTMLElement.Actions[i].Id,
                                        Type = EnumWebHTMLElementActionType.Scroll,
                                        ResultBool = webHTMLElement.Sroll()
                                    });
                                    break;
                                case EnumWebHTMLElementActionType.WaitWebHTMLElement:
                                    result.Add(new ActionReport()
                                    {
                                        Id = WebHTMLElement.Actions[i].Id,
                                        Type = EnumWebHTMLElementActionType.WaitWebHTMLElement,
                                        ResultBool = webHTMLElement.WaitWebHTMLElement(WebHTMLElement.Actions[i].isParentElementIsDriver, WebHTMLElement.Actions[i].ElementModel)
                                    });
                                    break;
                                case EnumWebHTMLElementActionType.CheckAttribute:
                                    result.Add(new ActionReport()
                                    {
                                        Id = WebHTMLElement.Actions[i].Id,
                                        Type = EnumWebHTMLElementActionType.CheckAttribute,
                                        ResultBool = webHTMLElement.CheckAttribute(WebHTMLElement.Actions[i].AttributeName, WebHTMLElement.Actions[i].AttributeValue)
                                    });
                                    break;
                                case EnumWebHTMLElementActionType.GetINNERText:
                                    var innerText = webHTMLElement.GetINNERText(WebHTMLElement.Actions[i].removeHTMLTags);
                                    result.Add(new ActionReport()
                                    {
                                        Id = WebHTMLElement.Actions[i].Id,
                                        Type = EnumWebHTMLElementActionType.GetINNERText,
                                        ResultString = innerText,
                                        ResultBool = innerText != null ? true : false
                                    });
                                    break;
                                case EnumWebHTMLElementActionType.GetAttributeValue:
                                    var attributeValue = webHTMLElement.GetINNERText(WebHTMLElement.Actions[i].removeHTMLTags);
                                    result.Add(new ActionReport()
                                    {
                                        Id = WebHTMLElement.Actions[i].Id,
                                        Type = EnumWebHTMLElementActionType.GetAttributeValue,
                                        ResultString = attributeValue,
                                        ResultBool = attributeValue != null ? true : false
                                    });
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("WebDriverService", ex.Message);
            }
            return result;
        }
        //---
        */
        public async Task<List<HTMLWebDriver>> GetWebDrivers()
        {
            try
            {
                if (webDrivers == null)
                    return new List<HTMLWebDriver>();
                return webDrivers;
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("WebDriverService", ex);
            }
            return new List<HTMLWebDriver>();
        }

        public async Task<HTMLWebDriver> GetWebDriverById(Guid WebDriverId)
        {
            try
            {
                var webDriver = webDrivers.FirstOrDefault(item => item.Id == WebDriverId);
                if (webDriver != null)
                    return webDriver;
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("WebDriverService", ex);
            }
            return null;
        }

        public async Task<bool> hasWebDriver(Guid WebDriverId)
        {
            try
            {
                var webDriver = webDrivers.FirstOrDefault(item => item.Id == WebDriverId);
                if (webDriver == null)
                    return false;
                return true;
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("WebDriverService", ex);
            }
            return false;
        }

        public async Task<bool> isUrlContains(Guid WebDriverId, string Text)
        {
            try
            {
                var settings = settingsService.GetServerSettings();
                if ((settings != null) && (await hasWebDriver(WebDriverId).ConfigureAwait(false)))
                {
                    var webDriver = await GetWebDriverById(WebDriverId).ConfigureAwait(false);
                    if ((webDriver != null) && (webDriver.Status != EnumWebDriverStatus.Closed) && (webDriver.Status != EnumWebDriverStatus.Error)
                        && (webDriver.Status != EnumWebDriverStatus.Loading) && (webDriver.WebDriver.Url.IndexOf(Text) != -1))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("WebDriverService", ex);
            }
            return false;
        }

        public async Task<WebHTMLElement> GetElement(Guid WebDriverId, EnumWebHTMLElementSelector Selector, string Link, bool? isRequired = true)
        {
            try
            {
                var settings = settingsService.GetServerSettings();
                if ((settings != null) && (await hasWebDriver(WebDriverId).ConfigureAwait(false)))
                {
                    var webDriver = await GetWebDriverById(WebDriverId).ConfigureAwait(false);
                    if ((webDriver != null) && (webDriver.Status != EnumWebDriverStatus.Closed) && (webDriver.Status != EnumWebDriverStatus.Error)
                        && (webDriver.Status != EnumWebDriverStatus.Loading) && (await hasWebHTMLElement(WebDriverId, Selector, Link).ConfigureAwait(false)))
                    {
                        var element = new WebHTMLElement(webDriver.WebDriver, Selector, Link, isRequired.Value, settings);
                        if (element.isAvailable)
                            return element;
                        else
                            await settingsService.AddWebElementLog(Selector.ToString("F"), Link).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("WebDriverService", ex);
            }
            return null;
        }

        public async Task<bool> hasWebHTMLElement(Guid WebDriverId, EnumWebHTMLElementSelector Selector, string Link, bool? isRequired = true)
        {
            try
            {
                var settings = settingsService.GetServerSettings();
                if ((settings != null) && (await hasWebDriver(WebDriverId).ConfigureAwait(false)))
                {
                    var webDriver = await GetWebDriverById(WebDriverId).ConfigureAwait(false);
                    if ((webDriver != null) && (webDriver.Status != EnumWebDriverStatus.Closed) && (webDriver.Status != EnumWebDriverStatus.Error)
                        && (webDriver.Status != EnumWebDriverStatus.Loading))
                    {
                        var body = new WebHTMLElement(webDriver.WebDriver, EnumWebHTMLElementSelector.TagName, "body", isRequired.Value, settings);
                        if (body.isAvailable)
                        {
                            var element = body.WaitWebHTMLElement(true, Selector, Link, isRequired.Value, settings);
                            return element;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("WebDriverService", ex);
            }
            return false;
        }
        
        public async Task<bool> hasWebHTMLElement(Guid WebDriverId, WebHTMLElement ParentElement, EnumWebHTMLElementSelector Selector, string Link, bool? isRequired = true)
        {
            try
            {
                var settings = settingsService.GetServerSettings();
                if ((settings != null) && (await hasWebDriver(WebDriverId).ConfigureAwait(false)))
                {
                    var webDriver = await GetWebDriverById(WebDriverId).ConfigureAwait(false);
                    if ((webDriver != null) && (webDriver.Status != EnumWebDriverStatus.Closed) && (webDriver.Status != EnumWebDriverStatus.Error)
                        && (webDriver.Status != EnumWebDriverStatus.Loading))
                    {
                        var element = ParentElement.WaitWebHTMLElement(false, Selector, Link, isRequired.Value, settings);
                        return element;
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("WebDriverService", ex);
            }
            return false;
        }

        public async Task<bool> GoToURL(Guid WebDriverId, string URL)
        {
            try
            {
                if (await hasWebDriver(WebDriverId).ConfigureAwait(false))
                {
                    var webDriver = await GetWebDriverById(WebDriverId).ConfigureAwait(false);
                    if ((webDriver != null) && (webDriver.Status != EnumWebDriverStatus.Closed) && (webDriver.Status != EnumWebDriverStatus.Error)
                        && (webDriver.Status != EnumWebDriverStatus.Loading))
                    {
                        webDriver.Status = EnumWebDriverStatus.Loading;
                        var oldURL = webDriver.WebDriver.Url;
                        switch (webDriver.WebDriverPlatform)
                        {
                            case EnumSocialPlatform.Vk:
                                URL = "https://vk.com/" + URL;
                                break;
                        }
                        webDriver.WebDriver.Navigate().GoToUrl(URL);
                        var loadResult = WaitPageLoading(webDriver, oldURL);
                        if (loadResult == EnumWebHTMLPageStatus.Ready)
                        {
                            webDriver.Status = EnumWebDriverStatus.Blocked;
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("WebDriverService", ex);
            }
            return false;
        }

        public async Task<bool> SetWebDriverStatus(Guid WebDriverId, EnumWebDriverStatus Status)
        {
            try
            {
                var webDriver = await GetWebDriverById(WebDriverId).ConfigureAwait(false);
                if ((webDriver != null) && (webDriver.Status != EnumWebDriverStatus.Closed) && (webDriver.Status != EnumWebDriverStatus.Error)
                        && (webDriver.Status != EnumWebDriverStatus.Loading))
                {
                    webDrivers[webDrivers.IndexOf(webDriver)].Status = Status;
                    return true;
                }
            }
            catch(Exception ex)
            {
                await settingsService.AddLog("WebDriverService", ex);
            }
            return false;
        }

        public async Task GetScreenshot(Guid WebDriverId, int BotClientRoleConnectionId, string ScreenshotName)
        {
            try
            {
                var webDriver = await GetWebDriverById(WebDriverId).ConfigureAwait(false);
                if ((webDriver != null) && (webDriver.Status != EnumWebDriverStatus.Closed) && (webDriver.Status != EnumWebDriverStatus.Error)
                    && (webDriver.Status != EnumWebDriverStatus.Loading))
                {
                    Screenshot image = ((ITakesScreenshot)webDriver.WebDriver).GetScreenshot();
                    ScreenshotName = ScreenshotName.Replace('-', '_');
                    var folderPath = await settingsService.GetScreenshotFolderPath(BotClientRoleConnectionId.ToString());
                    if (folderPath != null)
                    {
                        image.SaveAsFile($"{folderPath}\\{ScreenshotName}.png", ScreenshotImageFormat.Png);
                        var dialogScreenshotCreate = new DialogScreenshotCreateModel()
                        {
                            BotClientRoleConnectionId = BotClientRoleConnectionId,
                            ScreenshotCount = 1
                        };
                        var createDialogScreenshotResult = dialogScreenshotService.CreateDialogScreenshot(dialogScreenshotCreate);
                        if(createDialogScreenshotResult.HasError)
                            await settingsService.AddLog("WebDriverService", createDialogScreenshotResult.ExceptionMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("WebDriverService", ex);
            }
        }

        public async Task GoToMainPage(Guid WebDriverId)
        {
            try
            {
                var webDriver = await GetWebDriverById(WebDriverId).ConfigureAwait(false);
                if ((webDriver != null) && (webDriver.Status != EnumWebDriverStatus.Closed) && (webDriver.Status != EnumWebDriverStatus.Error)
                    && (webDriver.Status != EnumWebDriverStatus.Loading))
                {
                    string url = null;
                    switch (webDriver.WebDriverPlatform)
                    {
                        case EnumSocialPlatform.Vk:
                            url = "https://vk.com/";
                            break;
                        default:
                            break;
                    }
                    if (url != null)
                    {
                        var oldURL = webDriver.WebDriver.Url;
                        webDriver.WebDriver.Navigate().GoToUrl(url);
                        var loadResult = WaitPageLoading(webDriver, oldURL);
                    }
                }
            }
            catch (Exception ex)
            {
                await settingsService.AddLog("WebDriverService", ex);
            }
        }
        private EnumWebHTMLPageStatus WaitPageLoading(HTMLWebDriver WebDriver, string OldURL)
        {
            bool urlNotChanged = false;
            if (OldURL == WebDriver.WebDriver.Url)
                urlNotChanged = true;
            try
            {
                for (int i = 0; i < WebDriver.WebSettings.HTMLPageWaitingTime; i++)
                {
                    try
                    {
                        var js = (IJavaScriptExecutor)WebDriver.WebDriver;
                        object o = js.ExecuteScript("return document.readyState;");
                        var readyState = (string)o;
                        if ((readyState == "complete" || readyState == "interactive") && ((urlNotChanged) || (OldURL != WebDriver.WebDriver.Url)))
                            return EnumWebHTMLPageStatus.Ready;
                    }
                    catch
                    {
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("WebDriverService", ex);
            }
            return EnumWebHTMLPageStatus.Loading;
        }

        private Tuple<HTMLWebDriver, DriverReport> StartWebDriver(EnumSocialPlatform SocialPlatform)
        {
            try
            {
                var bufferWebDriver = new HTMLWebDriver(SocialPlatform, settingsService.GetServerSettings());
                if (bufferWebDriver.Status == EnumWebDriverStatus.Start)
                {
                    var oldURL = bufferWebDriver.WebDriver.Url;
                    var loadingResult = WaitPageLoading(bufferWebDriver, oldURL);
                    var driverReport = new DriverReport()
                    {
                        ServerId = settingsService.GetServerSettings().ServerId,
                        DriverId = bufferWebDriver.Id,
                        DriverStatus = loadingResult == EnumWebHTMLPageStatus.Ready ? bufferWebDriver.Status : EnumWebDriverStatus.Error
                    };
                    if (bufferWebDriver.Status == EnumWebDriverStatus.Error)
                    {
                        driverReport.HasError = true;
                        driverReport.ExceptionMessage = bufferWebDriver.ExceptionMessage;
                    }
                    return Tuple.Create(bufferWebDriver, driverReport);
                }
                else
                {
                    settingsService.AddLog("WebDriverService", bufferWebDriver.ExceptionMessage);
                }
            }
            catch(Exception ex)
            {
                settingsService.AddLog("WebDriverService", ex);
            }
            return null;
        }

    }
}
