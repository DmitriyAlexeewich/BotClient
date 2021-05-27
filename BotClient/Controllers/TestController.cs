using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BotClient.Bussines.Interfaces;
using BotClient.Models.Bot;
using BotDataModels.Bot.Enumerators;
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
        private readonly ITextService textService;

        public TestController(IBotCompositeService BotCompositeService,
                              IClientCompositeService ClientCompositeService,
                              IMissionCompositeService MissionCompositeService,
                              IWebDriverService WebDriverService,
                              ISettingsService SettingsService,
                              IVkActionService VkActionService,
                              IBotWorkService BotWorkService,
                              IPhraseService PhraseService,
                              ITextService TextService)
        {
            botCompositeService = BotCompositeService;
            clientCompositeService = ClientCompositeService;
            missionCompositeService = MissionCompositeService;
            webDriverService = WebDriverService;
            settingsService = SettingsService;
            vkActionService = VkActionService;
            botWorkService = BotWorkService;
            phraseService = PhraseService;
            textService = TextService;
        }

        [HttpPost("Test")]
        public async Task<IActionResult> Test()
        {
            var nowTime = new DateTime(2021, 1, 31, 23, 59, 59, 0);
            var nextTime = nowTime.AddMilliseconds(6000);
            var timer = DateTime.Now.AddMilliseconds(6000);
            while (DateTime.Now < timer) { }
            return Ok();
        }

    }
}
