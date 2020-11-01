using BotClient.Models.HTMLElements.Enumerators;
using BotClient.Models.Settings;
using HtmlAgilityPack;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotClient.Models.HTMLElements
{
    public class WebHTMLElement
    {
        public WebHTMLElementModel HTMLElementModel { get; }
        public WebConnectionSettings WebSettings { get; }
        public bool isAvailable { get; } = false;

        IWebDriver webDriver;
        IWebElement element = null;
        Random rand = new Random();

        public WebHTMLElement(IWebDriver WebDriver, EnumWebHTMLElementSelector SelectorType, string Link, bool isRequired, WebConnectionSettings ConnectionSettings)
        {
            var ElementModel = new WebHTMLElementModel()
            {
                SelectorType = SelectorType,
                Link = Link,
                isRequired = isRequired
            };
            if (WebDriver != null)
            {
                webDriver = WebDriver;
                element = FindWebElement(ElementModel);
                if (element != null)
                {
                    HTMLElementModel = ElementModel;
                    WebSettings = ConnectionSettings;
                    isAvailable = true;
                }
            }
        }

        private WebHTMLElement(IWebDriver WebDriver, WebHTMLElementModel ElementModel, WebConnectionSettings ConnectionSettings, bool? isParentElementIsDriver = true)
        {
            if (WebDriver != null)
            {
                webDriver = WebDriver;
                element = FindWebElement(ElementModel, isParentElementIsDriver);
                if (element != null)
                {
                    HTMLElementModel = ElementModel;
                    WebSettings = ConnectionSettings;
                    isAvailable = true;
                }
            }
        }

        public EnumWebHTMLPageStatus WaitPageLoading()
        {
            for (int i = 0; i < WebSettings.HTMLPageWaitingTime; i++)
            {
                try
                {
                    var js = (IJavaScriptExecutor)webDriver;
                    object o = js.ExecuteScript("return document.readyState;");
                    var readyState = (string)o;
                    if (readyState == "complete" || readyState == "interactive")
                        return EnumWebHTMLPageStatus.Ready;
                }
                catch
                {
                    return EnumWebHTMLPageStatus.Error;
                }
                Thread.Sleep(1000);
            }
            return EnumWebHTMLPageStatus.Loading;
        }

        public bool Click(EnumClickType ClickType)
        {
            try
            {
                element.Click();
                if (ClickType == EnumClickType.URLClick)
                {
                    var loadingResult = WaitPageLoading();
                    if (loadingResult != EnumWebHTMLPageStatus.Ready)
                        return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool PrintText(string Text)
        {
            if (isCanPrintText())
            {
                string printedText = "";
                for (int i = 0; i < Text.Length; i++)
                {
                    string letter = Text[i].ToString();
                    element.SendKeys(letter);
                    printedText += letter;
                    Thread.Sleep(rand.Next(WebSettings.KeyWaitingTimeMin, WebSettings.KeyWaitingTimeMax));
                }
                if (printedText == Text)
                {
                    element.SendKeys(Keys.Return);
                    return true;
                }
                element.Clear();
            }
            return false;
        }

        public bool Clear()
        {
            try
            {
                element.Clear();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Scroll()
        {
            try
            {
                for (int i = 0; i < WebSettings.ScrollCount; i++)
                {
                    element.SendKeys(Keys.End);
                    Thread.Sleep(rand.Next(WebSettings.KeyWaitingTimeMin, WebSettings.KeyWaitingTimeMax));
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool WaitWebHTMLElement(bool isParentElementIsDriver, EnumWebHTMLElementSelector SelectorType, string Link, bool isRequired, WebConnectionSettings ConnectionSettings)
        {
            var ElementModel = new WebHTMLElementModel()
            {
                SelectorType = SelectorType,
                Link = Link,
                isRequired = isRequired
            };
            for (int i = 0; i < WebSettings.HTMLElementWaitingTime; i++)
            {
                var bufferElement = FindWebElement(ElementModel, isParentElementIsDriver);
                if (bufferElement != null)
                    return true;
                Thread.Sleep(1000);
            }
            return false;
        }

        public bool CheckAttribute(string AttributeName, string AttributeValue)
        {
            try
            {
                if (element.GetAttribute(AttributeName) == AttributeValue)
                    return true;
            }
            catch
            {
                return false;
            }
            return false;
        }

        public string GetINNERText(bool removeHTMLTags)
        {
            try
            {
                var text = element.Text;
                if (removeHTMLTags)
                {
                    var htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(text);
                    text = htmlDocument.DocumentNode.InnerText;
                }
                return text;
            }
            catch
            {
                return null;
            }
        }

        public string GetAttributeValue(string AttributeName)
        {
            try
            {
                var attributeValue = element.GetAttribute(AttributeName);
                return attributeValue;
            }
            catch
            {
                return null;
            }
        }

        public WebHTMLElement GetElement(EnumWebHTMLElementSelector SelectorType, string Link, bool isRequired)
        {
            var elementModel = new WebHTMLElementModel()
            {
                SelectorType = SelectorType,
                Link = Link,
                isRequired = isRequired
            };
            var webHTMLElement = new WebHTMLElement(webDriver, elementModel, WebSettings, false);
            if (webHTMLElement.isAvailable)
                return webHTMLElement;
            return null;
        }

        private IWebElement FindWebElement(WebHTMLElementModel ElementModel, bool? isParentElementIsDriver = true)
        {
            IWebElement bufferElement = null;
            try
            {
                switch (ElementModel.SelectorType)
                {
                    case EnumWebHTMLElementSelector.Id:
                        if(isParentElementIsDriver.Value)
                            bufferElement = webDriver.FindElement(By.Id(ElementModel.Link));
                        else
                            bufferElement = element.FindElement(By.Id(ElementModel.Link));
                        break;
                    case EnumWebHTMLElementSelector.Name:
                        if (isParentElementIsDriver.Value)
                            bufferElement = webDriver.FindElement(By.Name(ElementModel.Link));
                        else
                            bufferElement = element.FindElement(By.Name(ElementModel.Link));
                        break;
                    case EnumWebHTMLElementSelector.XPath:
                        if (isParentElementIsDriver.Value)
                            bufferElement = webDriver.FindElement(By.XPath(ElementModel.Link));
                        else
                            bufferElement = element.FindElement(By.XPath(ElementModel.Link));
                        break;
                    case EnumWebHTMLElementSelector.TagName:
                        if (isParentElementIsDriver.Value)
                            bufferElement = webDriver.FindElement(By.TagName(ElementModel.Link));
                        else
                            bufferElement = element.FindElement(By.TagName(ElementModel.Link));
                        break;
                    case EnumWebHTMLElementSelector.ClassName:
                        if (isParentElementIsDriver.Value)
                            bufferElement = webDriver.FindElement(By.ClassName(ElementModel.Link));
                        else
                            bufferElement = element.FindElement(By.ClassName(ElementModel.Link));
                        break;
                    case EnumWebHTMLElementSelector.CSSSelector:
                        if (isParentElementIsDriver.Value)
                            bufferElement = webDriver.FindElement(By.CssSelector(ElementModel.Link));
                        else
                            bufferElement = element.FindElement(By.CssSelector(ElementModel.Link));
                        break;
                    default:
                        bufferElement = null;
                        break;
                }
            }
            catch
            {
                bufferElement = null;
            }
            return bufferElement;
        }

        private bool isCanPrintText()
        {
            string testText = new Guid().ToString().Remove(1);
            try
            {
                element.SendKeys(testText);
                element.Clear();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
