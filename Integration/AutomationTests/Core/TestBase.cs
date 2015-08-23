namespace UIAutomationTests.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Resources;
    using OpenQA.Selenium;

    public class TestBase
    {
        protected IWebDriver Driver { get; set; }
        protected ResourceManager EnvironmentConfig { get; set; }
        protected ResourceManager BrandConfig { get; set; }


        protected void ExecuteFaultHandledOperation<T>(ulong caseID, Func<T> codeToExecute)
        {
            var browsers = GetBrowsers(caseID);
            var brands = GetBrands(caseID);
            var enviorments = GetEnviorments(caseID);

            foreach (var br in browsers)
            {
                using (Driver = GetBrowserWebDriver.GetWebDriverForBrowser(br))
                {
                    foreach (var env in GetEnviorments(caseID)) {
                        EnvironmentConfig = Resources.GetEnvironmentResourceManager(env);
                        {
                            foreach (var brand in GetBrands(caseID)) {
                                BrandConfig = Resources.GetBrandResourceManager(brand);
                                try {
                                    codeToExecute.Invoke();
                                    //Report Sucsess
                                } catch (Exception) {
                                    //Report fail
                                } 
                            }
                        }
                        
                    }

                }
            }
        }

        public List<AutomationEnums> GetBrowsers(ulong caseID)
        {
            return new List<AutomationEnums>() {AutomationEnums.Firefox , AutomationEnums.Chrome};
        }

        public List<Brand> GetBrands(ulong caseID)
        {
            return new List<Brand>() { Brand.Ezbob , Brand.Everline  };
        }

        public List<Environment> GetEnviorments(ulong caseID)
        {
            return new List<Environment>() { Environment.QA, Environment.Staging };
        }

    }
}
