using BotClient.Models.HTMLElements.Enumerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.HTMLElements
{
    public class WebHTMLElementModel
    {
        public int Order { get; set; }
        public EnumWebHTMLElementName ElementName { get; set; }
        public EnumWebHTMLElementSelector SelectorType { get; set; }
        public string Link { get; set; }
        public bool isRequired { get; set; } = false;
        public WebHTMLElementModel ChildElement { get; set; } = null;
    }
}
