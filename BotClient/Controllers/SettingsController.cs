using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BotClient.Bussines.Interfaces;
using BotClient.Models.Enumerators;
using BotClient.Models.HTMLElements;
using BotClient.Models.Settings;
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

        [HttpPost("CreateSettings")]
        public async Task<IActionResult> CreateSettings([FromBody] WebConnectionSettings Settings)
        {
            if ((Settings.ParentServerIP != null) && (Settings.ParentServerIP.Length > 0) && (Settings.ServerId != null) && (Settings.Options != null)
                && (Settings.Options.Count > 0) && (Settings.KeyWaitingTimeMin >= 0) && (Settings.KeyWaitingTimeMax > 0) 
                && (Settings.KeyWaitingTimeMin < Settings.KeyWaitingTimeMax) && (Settings.HTMLPageWaitingTime >= 60) 
                && (Settings.HTMLElementWaitingTime >= 1) && (Settings.ScrollCount >= 1) && (Settings.ErrorChanceInWords >= 1) 
                && (Settings.ErrorChanceInWords <= 100) && (Settings.CapsChancePerThousandWords >= 1) 
                && (Settings.NumberChancePerHundredWords <= 100) && (Settings.MusicWaitingTime >= 60) 
                && (Settings.MusicWaitingTime <= 360) && (Settings.MusicWaitingDeltaTime > 0) 
                && (Settings.MusicWaitingDeltaTime <= 240) && (Settings.VideoWaitingTime >= 60) 
                && (Settings.VideoWaitingTime <= 360) && (Settings.VideoWaitingDeltaTime > 0) 
                && (Settings.VideoWaitingDeltaTime <= 240) && (Settings.VideoLoadingWaitingTime >= 60))
            {
                return Ok(await settingsService.CreateLink(Settings).ConfigureAwait(false));
            }
            return BadRequest("Invalid Settings");
        }
        /*
        [HttpPost("SetServerId")]
        public async Task<IActionResult> SetServerId([FromQuery] Guid ServerId)
        {
            if (ServerId != null)
            {
                return Ok(await settingsService.SetServerId(ServerId).ConfigureAwait(false));
            }
            return BadRequest("Invalid ServerId");
        }
        */
        [HttpPost("SetParentServerIP")]
        public async Task<IActionResult> SetParentServerIP([FromQuery] string ParentServerIP)
        {
            if ((ParentServerIP != null) && (IPAddress.TryParse(ParentServerIP, out IPAddress address)))
            {
                return Ok(await settingsService.SetParentServerIP(ParentServerIP).ConfigureAwait(false));
            }
            return BadRequest("Invalid ParentServerIP");
        }

        [HttpPost("SetBrowserOptions")]
        public async Task<IActionResult> SetBrowserOptions([FromBody] List<string> Options)
        {
            if ((Options != null)&& (Options.Count>0))
            {
                return Ok(await settingsService.SetBrowserOptions(Options).ConfigureAwait(false));
            }
            return BadRequest("Invalid Options");
        }

        [HttpPost("SetKeyWaitingTime")]
        public async Task<IActionResult> SetKeyWaitingTime([FromQuery] int KeyWaitingTimeMin, int KeyWaitingTimeMax)
        {
            if ((KeyWaitingTimeMin >= 0) && (KeyWaitingTimeMax > 0) && (KeyWaitingTimeMin < KeyWaitingTimeMax))
            {
                return Ok(await settingsService.SetKeyWaitingTime(KeyWaitingTimeMin, KeyWaitingTimeMax).ConfigureAwait(false));
            }
            return BadRequest("Invalid KeyWaitingTimeMin or KeyWaitingTimeMax. " +
                              "KeyWaitingTimeMin must be greater than 0 and be less than KeyWaitingTimeMax. " +
                              "KeyWaitingTimeMax must be greater than 0 and KeyWaitingTimeMin.");
        }
        
        [HttpPost("SetHTMLPageWaitingTime")]
        public async Task<IActionResult> SetHTMLPageWaitingTime([FromQuery] int HTMLPageWaitingTime)
        {
            if (HTMLPageWaitingTime >= 60)
            {
                return Ok(await settingsService.SetHTMLPageWaitingTime(HTMLPageWaitingTime).ConfigureAwait(false));
            }
            return BadRequest("Invalid HTMLPageWaitingTime." +
                              "HTMLPageWaitingTime must be greater than 60");
        }

        [HttpPost("SetHTMLElementWaitingTime")]
        public async Task<IActionResult> SetHTMLElementWaitingTime([FromQuery] int HTMLElementWaitingTime)
        {
            if ((HTMLElementWaitingTime >= 1))
            {
                return Ok(await settingsService.SetHTMLElementWaitingTime(HTMLElementWaitingTime).ConfigureAwait(false));
            }
            return BadRequest("Invalid HTMLElementWaitingTime. " +
                              "HTMLElementWaitingTime must be greater than 0");
        }

        [HttpPost("SetScrollCount")]
        public async Task<IActionResult> SetScrollCount([FromQuery] int ScrollCount)
        {
            if (ScrollCount >= 1)
            {
                return Ok(await settingsService.SetScrollCount(ScrollCount).ConfigureAwait(false));
            }
            return BadRequest("Invalid ScrollCount. " +
                              "ScrollCount must be greater than 0");
        }

        [HttpPost("SetErrorChancePerTenWords")]
        public async Task<IActionResult> SetErrorChancePerTenWords([FromQuery] int ErrorChancePerTenWords)
        {
            if ((ErrorChancePerTenWords >= 1) && (ErrorChancePerTenWords <= 100))
            {
                return Ok(await settingsService.SetErrorChancePerTenWords(ErrorChancePerTenWords).ConfigureAwait(false));
            }
            return BadRequest("Invalid ErrorChancePerTenWords. " +
                              "ErrorChancePerTenWords must be greater than 0 and must be less than or equal to 100");
        }

        [HttpPost("SetCapsChancePerThousandWords")]
        public async Task<IActionResult> SetCapsChancePerThousandWords([FromQuery] int CapsChancePerThousandWords)
        {
            if ((CapsChancePerThousandWords >= 1) && (CapsChancePerThousandWords <= 100))
            {
                return Ok(await settingsService.SetCapsChancePerThousandWords(CapsChancePerThousandWords).ConfigureAwait(false));
            }
            return BadRequest("Invalid ErrorChancePerTenWords. " +
                              "CapsChancePerThousandWords must be greater than 0 and must be less than or equal to 100");
        }

        [HttpPost("SetNumberChancePerHundredWords")]
        public async Task<IActionResult> SetNumberChancePerHundredWords([FromQuery] int NumberChancePerHundredWords)
        {
            if ((NumberChancePerHundredWords >= 1) && (NumberChancePerHundredWords <= 100))
            {
                return Ok(await settingsService.SetNumberChancePerHundredWords(NumberChancePerHundredWords).ConfigureAwait(false));
            }
            return BadRequest("Invalid ErrorChancePerTenWords. " +
                              "NumberChancePerHundredWords must be greater than 0 and must be less than or equal to 100");
        }

        [HttpPost("SetMusicWaitingTime")]
        public async Task<IActionResult> SetMusicWaitingTime([FromQuery] int MusicWaitingTime, [FromQuery] int MusicWaitingDeltaTime)
        {
            if ((MusicWaitingTime >= 60) && (MusicWaitingTime <= 360) && (MusicWaitingDeltaTime > 0) && (MusicWaitingDeltaTime <= 240))
            {
                return Ok(await settingsService.SetMusicWaitingTime(MusicWaitingTime * 1000, MusicWaitingDeltaTime * 1000).ConfigureAwait(false));
            }
            return BadRequest("Invalid MusicWaitingTime or MusicWaitingDeltaTime. " +
                              "MusicWaitingTime must be greater than 60 and be less than 360. " +
                              "MusicWaitingDeltaTime must be greater than 0 and 240.");
        }

        [HttpPost("SetVideoWaitingTime")]
        public async Task<IActionResult> SetVideoWaitingTime([FromQuery] int VideoWaitingTime, [FromQuery] int VideoWaitingDeltaTime)
        {
            if ((VideoWaitingTime >= 60) && (VideoWaitingTime <= 360) && (VideoWaitingDeltaTime > 0) && (VideoWaitingDeltaTime <= 240))
            {
                return Ok(await settingsService.SetVideoWaitingTime(VideoWaitingTime * 1000, VideoWaitingDeltaTime * 1000).ConfigureAwait(false));
            }
            return BadRequest("Invalid VideoWaitingTime or VideoWaitingDeltaTime. " +
                              "VideoWaitingTime must be greater than 60 and be less than 360. " +
                              "VideoWaitingDeltaTime must be greater than 0 and 240.");
        }

        [HttpPost("SetVideoLoadingWaitingTime")]
        public async Task<IActionResult> SetVideoLoadingWaitingTime([FromQuery] int VideoLoadingWaitingTime)
        {
            if (VideoLoadingWaitingTime >= 60)
            {
                return Ok(await settingsService.SetVideoLoadingWaitingTime(VideoLoadingWaitingTime * 1000).ConfigureAwait(false));
            }
            return BadRequest("Invalid VideoLoadingWaitingTime. " +
                              "VideoLoadingWaitingTime must be greater than 60.");
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
