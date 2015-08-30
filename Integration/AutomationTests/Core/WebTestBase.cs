namespace UIAutomationTests.Core
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Resources;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using TestRailModels.Automation;
    using TestRailModels.TestRail;

    public class WebTestBase
    {
        protected IWebDriver Driver { get; set; }
        protected ResourceManager EnvironmentConfig { get; set; }
        protected ResourceManager BrandConfig { get; set; }

        private static bool? isDebugMode;
        protected static bool IsDebugMode
        {
            get
            {
                if (isDebugMode == null)
                {
                    isDebugMode = Convert.ToBoolean(ConfigurationManager.AppSettings["isDebugMode"]);
                }
                return (bool)isDebugMode;
            }
        }

        protected bool ExecuteTest<T>(Func<T> codeToExecute)
        {

            MethodBase method = new StackFrame(1).GetMethod();
            ulong caseID = ulong.Parse(((CategoryAttribute)(method.GetCustomAttributes(typeof(CategoryAttribute), true)[0])).Name);

            List<AutomationModels.Browser> browsers = GetBrowsers(caseID);
            List<AutomationModels.Brand> brands = GetBrands(caseID);
            List<AutomationModels.Environment> enviorments = GetEnviorments(caseID);

            if (IsNotValidConfigured(browsers, brands, enviorments))
            {
                if (!IsDebugMode) {
                    TestRailRepository.ReportTestRailBlockedNotConfiguredResults(caseID);
                }
                return false;
            }

            foreach (AutomationModels.Browser browser in browsers)
            {
                Driver = GetBrowserWebDriver.GetWebDriverForBrowser(browser);
                foreach (AutomationModels.Environment enviorment in enviorments)
                {
                    EnvironmentConfig = Resources.GetEnvironmentResourceManager(enviorment);
                    {
                        foreach (AutomationModels.Brand brand in brands)
                        {
                            BrandConfig = Resources.GetBrandResourceManager(brand);
                            try
                            {
                                if (!IsDebugMode) {
                                    if (TestRailRepository.BlockedSet.Contains(caseID)) {
                                        TestRailRepository.ReportTestRailResults(caseID, browser, brand, enviorment, ResultStatus.Blocked, "Automation is blocked depended test failed already");
                                        return false;
                                    }
                                }

                                codeToExecute.Invoke();
                                if (!IsDebugMode) {
                                    TestRailRepository.ReportTestRailResults(caseID, browser, brand, enviorment, ResultStatus.Passed, "Automation run passed");
                                }
                            }
                            catch (Exception ex){
                                if (!IsDebugMode) {
                                    UpdateBlockedList(caseID);
                                    TestRailRepository.ReportTestRailResults(caseID, browser, brand, enviorment, ResultStatus.Failed, ex.StackTrace);
                                }
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public bool IsNotValidConfigured(List<AutomationModels.Browser> browsers,
                                      List<AutomationModels.Brand> brands,
                                      List<AutomationModels.Environment> enviorments) {
            return ((browsers.Count == 1 && browsers.FirstOrDefault() == AutomationModels.Browser.None) ||
                   (brands.Count == 1 && brands.FirstOrDefault() == AutomationModels.Brand.None) ||
                   (enviorments.Count == 1 && enviorments.FirstOrDefault() == AutomationModels.Environment.None));
        }

        public void UpdateBlockedList(ulong caseID)
        {
            AtutomationCaseRun caseDependency = TestRailRepository.PlanRepository.FirstOrDefault(x => x.CaseBase.ID == caseID);
            if (caseDependency != null && caseDependency.IncludedIn != null)
                if (caseDependency.IncludedIn.Count > 0)
                {
                    TestRailRepository.BlockedSet.AddAll(caseDependency.IncludedIn);
                }
        }

        public List<AutomationModels.Browser> GetBrowsers(ulong caseID)
        {
            if(IsDebugMode){
                return new List<AutomationModels.Browser>() {
                    AutomationModels.Browser.Firefox
                };
            }
            return TestRailRepository.PlanRepository.Where(x => x.CaseBase.ID == caseID).Select(x => x.Browser).Distinct().ToList();
        }

        public List<AutomationModels.Brand> GetBrands(ulong caseID)
        {
            if(IsDebugMode){
                return new List<AutomationModels.Brand>() {
                    AutomationModels.Brand.Ezbob
                };
            }
            return TestRailRepository.PlanRepository.Where(x => x.CaseBase.ID == caseID).Select(x => x.Brand).Distinct().ToList();
        }

        public List<AutomationModels.Environment> GetEnviorments(ulong caseID)
        {
            if (IsDebugMode){
                return new List<AutomationModels.Environment>() {
                    AutomationModels.Environment.QA
                };
            }
            return TestRailRepository.PlanRepository.Where(x => x.CaseBase.ID == caseID).Select(x => x.Environment).Distinct().ToList();
        }
    }
}
