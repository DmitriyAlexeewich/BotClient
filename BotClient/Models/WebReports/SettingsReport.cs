using System;
using System.Net;

namespace BotClient.Models.WebReports
{
    public class SettingsReport
    {
        public Guid ServerId { get; set; }
        public bool HasError { get; set; } = false;
        public string IP { get; } = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1].ToString();
        public string LocalDateTime { get; } = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss \"GMT\" zzz");
        public string ExceptionMessage { get; set; }
    }
}
