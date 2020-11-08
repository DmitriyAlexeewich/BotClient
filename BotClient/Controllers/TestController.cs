using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BotClient.Bussines.Interfaces;
using BotClient.Bussines.Interfaces.Composite;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BotClient.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IBotCompositeService botCompositeService;
        private readonly IClientCompositeService clientCompositeService;
        private readonly IMissionCompositeService missionCompositeService;
        private readonly IWebDriverService webDriverService;
        private readonly ISettingsService settingsService;
        private readonly IVkActionService vkActionService;

        public TestController(IBotCompositeService BotCompositeService,
                              IClientCompositeService ClientCompositeService,
                              IMissionCompositeService MissionCompositeService,
                              IWebDriverService WebDriverService,
                              ISettingsService SettingsService,
                              IVkActionService VkActionService)
        {
            botCompositeService = BotCompositeService;
            clientCompositeService = ClientCompositeService;
            missionCompositeService = MissionCompositeService;
            webDriverService = WebDriverService;
            settingsService = SettingsService;
            vkActionService = VkActionService;
        }

        [HttpGet("GetBotById")]
        public async Task<IActionResult> GetBotById([FromQuery] int Id)
        {
            return Ok(await botCompositeService.GetBotById(Id).ConfigureAwait(false));
        }

        [HttpGet("GetBotByVkId")]
        public async Task<IActionResult> GetBotByVkId([FromQuery] string Id)
        {
            return Ok(await botCompositeService.GetBotByVkId(Id).ConfigureAwait(false));
        }

        [HttpGet("GetBotCustomize")]
        public async Task<IActionResult> GetBotCustomize([FromQuery] int BotId)
        {
            return Ok(await botCompositeService.GetBotCustomize(BotId).ConfigureAwait(false));
        }

        [HttpGet("GetPatterns")]
        public async Task<IActionResult> GetPatterns([FromQuery] int RoleId)
        {
            return Ok(await botCompositeService.GetPatterns(RoleId).ConfigureAwait(false));
        }
    }
}
