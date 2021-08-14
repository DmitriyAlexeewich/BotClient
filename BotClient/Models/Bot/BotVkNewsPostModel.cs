using BotClient.Models.HTMLElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.Bot
{
    public class BotVkNewsPostModel
    {
        public string NewsVkId { get; set; }
        public WebHTMLElement CommentInput { get; set; }
        public WebHTMLElement SendBtn { get; set; }
        public List<BotVkNewsPostCommentModel> Comments { get; set; }
    }
}
