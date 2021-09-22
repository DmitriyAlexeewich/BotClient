using BotClient.Models.Enumerators;
using BotClient.Models.HTMLElements;
using BotClient.Models.HTMLElements.Enumerators;
using BotClient.Models.HTMLWebDriver;
using BotClient.Models.WebReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces
{
    public interface IWebDriverService
    {
        Task Start(int BrowserCount, EnumSocialPlatform SocialPlatform);
        Task Restart(Guid WebDriverId);
        Task Stop(Guid WebDriverId);
        Task<List<HTMLWebDriver>> GetWebDrivers();
        Task<HTMLWebDriver> GetWebDriverById(Guid WebDriverId);
        Task<bool> hasWebDriver(Guid WebDriverId);
        Task<bool> isUrlContains(Guid WebDriverId, string Text);
        Task<WebHTMLElement> GetElement(Guid WebDriverId, EnumWebHTMLElementSelector Selector, string Link, bool? isRequired = true);
        Task<bool> hasWebHTMLElement(Guid WebDriverId, EnumWebHTMLElementSelector Selector, string Link, bool? isRequired = true);
        Task<bool> hasWebHTMLElement(Guid WebDriverId, WebHTMLElement ParentElement, EnumWebHTMLElementSelector Selector, string Link, bool? isRequired = true);
        Task<bool> GoToURL(Guid WebDriverId, string URL);
        Task<bool> SetWebDriverStatus(Guid WebDriverId, EnumWebDriverStatus Status);
        Task GetScreenshot(Guid WebDriverId, int RoleId, int MissionId, int ConnectionId, string ScreenshotName);
        Task GoToMainPage(Guid WebDriverId);
        Task<string> GetCurrentURL(Guid WebDriverId);
        Task<bool> ExecuteJS(Guid WebDriverId, string JSText);
        /*
        Task ExecuteAlgoritm(Guid WebDriverId, List<WebHTMLElementModel> WebHTMLElementModels);
        Task<List<ActionReport>> ExecuteCommand(Guid WebDriverId, WebHTMLElementModel WebHTMLElement);*/
    }
}
