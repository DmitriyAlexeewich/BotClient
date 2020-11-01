using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.Role
{
    public class NodePatternModel
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public int MissionId { get; set; }
        public int PatternId { get; set; }
        public int NodeId { get; set; }
        public string Title { get; set; }
        public string PatternText { get; set; }
        public bool isInclude { get; set; }
        public bool isRegex { get; set; }
    }
}
