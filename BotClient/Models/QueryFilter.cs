using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models
{
    public class QueryFilter
    {
        public string Table { get; set; }
        public string Column { get; set; }
        public string Filter { get; set; }
    }
}
