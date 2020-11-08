using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.Bot
{
    public class BotCustomizeModel
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public int BotId { get; set; }
        public string Coutry { get; set; }
        public string City { get; set; }
        public string MobilePhone { get; set; }
        public string HomePhone { get; set; }
        public string Skype { get; set; }
        public string JobSite { get; set; }
        public string Job { get; set; }
        public string Interest { get; set; }
        public string FavoriteMusic { get; set; }
        public string FavoriteFilms { get; set; }
        public string FavoriteTVShows { get; set; }
        public string FavoriteBook { get; set; }
        public string FavoriteGame { get; set; }
        public string FavoriteQuote { get; set; }
        public string PersonalInfo { get; set; }
        public string Grands { get; set; }
        public string Parents { get; set; }
        public string SistersAndBrothers { get; set; }
        public string Childs { get; set; }
        public string GrandChilds { get; set; }
    }
}
