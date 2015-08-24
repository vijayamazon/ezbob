namespace UIAutomationTests.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Resources;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using TestRailModels.Automation;
    using TestRailModels.TestRail;

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

            foreach (var browser in browsers) {
                Driver = GetBrowserWebDriver.GetWebDriverForBrowser(browser);
                foreach (var enviorment in enviorments)
                {
                        EnvironmentConfig = Resources.GetEnvironmentResourceManager(enviorment);
                        {
                            foreach (var brand in brands)
                            {
                                BrandConfig = Resources.GetBrandResourceManager(brand);
                                try {
                                    codeToExecute.Invoke();
                                    TestRailRepository.ReportTestRailResults(caseID, browser, brand, enviorment, ResultStatus.Passed, "Automation run passed");
                                } catch (Exception ex) {
                                    TestRailRepository.ReportTestRailResults(caseID, browser, brand, enviorment, ResultStatus.Failed, ex.StackTrace);
                                } 
                            }
                        }
                        
                    }
                
            }
        }

        public List<AutomationModels.Browser> GetBrowsers(ulong caseID) {
            return TestRailRepository.PlanRepository.Where(x => x.CaseBase.ID == caseID).Select(x => x.Browser).ToList();
        }

        public List<AutomationModels.Brand> GetBrands(ulong caseID)
        {
            return TestRailRepository.PlanRepository.Where(x => x.CaseBase.ID == caseID).Select(x => x.Brand).ToList();
        }

        public List<AutomationModels.Environment> GetEnviorments(ulong caseID)
        {
            return TestRailRepository.PlanRepository.Where(x => x.CaseBase.ID == caseID).Select(x => x.Environment).ToList();
        }

    }
}
