using BotClient.Models.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.Client
{
    public class ClientCompositeModel
    {
        public int Id { get; set; }
        public string VkId { get; set; }
        public string FullName { get; set; }
        public int BotClientConnectionId { get; set; }
        public int BotId { get; set; }
        public int RoleId { get; set; }
        public int CurrentMissionId { get; set; }
        public string MissionPath { get; set; }
        public bool isComplete { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
    }
}
