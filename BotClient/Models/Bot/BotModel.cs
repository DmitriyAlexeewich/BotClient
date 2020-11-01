using BotClient.Models.Bot.Enumerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.Bot
{
    public class BotModel
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public bool isMale { get; set; }
        public EnumGender Gender { get; set; }
        public int Age { get; set; }
        public string VkId { get; set; }
        public string FullName { get; set; }
        public DateTime OnlineDate { get; set; }
        public bool isDead { get; set; }
        public bool isPrintBlock { get; set; }
        public bool isLogin { get; set; }
        public bool isUpdatedCusomizeInfo { get; set; }
    }
}
