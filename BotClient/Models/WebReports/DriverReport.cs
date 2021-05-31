using BotClient.Models.Enumerators;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Policy;

namespace BotClient.Models.WebReports
{
    public class DriverReport
    {
        public int ServerId { get; set; }
        public Guid DriverId { get; set; }
        public bool HasError { get; set; } = false;
        public EnumWebDriverStatus DriverStatus { get; set; }
        public string IP { get; } = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1].ToString();
        public Url URL { get; set; }
        public string LocalDateTime { get; } = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss \"GMT\" zzz");
        public Exception ExceptionMessage { get; set; }
        public List<ActionReport> ActionsReport { get; set; } = new List<ActionReport>();
    }
}
