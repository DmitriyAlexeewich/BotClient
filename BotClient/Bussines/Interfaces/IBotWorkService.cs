﻿using BotClient.Models.Bot;
using BotClient.Models.WebReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces
{
    public interface IBotWorkService
    {
        Task<List<BotWorkStatusModel>> StartBot(int ServerId);
        Task StopBot();
        Task<List<string>> Test(string Text);
    }
}
