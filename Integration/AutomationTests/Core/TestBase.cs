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

        protected bool ExecuteTest<T>(Func<T> codeToExecute) {
            
            MethodBase method = new StackFrame(1).GetMethod();
            ulong caseID = ulong.Parse(((CategoryAttribute)(method.GetCustomAttributes(typeof(CategoryAttribute), true)[0])).Name);

            List<AutomationModels.Browser> browsers = GetBrowsers(caseID);
            List<AutomationModels.Brand> brands = GetBrands(caseID);
            List<AutomationModels.Environment> enviorments = GetEnviorments(caseID);
            bool IsPassedAllConfigs = true;

            foreach (AutomationModels.Browser browser in browsers) {
                Driver = GetBrowserWebDriver.GetWebDriverForBrowser(browser);
                foreach (AutomationModels.Environment enviorment in enviorments)
                {
                        EnvironmentConfig = Resources.GetEnvironmentResourceManager(enviorment);
                        {
                            foreach (AutomationModels.Brand brand in brands)
                            {
                                BrandConfig = Resources.GetBrandResourceManager(brand);
                                try {
                                    if (TestRailRepository.BlockedSet.Contains(caseID)) {
                                        TestRailRepository.ReportTestRailResults(caseID, browser, brand, enviorment, ResultStatus.Blocked, "Automation is blocked depended test failed already");
                                    } 
                                    else {
                                        codeToExecute.Invoke();
                                        TestRailRepository.ReportTestRailResults(caseID, browser, brand, enviorment, ResultStatus.Passed, "Automation run passed");
                                    }
                                } catch (Exception ex) {
                                    UpdateBlockedList(caseID);
                                    TestRailRepository.ReportTestRailResults(caseID, browser, brand, enviorment, ResultStatus.Failed, ex.StackTrace);
                                    IsPassedAllConfigs = false;
                                }
                            }
                        }
                        
                    }
                
            }
            return IsPassedAllConfigs;
        }

        public void UpdateBlockedList(ulong caseID) {
            AtutomationCaseRun caseDependency = TestRailRepository.PlanRepository.FirstOrDefault(x => x.CaseBase.ID == caseID);
                                    if (caseDependency != null && caseDependency.IncludedIn != null)
                                        if (caseDependency.IncludedIn.Count > 0) {
                                            TestRailRepository.BlockedSet.AddAll(caseDependency.IncludedIn);
                                        }
        }

        public List<AutomationModels.Browser> GetBrowsers(ulong caseID) {
            return TestRailRepository.PlanRepository.Where(x => x.CaseBase.ID == caseID).Select(x => x.Browser).Distinct().ToList();
        }

        public List<AutomationModels.Brand> GetBrands(ulong caseID)
        {
            return TestRailRepository.PlanRepository.Where(x => x.CaseBase.ID == caseID).Select(x => x.Brand).Distinct().ToList();
        }

        public List<AutomationModels.Environment> GetEnviorments(ulong caseID)
        {
            return TestRailRepository.PlanRepository.Where(x => x.CaseBase.ID == caseID).Select(x => x.Environment).Distinct().ToList();
        }

    }
}
