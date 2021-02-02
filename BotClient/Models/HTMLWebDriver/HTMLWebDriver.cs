using BotClient.Models.Enumerators;
using BotClient.Models.Settings;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;

namespace BotClient.Models.HTMLWebDriver
{
    public class HTMLWebDriver
    {
        public Guid Id { get { return _id; } }
        public EnumWebDriverStatus Status { get; set; } = EnumWebDriverStatus.Start;
        [JsonIgnore]
        public IWebDriver WebDriver { get; }
        public Exception ExceptionMessage { get { return _exception; } }
        [JsonIgnore]
        public WebConnectionSettings WebSettings { get; }
        public EnumSocialPlatform WebDriverPlatform { get; }
        public bool hasBots { get; set; }

        private Exception _exception = new Exception();
        private Guid _id = Guid.NewGuid();

        public HTMLWebDriver(EnumSocialPlatform SocialPlatform, WebConnectionSettings ConnectionSettings)
        {
            try
            {
                var options = new ChromeOptions();
                for (int i = 0; i < ConnectionSettings.Options.Count; i++)
                    options.AddArgument(ConnectionSettings.Options[i]);
                WebDriver = new ChromeDriver(ChromeDriverService.CreateDefaultService(), options, TimeSpan.FromMinutes(10));
                WebDriver.Manage().Timeouts().PageLoad.Add(TimeSpan.FromMinutes(10));
                switch (SocialPlatform)
                {
                    case EnumSocialPlatform.Vk:
                        WebDriver.Navigate().GoToUrl("https://vk.com/");
                        break;
                    default:
                        break;
                }
                Status = EnumWebDriverStatus.Start;
                WebSettings = ConnectionSettings;
                WebDriverPlatform = SocialPlatform;
            }
            catch (Exception ex)
            {
                _exception = ex;
                Status = EnumWebDriverStatus.Error;
            }
        }

        public void SetNewId(Guid Id)
        {
            _id = Id;
        }
    }
}
