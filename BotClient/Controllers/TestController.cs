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
        public async Task<IActionResult> Test([FromBody] string Text)
        {
            var f = new List<BotMessageText>();
            for (int i = 0; i < 10; i++)
                f.Add(await textService.RandOriginalMessage(Text).ConfigureAwait(false));
            return Ok(f);
        }

    }
}
