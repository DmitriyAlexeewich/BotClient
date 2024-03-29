﻿using BotClient.Models.HTMLElements.Enumerators;
using BotDataModels.Settings;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
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

        private WebHTMLElement(IWebDriver WebDriver, EnumWebHTMLElementSelector SelectorType, string Link, bool isRequired, IWebElement WebDriverWebElement, WebConnectionSettings ConnectionSettings)
        {
            if (WebDriver != null)
            {
                webDriver = WebDriver;
                element = WebDriverWebElement;
                if (element != null)
                {
                    var ElementModel = new WebHTMLElementModel()
                    {
                        SelectorType = SelectorType,
                        Link = Link,
                        isRequired = isRequired
                    };
                    HTMLElementModel = ElementModel;
                    WebSettings = ConnectionSettings;
                    isAvailable = true;
                }
            }
        }

        public EnumWebHTMLPageStatus WaitPageLoading(string OldURL)
        {
            for (int i = 0; i < WebSettings.HTMLPageWaitingTime; i++)
            {
                try
                {
                    var js = (IJavaScriptExecutor)webDriver;
                    object o = js.ExecuteScript("return document.readyState;");
                    var readyState = (string)o;
                    if ((readyState == "complete" || readyState == "interactive") && (OldURL != webDriver.Url))
                        return EnumWebHTMLPageStatus.Ready;
                }
                catch(Exception ex)
                {
                    return EnumWebHTMLPageStatus.Error;
                }
                WaitTime(1000);
            }
            return EnumWebHTMLPageStatus.Loading;
        }

        public bool Click(EnumClickType ClickType)
        {
            try
            {
                var oldURL = webDriver.Url;
                try
                {
                    element.Click();
                }
                catch
                {
                    try
                    {
                        Actions actions = new Actions(webDriver);
                        actions.MoveToElement(element).Click().Perform();
                    }
                    catch
                    {
                        IJavaScriptExecutor executor = (IJavaScriptExecutor)webDriver;
                        executor.ExecuteScript("arguments[0].click();", element);

                    }
                }
                if (ClickType == EnumClickType.URLClick)
                {
                    var loadingResult = WaitPageLoading(oldURL);
                    if (loadingResult != EnumWebHTMLPageStatus.Ready)
                        return false;
                }
            }
            catch (Exception ex)
            {
            }
            return true;
        }

        public bool SendKey(string KeyName)
        {
            try
            {
                element.SendKeys(KeyName);
                return true;
            }
            catch
            {
            }
            return false;
        }

        public bool PrintText(string Text)
        {
            try
            {
                var sendedText = "";
                if (isCanPrintText())
                {
                    for (int i = 0; i < Text.Length; i++)
                    {
                        string letter = Text[i].ToString();
                        if (letter.IndexOf("\n") != -1)
                            element.SendKeys(Keys.Control + Keys.Enter);
                        else
                            element.SendKeys(letter);
                    }
                    sendedText = GetINNERText(true);
                    sendedText += GetAttributeValue("value");
                    if (sendedText.IndexOf(Text) != -1)
                        return true;
                    element.Clear();
                }
                IJavaScriptExecutor executor = (IJavaScriptExecutor)webDriver;
                executor.ExecuteScript($"arguments[0].value='{Text}';" +
                                        $"arguments[0].innerHTML='{Text}';", element);
                sendedText = GetINNERText(true);
                if (sendedText == Text)
                    return true;
            }
            catch (Exception ex)
            { }
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
                    WaitTime(1000);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ScrollTo()
        {
            try
            {
                if (element != null)
                {
                    IJavaScriptExecutor executor = (IJavaScriptExecutor)webDriver;
                    executor.ExecuteScript("arguments[0].scrollIntoView();", element);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool ScrollElementJS()
        {
            try
            {
                if (element != null)
                {
                    IJavaScriptExecutor executor = (IJavaScriptExecutor)webDriver;
                    executor.ExecuteScript("arguments[0].scroll(0,1000000000);", element);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool WaitWebHTMLElement(bool isParentElementIsDriver, EnumWebHTMLElementSelector SelectorType, string Link, bool isRequired, WebConnectionSettings ConnectionSettings)
        {
            try
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
                    WaitTime(1000);
                }
            }
            catch (Exception ex)
            { }
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
                if (text == null)
                    text = "";
                return text;
            }
            catch
            {
                return "";
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
            try
            {
                var elementModel = new WebHTMLElementModel()
                {
                    SelectorType = SelectorType,
                    Link = Link,
                    isRequired = isRequired
                };
                var childElement = FindWebElement(elementModel, false);
                if (childElement != null)
                {
                    var webHTMLElement = new WebHTMLElement(webDriver, SelectorType, Link, isRequired, childElement, WebSettings);
                    if (webHTMLElement.isAvailable)
                        return webHTMLElement;
                }
            }
            catch (Exception ex)
            { }
            return null;
        }

        public List<WebHTMLElement> GetChildElements(EnumWebHTMLElementSelector SelectorType, string Link, bool isRequired)
        {
            var findElementsResult = FindWebElements(SelectorType, Link, false);
            var result = new List<WebHTMLElement>();
            if ((findElementsResult != null) && (findElementsResult.Count > 0))
            {
                for (int i = 0; i < findElementsResult.Count; i++)
                {
                    var bufferElement = new WebHTMLElement(webDriver, SelectorType, Link, isRequired, findElementsResult[i], WebSettings);
                    if (bufferElement.isAvailable)
                        result.Add(bufferElement);
                }
            }
            return result;
        }

        private IWebElement FindWebElement(WebHTMLElementModel ElementModel, bool? isParentElementIsDriver = true)
        {
            IWebElement bufferElement;
            try
            {
                switch (ElementModel.SelectorType)
                {
                    case EnumWebHTMLElementSelector.Id:
                        if (isParentElementIsDriver.Value)
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

        private List<IWebElement> FindWebElements(EnumWebHTMLElementSelector SelectorType, string Link, bool? isParentElementIsDriver = true)
        {
            List<IWebElement> bufferElements = new List<IWebElement>();
            try
            {
                switch (SelectorType)
                {
                    case EnumWebHTMLElementSelector.Name:
                        if (isParentElementIsDriver.Value)
                            bufferElements = webDriver.FindElements(By.Name(Link)).ToList();
                        else
                            bufferElements = element.FindElements(By.Name(Link)).ToList();
                        break;
                    case EnumWebHTMLElementSelector.XPath:
                        if (isParentElementIsDriver.Value)
                            bufferElements = webDriver.FindElements(By.XPath(Link)).ToList();
                        else
                            bufferElements = element.FindElements(By.XPath(Link)).ToList();
                        break;
                    case EnumWebHTMLElementSelector.TagName:
                        if (isParentElementIsDriver.Value)
                            bufferElements = webDriver.FindElements(By.TagName(Link)).ToList();
                        else
                            bufferElements = element.FindElements(By.TagName(Link)).ToList();
                        break;
                    case EnumWebHTMLElementSelector.ClassName:
                        if (isParentElementIsDriver.Value)
                            bufferElements = webDriver.FindElements(By.ClassName(Link)).ToList();
                        else
                            bufferElements = element.FindElements(By.ClassName(Link)).ToList();
                        break;
                    case EnumWebHTMLElementSelector.CSSSelector:
                        if (isParentElementIsDriver.Value)
                            bufferElements = webDriver.FindElements(By.CssSelector(Link)).ToList();
                        else
                            bufferElements = element.FindElements(By.CssSelector(Link)).ToList();
                        break;
                    default:
                        bufferElements = null;
                        break;
                }
            }
            catch
            {
                bufferElements = null;
            }
            return bufferElements;
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

        private bool WaitTime(int Milliseconds)
        {
            var waitingTime = DateTime.Now.AddMilliseconds(Milliseconds / 10);
            while (DateTime.Now < waitingTime) { }
            return true;
        }
    }
}
