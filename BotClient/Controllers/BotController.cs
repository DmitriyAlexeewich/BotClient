using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BotClient.Bussines.Interfaces;
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
        private readonly IWebHostEnvironment appEnvironment;

        public BotController(IBotWorkService BotWorkService, 
                             IBotCompositeService BotCompositeService,
                             IWebHostEnvironment AppEnvironment)
        {
            botWorkService = BotWorkService;
            botCompositeService = BotCompositeService;
            appEnvironment = AppEnvironment;
        }

        [HttpPost("StartBot")]
        public async Task<IActionResult> StartBot([FromBody] List<int> BotsId)
        {
            var message = "Invalid BotsId. The list of bot IDs is empty";
            if (BotsId != null)
            {
                if (BotsId.Count > 0)
                    return Ok(await botWorkService.StartBot(BotsId).ConfigureAwait(false));
                else
                    message = "Invalid BotsId. Bot id list contains no elements";
            }
            return BadRequest(message);
        }

        [HttpPost("StopBot")]
        public async Task<IActionResult> StopBot([FromQuery] List<int> BotsId)
        {
            var message = "Invalid BotsId. The list of bot IDs is empty";
            if (BotsId != null)
            {
                if (BotsId.Count > 0)
                    return Ok(await botWorkService.StopBot(BotsId).ConfigureAwait(false));
                else
                    message = "Invalid BotsId. Bot id list contains no elements";
            }
            return BadRequest(message);
        }

        [HttpGet("GetBots")]
        public async Task<IActionResult> GetBots()
        {
            return Ok(await botWorkService.GetBots().ConfigureAwait(false));
        }

        [HttpGet("GetBot")]
        public async Task<IActionResult> GetBot()
        {
            return Ok(botCompositeService.GetBotById(5));
        }

        [HttpGet("GetScreenshot")]
        public async Task<IActionResult> GetScreenshot()
        {
            string file_path = "C:\\Screenshot\\41244\\2020_12_01.png";
            // Тип файла - content-type
            string file_type = "image/png";
            // Имя файла - необязательно
            string file_name = "2020_12_01.png";
            var t = new List<PhysicalFileResult>();
            t.Add(PhysicalFile(file_path, file_type, file_name));
            return Ok(t);
        }
    }
}
