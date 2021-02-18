using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BotClient.Bussines.Interfaces;
using BotClient.Models.Client;
using BotClient.Models.WebReports;
using BotMySQL.Bussines.Interfaces.Composite;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BotClient.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BotController : ControllerBase
    {

        private readonly IBotWorkService botWorkService;
        private readonly IBotCompositeService botCompositeService;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor httpContextAccessor;

        public BotController(IBotWorkService BotWorkService, 
                             IBotCompositeService BotCompositeService,
                             IWebHostEnvironment WebHostEnvironment,
                             IHttpContextAccessor HttpContextAccessor)
        {
            botWorkService = BotWorkService;
            botCompositeService = BotCompositeService;
            webHostEnvironment = WebHostEnvironment;
            httpContextAccessor = HttpContextAccessor;
        }

        [HttpGet("StartBot")]
        public async Task<IActionResult> StartBot([FromQuery] int ServerId, [FromQuery] int BotCount)
        {
            var message = "Invalid ServerId. The ServerId is empty";
            if (ServerId != null)
            {
                if (ServerId > 0)
                    return Ok(await botWorkService.StartBot(ServerId, BotCount).ConfigureAwait(false) == true ? "Success" : "Error");
                else
                    message = "Invalid BotsId. Bot id list contains no elements";
            }
            return BadRequest(message);
        }

        [HttpGet("GetBot")]
        public async Task<IActionResult> GetBot()
        {
            return Ok(botCompositeService.GetBotById(5));
        }

        [HttpPost("GetScreenshotsLink")]
        public async Task<IActionResult> GetScreenshot(List<GetScreenshotURLModel> Dialogs)
        {
            var links = new List<string>();
            for (int i = 0; i < Dialogs.Count; i++)
            {
                if(Dialogs[i].DialogId > 0)
                    links.Add($"http://{httpContextAccessor.HttpContext.Request.Host.Value}/Bot/DownloadScreenshot/{Dialogs[i].DialogId}");
            }
            return Ok(links);
        }

        [HttpGet("DownloadScreenshot/{Id}")]
        public async Task<IActionResult> DownloadScreenshot(int Id)
        {
            var screenshotDirectory = new DirectoryInfo("C:\\Screenshot\\" + Id);
            var screenshotFile = (from item in screenshotDirectory.GetFiles()
                                  orderby item.LastWriteTime descending
                                  select item).First();
            return PhysicalFile(screenshotFile.FullName, "image/png", screenshotFile.Name);
        }
    }
}
