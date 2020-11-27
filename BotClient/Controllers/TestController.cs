using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotClient.Bussines.Interfaces;
using BotMySQL.Bussines.Interfaces.Composite;
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

        [HttpGet("Test")]
        public async Task<IActionResult> Test()
        {
            var text = "[73335777,-2001335777,'https://vk.com/mp3/audio_api_unavailable.mp3?extra=Ahv0rg4VANLos19Al2q1otrYEwrJBe5frKTfntDfz29qpvbosgKTDgvIof9UvJu5yO4WqNuToLrWlMDFywful20MzxDTztn0yI1fCZfLmw4XBO1AB2jZEdHRBdrHugXHowO4yJHKnZyOswjftMOYyxHYzxvHAMvxmwuYrKr2ttbFCNrZoeP2zOv1tLPesOnzEg9WlM55sgvRtwvZzOvJnLPJDgi5zwTOmOTJEfGYmgPuuw1MthaVAZeVBLC5mw5Ose9nrgXvDwCZlO9VAt1UvJfIn25JteXVsvr1sJjjx3vxndaUDv9Wp3m4BtvxrNj4v2i3BtK#AqS5oty','Liar Mask','Rika Mayama',293,-1,0,'',0,66,'for_you:daily_recoms','[]','8be772c0fd0ba7f719//f9d19a217be41580eb///b0a4ffa86fecf92f09/','https://sun9-11.userapi.com/impf/c857224/v857224345/1ffcaf/Q-5Wiwpie0Q.jpg?size=80x80&quality=96&sign=7d7dd4ab42a698419f49003d7a4798f2,https://sun9-11.userapi.com/impf/c857224/v857224345/1ffcaf/Q-5Wiwpie0Q.jpg?size=150x150&quality=96&sign=46c93ed9fa8c4824e230a262f55e38be',{'duration':293,'content_id':'-2001335777_73335777','puid22':7,'account_age_type':3,'_SITEID':276,'vk_id':322676110,'ver':251116},'',[{'id':'7943699743681780598','name':'Rika Mayama'}],'',[-2000622091,8622091,'685b2aa446ba809a85'],'5b4a0d197uonw-pdJdz6iL4BFWIn8Q-1ADmq56jGzVyoT09cZAOQgwbo7mAZ-Yld8x8VYjDxD70QPszjq8arEadICUtOEYScH_7PYxjpmTz1HwRfJ-AbzBA5qufiwqtmrU4sR04S274H8c9dEO2SRvo3Bn4Q9SnABjzM_-LGzESETwlHSxWhlAzxuWMM7s895zVxfSOn-BrElZ-1m5E',0,0,true,'95c46a87eb98abf2db',false]";
            var t = JsonConvert.DeserializeObject<string[]>(text);
            return Ok();
        }
    }
}
