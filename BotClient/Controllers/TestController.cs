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
    public class TestController : ControllerBase
    {
        private IBotService botService;

        public TestController(IBotService bot)
        {
            botService = bot;
        }

        [HttpGet("Test")]
        public async Task<IActionResult> Test()
        {
            var t = await botService.GetById(1).ConfigureAwait(false);
            t = null;
            return Ok();
        }
    }
}
