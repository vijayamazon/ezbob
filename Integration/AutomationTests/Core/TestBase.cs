namespace UIAutomationTests.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Resources;
    using NUnit.Framework;
    using OpenQA.Selenium;

    public class TestBase
    {
        protected IWebDriver Driver { get; set; }
        protected ResourceManager EnvironmentConfig { get; set; }
        protected ResourceManager BrandConfig { get; set; }

        protected void ExecuteTest<T>(Func<T> codeToExecute) {
            
            MethodBase method = new StackFrame(1).GetMethod();
            ulong caseID = ulong.Parse(((CategoryAttribute)(method.GetCustomAttributes(typeof(CategoryAttribute), true)[0])).Name);

            var browsers = GetBrowsers(caseID);
            var brands = GetBrands(caseID);
            var enviorments = GetEnviorments(caseID);

            foreach (var br in browsers) {
                Driver = GetBrowserWebDriver.GetWebDriverForBrowser(br);
                foreach (var env in enviorments)
                {
                        EnvironmentConfig = Resources.GetEnvironmentResourceManager(env);
                        {
                            foreach (var brand in brands)
                            {
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

        public List<AutomationEnums> GetBrowsers(ulong caseID)
        {
            return new List<AutomationEnums>() {AutomationEnums.Firefox , AutomationEnums.Chrome};
        }

        public List<Brand> GetBrands(ulong caseID)
        {
            return new List<Brand>() { Brand.Ezbob  };
        }

        public List<Environment> GetEnviorments(ulong caseID)
        {
            return new List<Environment>() { Environment.QA, Environment.Staging };
        }

    }
}
