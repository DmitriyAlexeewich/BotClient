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
        Task<bool> StartBot(int ServerId, int BotCount);
        Task Test(string Text);
    }
}
