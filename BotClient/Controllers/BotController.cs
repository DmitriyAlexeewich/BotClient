using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BotClient.Bussines.Interfaces;
using BotClient.Models.Client;
using BotClient.Models.WebReports;
using BotFile.Bussines.Interfaces;
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
        private readonly IFileService fileService;

        public BotController(IBotWorkService BotWorkService, 
                             IBotCompositeService BotCompositeService,
                             IWebHostEnvironment WebHostEnvironment,
                             IHttpContextAccessor HttpContextAccessor,
                              IFileService FileService)
        {
            botWorkService = BotWorkService;
            botCompositeService = BotCompositeService;
            webHostEnvironment = WebHostEnvironment;
            httpContextAccessor = HttpContextAccessor;
            fileService = FileService;
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

        [HttpGet("DownloadScreenshot")]
        public async Task<IActionResult> DownloadScreenshot([FromQuery] int RoleId, [FromQuery] int BotClientRoleConnectionId)
        {
            var filePath = fileService.ArchiveFile(fileService.GetScreenshotDirectoryPath(RoleId.ToString(), BotClientRoleConnectionId.ToString()), BotClientRoleConnectionId.ToString());
            if (filePath.Length > 0)
            {
                var fileDirectory = new DirectoryInfo(filePath);
                return PhysicalFile(fileDirectory.FullName, "application/zip", fileDirectory.Name);
            }
            return BadRequest();
        }
    }
}
