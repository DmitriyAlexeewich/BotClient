using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BotClient.Bussines.Interfaces;
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

        [HttpGet("StartBot")]
        public async Task<IActionResult> StartBot([FromBody] List<int> BotsId)
        {
            return Ok(await botWorkService.StartBot(BotsId).ConfigureAwait(false));
        }

        [HttpGet("StopBot")]
        public async Task<IActionResult> StopBot([FromQuery] List<int> BotsId)
        {
            return Ok(await botWorkService.StopBot(BotsId).ConfigureAwait(false));
        }

        [HttpGet("GetBots")]
        public async Task<IActionResult> GetBots()
        {
            return Ok(await botWorkService.GetBots().ConfigureAwait(false));
        }
    }
}
