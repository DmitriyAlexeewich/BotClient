using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BotClient.Bussines.Interfaces;
using BotClient.Models.Bot;
using BotClient.Models.Enumerators;
using BotClient.Models.FileSystem.Enumerators;
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
        private readonly IFileSystemService fileSystemService;
        private readonly IGoogleDriveService googleDriveService;

        public TestController(IBotCompositeService BotCompositeService,
                              IClientCompositeService ClientCompositeService,
                              IMissionCompositeService MissionCompositeService,
                              IWebDriverService WebDriverService,
                              ISettingsService SettingsService,
                              IVkActionService VkActionService,
                              IBotWorkService BotWorkService,
                              IPhraseService PhraseService,
                              ITextService TextService,
                              IFileSystemService FileSystemService,
                              IGoogleDriveService GoogleDriveService)
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
            fileSystemService = FileSystemService;
            googleDriveService = GoogleDriveService;
        }

        [HttpGet("Test")]
        public async Task<IActionResult> Test()
        {
            googleDriveService.GetFiles();
            return Ok();
        }

    }
}
