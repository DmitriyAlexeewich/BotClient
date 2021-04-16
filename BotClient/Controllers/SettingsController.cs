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

        [HttpGet("GetServerLogs")]
        public async Task<IActionResult> GetServerLogs()
        {
            var result = await settingsService.GetLogLines().ConfigureAwait(false);
            return Ok(result);
        }
        /*
        [HttpPost("AddUpdateAlgoritm")]
        public async Task<IActionResult> AddUpdateAlgoritm([FromQuery] int AlgoritmId, [FromQuery] int SocialPlatformId, [FromBody] List<WebHTMLElementModel> Algoritm)
        {
            if (((EnumAlgoritmName)AlgoritmId) != 0)
            {
                if (((EnumSocialPlatform)SocialPlatformId) != 0)
                {
                    if ((Algoritm != null) && (Algoritm.Count > 0))
                    {
                        var result = await settingsService.AddUpdateAlgoritm((EnumAlgoritmName)AlgoritmId, (EnumSocialPlatform)SocialPlatformId, Algoritm).ConfigureAwait(false);
                        if (!result.HasError)
                            return Ok();
                        return BadRequest("Something go wrong");
                    }
                    return BadRequest("Invalid Algoritm");
                }
                return BadRequest("Invalid SocialPlatformId");
            }
            return BadRequest("Invalid AlgoritmId");
        }

        [HttpGet("GetAlgoritm")]
        public async Task<IActionResult> GetAlgoritm([FromQuery] int AlgoritmId, [FromQuery] int SocialPlatformId)
        {
            var result = await settingsService.GetAlgoritm((EnumAlgoritmName)AlgoritmId, (EnumSocialPlatform)SocialPlatformId).ConfigureAwait(false);
            if (result == null)
                return BadRequest("None algoritm");
            return Ok(result);
        }
        */

    }
}
