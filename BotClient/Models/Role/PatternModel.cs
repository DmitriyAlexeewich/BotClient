using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.Role
{
    public class PatternModel
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public int RoleId { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string AnswerText { get; set; }
        public bool isInclude { get; set; }
        public bool isRegex { get; set; }
        public bool isRolePattern { get; set; }
    }
}
