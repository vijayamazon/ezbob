using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIAutomationTests.Core
{
    using OpenQA.Selenium;

    public class TestBase
    {
        protected IWebDriver Driver { get; set; }

        protected void ExecuteFaultHandledOperation<T>(string category,  Func<T> codeToExecute) {
            var browsers = GetBrowsers(category);
            foreach (var br in browsers)
            {
                using (Driver = GetBrowserWebDriver.GetWebDriverForBrowser(br))
                {
                    codeToExecute.Invoke();
                }
            }
        }

        List<Browser> GetBrowsers(string category) {
            return new List<Browser>() {Browser.Firefox , Browser.Chrome};
        }
    }
}
