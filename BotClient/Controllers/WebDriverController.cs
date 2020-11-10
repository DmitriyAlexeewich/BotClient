using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BotClient.Bussines.Interfaces;
using BotClient.Models.Enumerators;
using BotClient.Models.HTMLElements;
using BotClient.Models.WebReports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BotClient.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WebDriverController : ControllerBase
    {
        private readonly IWebDriverService webDriverService;

        public WebDriverController(IWebDriverService WebDriverService)
        {
            webDriverService = WebDriverService;
        }

        [HttpGet("Start")]
        public async Task<IActionResult> Start([FromQuery] int BrowserCount, [FromQuery] int SocialPlatform)
        {
            var message = "";
            if (BrowserCount > 0)
            {
                if ((EnumSocialPlatform)SocialPlatform != 0)
                {
                    Task.Run(() => { webDriverService.Start(BrowserCount, (EnumSocialPlatform)SocialPlatform); });
                    return Ok(new DriverStartReport()
                    {
                        IsSuccess = true,
                        BrowserCount = BrowserCount,
                        SocialPlatform = (EnumSocialPlatform)SocialPlatform
                    });
                }
                else
                {
                    message = $"Invalid SocialPlatform, SocialPlatform must be greater than 0 and " +
                        $"be less than {Enum.GetValues(typeof(EnumSocialPlatform)).Cast<EnumSocialPlatform>().Last()}";
                }
            }
            else
                message = "Invalid BrowserCount, BrowserCount must be greater than 0";
            return BadRequest(message);
        }

        [HttpGet("Restart")]
        public async Task<IActionResult> Restart([FromQuery] Guid WebDriverId)
        {
            if (await webDriverService.hasWebDriver(WebDriverId).ConfigureAwait(false))
            {
                var webDriver = await webDriverService.GetWebDriverById(WebDriverId).ConfigureAwait(false);
                Task.Run(() => {webDriverService.Restart(WebDriverId);});
                return Ok(new DriverRestartReport()
                {
                    isSuccess = true,
                    SocialPlatform = webDriver.WebDriverPlatform
                });
            }
            return BadRequest("Invalid WebDriverId");
        }

        [HttpGet("GetWebDrivers")]
        public async Task<IActionResult> GetWebDrivers()
        {
            var drivers = await webDriverService.GetWebDrivers().ConfigureAwait(false);
            var entities = drivers.Select(driver => new
            {
                WebDriverId = driver.Id,
                Status = driver.Status.ToString(),
                Exception = driver.Status != EnumWebDriverStatus.Error ? null : driver.ExceptionMessage
            });
            var result = JsonConvert.SerializeObject(entities, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            return Ok(result);
        }
    }
}
