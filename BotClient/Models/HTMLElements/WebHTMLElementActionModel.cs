using System;
using BotClient.Models.HTMLElements.Enumerators;

namespace BotClient.Models.HTMLElements
{
    public class WebHTMLElementActionModel
    {
        public Guid Id { get; set; }
        public EnumWebHTMLElementActionType ActionType { get; set; }
        public EnumWebHTMLElementName AlgoritmActionType { get; set; } = 0;
        public EnumClickType ClickType { get; set; } = 0;
        public string Text { get; set; } = string.Empty;
        public bool isParentElementIsDriver { get; set; } = false;
        public WebHTMLElementModel ElementModel { get; set; } = null;
        public string AttributeName { get; set; } = string.Empty;
        public string AttributeValue { get; set; } = string.Empty;
        public bool removeHTMLTags { get; set; } = false;
    }
}
