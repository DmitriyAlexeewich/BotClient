﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.Client
{
    public class DialogWithNewMessagesModel
    {
        public string ClientVkId { get; set; }
        public string PlatformLastMessageDate { get; set; }
    }
}
