using BotClient.Bussines.Interfaces;
using BotClient.Models.HTMLElements;
using BotClient.Models.HTMLElements.Enumerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services
{
    public class WebElementService : IWebElementService
    {
        private readonly IWebDriverService webDriverService;
        private readonly ISettingsService settingsService;

        public WebElementService(IWebDriverService WebDriverService,
                                 ISettingsService SettingsService)
        {
            webDriverService = WebDriverService;
            settingsService = SettingsService;
        }



        public bool ClickToElement(WebHTMLElement Element, EnumClickType ClickType)
        {
            if ((Element != null) && (ClickType != null) && (ClickType != 0))
                return Element.Click(ClickType);
            return false;
        }

        public async Task<bool> ClickToElement(Guid WebDriverId, EnumWebHTMLElementSelector Selector, string Link, EnumClickType ClickType, bool? isElementRequired = true)
        {
            var element = await webDriverService.GetElement(WebDriverId, Selector, Link, isElementRequired).ConfigureAwait(false);
            if (element == null)
                await settingsService.AddWebElementLog(Selector.ToString("F"), Link).ConfigureAwait(false);
            return ClickToElement(element, ClickType);
        }

        public bool PrintTextToElement(WebHTMLElement Element, string Text)
        {
            if ((Element != null) && (Text.Length > 0))
                return Element.PrintText(Text);
            return false;
        }

        public bool SendKeyToElement(WebHTMLElement Element, string KeyName)
        {
            if ((Element != null) && (KeyName.Length > 0))
                return Element.SendKey(KeyName);
            return false;
        }

        public bool ClearElement(WebHTMLElement Element)
        {
            if (Element != null)
                return Element.Clear();
            return false;
        }

        public async Task<bool> ClearElement(Guid WebDriverId, EnumWebHTMLElementSelector Selector, string Link, bool? isElementRequired = true)
        {
            var element = await webDriverService.GetElement(WebDriverId, Selector, Link, isElementRequired).ConfigureAwait(false);
            if (element == null)
                await settingsService.AddWebElementLog(Selector.ToString("F"), Link).ConfigureAwait(false);
            return ClearElement(element);
        }

        public async Task<bool> PrintTextToElement(Guid WebDriverId, EnumWebHTMLElementSelector Selector, string Link, string Text, bool? isElementRequired = true)
        {
            var element = await webDriverService.GetElement(WebDriverId, Selector, Link, isElementRequired).ConfigureAwait(false);
            if (element == null)
                await settingsService.AddWebElementLog(Selector.ToString("F"), Link).ConfigureAwait(false);
            return PrintTextToElement(element, Text);
        }

        public async Task<bool> SendKeyToElement(Guid WebDriverId, EnumWebHTMLElementSelector Selector, string Link, string KeyName, bool? isElementRequired = true)
        {
            var element = await webDriverService.GetElement(WebDriverId, Selector, Link, isElementRequired).ConfigureAwait(false);
            if (element == null)
                await settingsService.AddWebElementLog(Selector.ToString("F"), Link).ConfigureAwait(false);
            return SendKeyToElement(element, KeyName);
        }

        public bool ScrollElement(WebHTMLElement Element)
        {
            if (Element != null)
                return Element.Scroll();
            return false;
        }

        public async Task<bool> ScrollElement(Guid WebDriverId, EnumWebHTMLElementSelector Selector, string Link, string Text, bool? isElementRequired = true)
        {
            var element = await webDriverService.GetElement(WebDriverId, Selector, Link, isElementRequired).ConfigureAwait(false);
            if (element == null)
                await settingsService.AddWebElementLog(Selector.ToString("F"), Link).ConfigureAwait(false);
            return ScrollElement(element);
        }

        public bool CompareElementAttribute(WebHTMLElement Element, string AttributeName, string AttributeValue)
        {
            if ((Element != null) && (AttributeName.Length > 0) && (AttributeValue.Length > 0))
                return Element.CheckAttribute(AttributeName, AttributeValue);
            return false;
        }

        public async Task<bool> CompareElementAttribute(Guid WebDriverId, EnumWebHTMLElementSelector Selector, string Link, string AttributeName, string AttributeValue, bool? isElementRequired = true)
        {
            var element = await webDriverService.GetElement(WebDriverId, Selector, Link, isElementRequired).ConfigureAwait(false);
            if (element == null)
                await settingsService.AddWebElementLog(Selector.ToString("F"), Link).ConfigureAwait(false);
            return CompareElementAttribute(element, AttributeName, AttributeValue);
        }

        public string GetElementINNERText(WebHTMLElement Element, bool? removeHTMLTags = false)
        {
            if (Element != null)
                return Element.GetINNERText(removeHTMLTags.Value);
            return null;
        }

        public async Task<string> GetElementINNERText(Guid WebDriverId, EnumWebHTMLElementSelector Selector, string Link, bool? removeHTMLTags = false, bool? isElementRequired = true)
        {
            var element = await webDriverService.GetElement(WebDriverId, Selector, Link, isElementRequired).ConfigureAwait(false);
            if (element == null)
                await settingsService.AddWebElementLog(Selector.ToString("F"), Link).ConfigureAwait(false);
            return GetElementINNERText(element, removeHTMLTags);
        }

        public string GetAttributeValue(WebHTMLElement Element, string AttributeName)
        {
            if ((Element != null) && (AttributeName.Length > 0))
                return Element.GetAttributeValue(AttributeName);
            return null;
        }

        public async Task<string> GetAttributeValue(Guid WebDriverId, EnumWebHTMLElementSelector Selector, string Link, string AttributeName, bool? isElementRequired = true)
        {
            var element = await webDriverService.GetElement(WebDriverId, Selector, Link, isElementRequired).ConfigureAwait(false);
            if (element == null)
                await settingsService.AddWebElementLog(Selector.ToString("F"), Link).ConfigureAwait(false);
            return GetAttributeValue(element, AttributeName);
        }

        public bool isElementAvailable(WebHTMLElement Element)
        {
            if (Element != null)
                return Element.isAvailable;
            return false;
        }

        public WebHTMLElement GetElementInElement(WebHTMLElement Element, EnumWebHTMLElementSelector Selector, string Link, bool? isRequired = true)
        {
            if ((Element != null) && (Selector != null) && (Link != null) && (Link.Length > 0))
            {
                var element = Element.GetElement(Selector, Link, isRequired.Value);
                return element;
            }
            return null;
        }
        
        public async Task<WebHTMLElement> GetElementInElement(Guid WebDriverId, EnumWebHTMLElementSelector ParentSelector, string ParentLink, EnumWebHTMLElementSelector Selector, string Link, bool? isRequired = true)
        {
            var parentElement = await webDriverService.GetElement(WebDriverId, ParentSelector, ParentLink).ConfigureAwait(false);
            if (parentElement == null)
                await settingsService.AddWebElementLog(Selector.ToString("F"), Link).ConfigureAwait(false);
            if ((parentElement != null) && (Selector != 0) && (Link.Length > 0))
            {
                return GetElementInElement(parentElement, Selector, Link, isRequired);
            }
            return null;
        }

        public List<WebHTMLElement> GetChildElements(WebHTMLElement Element, EnumWebHTMLElementSelector SelectorType, string Link, bool? isRequired = true)
        {
            if ((Element != null) && (SelectorType != 0) && (Link.Length > 0))
            {
                var childs = Element.GetChildElements(SelectorType, Link, isRequired.Value);
                if ((childs != null) && (childs.Count > 0))
                    return childs;
            }
            return new List<WebHTMLElement>();
        }

        public async Task<List<WebHTMLElement>> GetChildElements(Guid WebDriverId, EnumWebHTMLElementSelector SelectorType, string Link, bool? isRequired = true)
        {
            var body = await webDriverService.GetElement(WebDriverId, EnumWebHTMLElementSelector.TagName, "body").ConfigureAwait(false);
            if (body != null)
            {
                var childs = body.GetChildElements(SelectorType, Link, isRequired.Value);
                if ((childs != null) && (childs.Count > 0))
                    return childs;
            }
            return new List<WebHTMLElement>();
        }

    }
}
