using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BotClient.Bussines.Interfaces;
using BotClient.Models.WebReports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BotClient.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BotController : ControllerBase
    {

        private readonly IBotWorkService botWorkService;

        public BotController(IBotWorkService BotWorkService)
        {
            botWorkService = BotWorkService;
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
    }
}
