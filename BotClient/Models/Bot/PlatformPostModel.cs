using BotClient.Models.HTMLElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.Bot
{
    public class PlatformPostModel
    {
        public string PostId { get; }
        public WebHTMLElement Element { get; }
        public int Rating { get; }

        public PlatformPostModel(string NewPostId, WebHTMLElement NewElement, int NewRating)
        {
            PostId = NewPostId;
            Element = NewElement;
            Rating = NewRating;
        }
    }
}
