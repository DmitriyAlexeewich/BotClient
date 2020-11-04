﻿using BotClient.Bussines.Interfaces;
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
        public WebElementService(IWebDriverService WebDriverService)
        {
            webDriverService = WebDriverService;
        }



        public bool ClickToElement(WebHTMLElement Element, EnumClickType ClickType)
        {
            if (Element != null)
                return Element.Click(ClickType);
            return false;
        }

        public async Task<bool> ClickToElement(Guid WebDriverId, EnumWebHTMLElementSelector Selector, string Link, EnumClickType ClickType, bool? isElementRequired = true)
        {
            var element = await webDriverService.GetElement(WebDriverId, Selector, Link, isElementRequired).ConfigureAwait(false);
            return ClickToElement(element, ClickType);
        }

        public bool PrintTextToElement(WebHTMLElement Element, string Text)
        {
            if (Element != null)
                return Element.PrintText(Text);
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
            return ClearElement(element);
        }

        public async Task<bool> PrintTextToElement(Guid WebDriverId, EnumWebHTMLElementSelector Selector, string Link, string Text, bool? isElementRequired = true)
        {
            var element = await webDriverService.GetElement(WebDriverId, Selector, Link, isElementRequired).ConfigureAwait(false);
            return PrintTextToElement(element, Text);
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
            return ScrollElement(element);
        }

        public bool CompareElementAttribute(WebHTMLElement Element, string AttributeName, string AttributeValue)
        {
            if (Element != null)
                return Element.CheckAttribute(AttributeName, AttributeValue);
            return false;
        }

        public async Task<bool> CompareElementAttribute(Guid WebDriverId, EnumWebHTMLElementSelector Selector, string Link, string AttributeName, string AttributeValue, bool? isElementRequired = true)
        {
            var element = await webDriverService.GetElement(WebDriverId, Selector, Link, isElementRequired).ConfigureAwait(false);
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
            return GetElementINNERText(element, removeHTMLTags);
        }

        public string GetAttributeValue(WebHTMLElement Element, string AttributeName)
        {
            if (Element != null)
                return Element.GetAttributeValue(AttributeName);
            return null;
        }

        public async Task<string> GetAttributeValue(Guid WebDriverId, EnumWebHTMLElementSelector Selector, string Link, string AttributeName, bool? isElementRequired = true)
        {
            var element = await webDriverService.GetElement(WebDriverId, Selector, Link, isElementRequired).ConfigureAwait(false);
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
            var element = Element.GetElement(Selector, Link, isRequired.Value);
            return element;
        }
        
        public async Task<WebHTMLElement> GetElementInElement(Guid WebDriverId, EnumWebHTMLElementSelector ParentSelector, string ParentLink, EnumWebHTMLElementSelector Selector, string Link, bool? isRequired = true)
        {
            var parentElement = await webDriverService.GetElement(WebDriverId, ParentSelector, ParentLink).ConfigureAwait(false);
            if (parentElement != null)
            {
                return GetElementInElement(parentElement, Selector, Link, isRequired);
            }
            return null;
        }

        public List<WebHTMLElement> GetChildElements(WebHTMLElement Element, EnumWebHTMLElementSelector SelectorType, string Link, bool? isRequired = true)
        {
            var childs = Element.GetChildElements(SelectorType, Link, isRequired.Value);
            if ((childs != null) && (childs.Count > 0))
                return childs;
            return null;
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
            return null;
        }

    }
}
