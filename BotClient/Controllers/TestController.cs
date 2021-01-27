using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BotClient.Bussines.Interfaces;
using BotClient.Models.Bot;
using BotMySQL.Bussines.Interfaces.Composite;
using BotMySQL.Bussines.Interfaces.MySQL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
        private readonly IBotWorkService botWorkService;
        private readonly IPhraseService phraseService;

        public TestController(IBotCompositeService BotCompositeService,
                              IClientCompositeService ClientCompositeService,
                              IMissionCompositeService MissionCompositeService,
                              IWebDriverService WebDriverService,
                              ISettingsService SettingsService,
                              IVkActionService VkActionService,
                              IBotWorkService BotWorkService,
                              IPhraseService PhraseService)
        {
            botCompositeService = BotCompositeService;
            clientCompositeService = ClientCompositeService;
            missionCompositeService = MissionCompositeService;
            webDriverService = WebDriverService;
            settingsService = SettingsService;
            vkActionService = VkActionService;
            botWorkService = BotWorkService;
            phraseService = PhraseService;
        }

        [HttpPost("Test")]
        public async Task<IActionResult> Test([FromBody] string Text)
        {
            for (int i=0; i<100; i++)
                await botWorkService.Test(Text);
            return Ok();
        }
    }
}
