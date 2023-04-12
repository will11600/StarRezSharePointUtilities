using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using StarRezTest.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarRezTest.Bots
{
    public abstract class ChromeBot : IDisposable
    {
        private readonly string defaultDriverPath = Path.Combine(Environment.CurrentDirectory, @"\drivers\chromedriver.exe");

        public readonly IWebDriver Driver;
        public int Timeout = 3000;
        public virtual string LoginPageTitle => "Login";
        public virtual string BaseUrl => string.Empty;

        public string Url
        {
            get => Driver.Url;
            set
            {
                if (Driver.Url != value) { Driver.Navigate().GoToUrl(BaseUrl + value); }
            }
        }

        protected ChromeBot()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--log-level=3");
            Driver = new ChromeDriver(defaultDriverPath, options);
        }

        public void Dispose()
        {
            Driver.Dispose();
        }

        public bool TryFindElement(By by, out IWebElement? element)
        {
            int waitIntreval = 100;
            element = null;

            for (int i = 0; i < Timeout; i += waitIntreval)
            {
                try { element = Driver.FindElement(by); }
                catch (NoSuchElementException)
                {
                    Thread.Sleep(waitIntreval);
                    continue;
                }
                return true;
            }

            return false;
        }

        public bool TryClickElement(IWebElement element)
        {
            int waitIntreval = 100;

            for (int i = 0; i < Timeout; i += waitIntreval)
            {
                try { element.Click(); }
                catch (ElementNotInteractableException)
                {
                    Thread.Sleep(waitIntreval);
                    continue;
                }
                catch (NoSuchElementException)
                {
                    Thread.Sleep(waitIntreval);
                    continue;
                }
                return true;
            }

            return false;
        }

        protected bool EnsureIsURL(string url, bool authenticate = true)
        {
            Url = BaseUrl + url;
            for (int i = 0; i < 3; i++)
            {
                Thread.Sleep(400);
                if (Driver.Title == LoginPageTitle && authenticate) { Authenticate(); }
                if (Url != url) { Url = url; }
                else { return true; }
            }
            return false;
        }

        public bool TryCompleteForm(Form form, bool authenticate = true)
        {
            bool success = true;

            EnsureIsURL(form.Url, authenticate);

            foreach (var field in form.Fields)
            {
                if (TryFindElement(field.TargetElement, out var element))
                {
                    success = TrySetElement(element!, field.Content?.Invoke() ?? string.Empty);
                    Thread.Sleep(200);
                }
                else { return false; }
            }

            form.Submit();
            return success;
        }

        protected bool TrySetElement(IWebElement element, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) { return false; }

            for (int i = 0; i < Timeout; i += 100)
            {
                try
                {
                    switch (element!.TagName)
                    {
                        case "select":
                            var select = new SelectElement(element!);
                            select.SelectByText(value, true);
                            break;
                        default:
                            element!.SendKeys(value);
                            break;
                    }
                }
                catch (ElementNotInteractableException)
                {
                    Thread.Sleep(100);
                    continue;
                }
                return true;
            }
            return false;
        }

        public void CompleteForm(Form form, bool authenticate = true)
        {
            EnsureIsURL(form.Url, authenticate);

            foreach (var field in form.Fields)
            {
                if (TryFindElement(field.TargetElement, out var element))
                {
                    element!.SendKeys(field.Content());
                    Thread.Sleep(200);
                }
                else { throw new InvalidOperationException(); }
            }

            form.Submit();
        }

        protected bool RunJSOnPage(string js)
        {
            IJavaScriptExecutor? jsExe = Driver as IJavaScriptExecutor;
            try { jsExe?.ExecuteScript(js); }
            catch (JavaScriptException) { return false; }
            return true;
        }

        public abstract void Submit();

        public abstract void Deauthenticate();

        public abstract void Authenticate();
    }
}
