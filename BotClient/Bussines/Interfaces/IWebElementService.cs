using BotClient.Models.HTMLElements;
using BotClient.Models.HTMLElements.Enumerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Interfaces
{
    public interface IWebElementService
    {
        bool ClickToElement(WebHTMLElement Element, EnumClickType ClickType);
        Task<bool> ClickToElement(Guid WebDriverId, EnumWebHTMLElementSelector Selector, string Link, EnumClickType ClickType, bool? isElementRequired = true);
        bool PrintTextToElement(WebHTMLElement Element, string Text);
        Task<bool> PrintTextToElement(Guid WebDriverId, EnumWebHTMLElementSelector Selector, string Link, string Text, bool? isElementRequired = true);
        bool SendKeyToElement(WebHTMLElement Element, string KeyName);
        Task<bool> SendKeyToElement(Guid WebDriverId, EnumWebHTMLElementSelector Selector, string Link, string KeyName, bool? isElementRequired = true);
        bool ClearElement(WebHTMLElement Element);
        Task<bool> ClearElement(Guid WebDriverId, EnumWebHTMLElementSelector Selector, string Link, bool? isElementRequired = true);
        bool ScrollElement(WebHTMLElement Element);
        Task<bool> ScrollElement(Guid WebDriverId, EnumWebHTMLElementSelector Selector, string Link, bool? isElementRequired = true);
        bool ScrollToElement(WebHTMLElement Element);
        Task<bool> ScrollToElement(Guid WebDriverId, EnumWebHTMLElementSelector Selector, string Link, bool? isElementRequired = true);
        bool ScrollElementJs(WebHTMLElement Element);
        Task<bool> ScrollElementJs(Guid WebDriverId, EnumWebHTMLElementSelector Selector, string Link, bool? isElementRequired = true);
        bool CompareElementAttribute(WebHTMLElement Element, string AttributeName, string AttributeValue);
        Task<bool> CompareElementAttribute(Guid WebDriverId, EnumWebHTMLElementSelector Selector, string Link, string AttributeName, string AttributeValue, bool? isElementRequired = true);
        string GetElementINNERText(WebHTMLElement Element, bool? removeHTMLTags = false);
        Task<string> GetElementINNERText(Guid WebDriverId, EnumWebHTMLElementSelector Selector, string Link, bool? removeHTMLTags = false, bool? isElementRequired = true);
        string GetAttributeValue(WebHTMLElement Element, string AttributeName);
        Task<string> GetAttributeValue(Guid WebDriverId, EnumWebHTMLElementSelector Selector, string Link, string AttributeName, bool? isElementRequired = true);
        bool isElementAvailable(WebHTMLElement Element);
        WebHTMLElement GetElementInElement(WebHTMLElement Element, EnumWebHTMLElementSelector Selector, string Link, bool? isRequired = true);
        Task<WebHTMLElement> GetElementInElement(Guid WebDriverId, EnumWebHTMLElementSelector ParentSelector, string ParentLink, EnumWebHTMLElementSelector Selector, string Link, bool? isRequired = true);
        List<WebHTMLElement> GetChildElements(WebHTMLElement Element, EnumWebHTMLElementSelector SelectorType, string Link, bool? isRequired = true);
        Task<List<WebHTMLElement>> GetChildElements(Guid WebDriverId, EnumWebHTMLElementSelector ParentSelectorType, string ParentLink, EnumWebHTMLElementSelector SelectorType, string Link, bool? isRequired = true);
    }
}
