using BotClient.Models.Role.Enumerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.Role
{
    public class MissionNodeModel
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public int MissionId { get; set; }
        public int NodeId { get; set; }
        public int PatternId { get; set; }
        public string Path { get; set; }
        public string Title { get; set; }
        public EnumMissionActionType Type { get; set; }
        public string Text { get; set; }
        public bool isRequired { get; set; }
    }
}
