using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.Role
{
    public class BotClientRoleConnectorModel
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public int BotId { get; set; }
        public int ClientId { get; set; }
        public int RoleId { get; set; }
        public int MissionId { get; set; }
        public string MissionPath { get; set; }
        public bool isComplete { get; set; }
        public bool isSuccess { get; set; }
        public bool hasNewMessage { get; set; }
        public bool hasNewBotMessages { get; set; }
    }
}
