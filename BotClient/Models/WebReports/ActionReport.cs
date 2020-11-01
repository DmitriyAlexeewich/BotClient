using BotClient.Models.HTMLElements.Enumerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.WebReports
{
    public class ActionReport
    {
        public Guid Id { get; set; }
        public EnumWebHTMLElementActionType Type { get; set; }
        public bool ResultBool { get; set; } = false;
        public string ResultString { get; set; } = null;
    }
}
