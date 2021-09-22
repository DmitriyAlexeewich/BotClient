using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BotClient.Bussines.Interfaces;
using BotClient.Models.Enumerators;
using BotClient.Models.HTMLElements;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BotClient.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class SettingsController : ControllerBase
    {
        private readonly ISettingsService settingsService;

        public SettingsController(ISettingsService SettingsService)
        {
            settingsService = SettingsService;
        }
        [HttpGet("GetServerSettings")]
        public async Task<IActionResult> GetServerSettings()
        {
            return Ok(settingsService.GetServerSettings());
        }

    }
}
