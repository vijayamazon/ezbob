namespace UIAutomationTests.Core {
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Resources;
    using log4net;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using TestRailModels.Automation;
    using TestRailModels.TestRail;

    [TestFixture]
    public class WebTestBase {
        protected IWebDriver Driver { get; set; }
        protected ResourceManager EnvironmentConfig { get; set; }
        protected ResourceManager BrandConfig { get; set; }
        protected ActionBot actionBot { get; set; }

        private static readonly ILog log = LogManager.GetLogger(typeof(WebTestBase));

        private static bool? isDebugMode;


        protected static bool IsDebugMode {
            get {
                if (isDebugMode == null) {
                    isDebugMode = Convert.ToBoolean(ConfigurationManager.AppSettings["isDebugMode"]);
                }
                return isDebugMode != null && (bool)isDebugMode;
            }
        }

        public static string IsRunLocal {
            get {
                if (Convert.ToBoolean(ConfigurationManager.AppSettings["isRunLocal"]) == false)
                    return "";
                return ":44300";
            }
        }

        [TestFixtureSetUp]
        public void RunBeforeTests() {
            FileInfo fileInfo = new FileInfo(@"C:\Exception\Log4NetConfig.config");
            log4net.Config.XmlConfigurator.Configure(fileInfo);
        }

        [TestFixtureTearDown]
        public void RunAfterTests() {
            LogManager.Shutdown();
            if (IsDebugMode)
                return;
            foreach (var driver in TestRailRepository.PlanRepository.Select(x => x.Browser).Distinct().ToList())
                GetBrowserWebDriver.GetWebDriverForBrowser(driver).Quit();
        }

        protected bool ExecuteTest(Action<string> codeToExecute) {

            MethodBase method = new StackFrame(1).GetMethod();
            ulong caseID = ulong.Parse(((CategoryAttribute)(method.GetCustomAttributes(typeof(CategoryAttribute), true)[0])).Name);

            List<AutomationModels.Browser> browsers = GetBrowsers(caseID);
            List<AutomationModels.Brand> brands = GetBrands(caseID);
            List<AutomationModels.Environment> enviorments = GetEnviorments(caseID);


            if (IsNotValidConfigured(browsers, brands, enviorments)) {
                if (!IsDebugMode) {
                    TestRailRepository.ReportTestRailBlockedNotConfiguredResults(caseID);
                }
                return false;
            }
            bool res = true;
            foreach (AutomationModels.Browser browser in browsers) {
                Driver = GetBrowserWebDriver.GetWebDriverForBrowser(browser);
                foreach (AutomationModels.Environment enviorment in enviorments) {
                    EnvironmentConfig = Resources.GetEnvironmentResourceManager(enviorment);
                    foreach (AutomationModels.Brand brand in brands) {
                        BrandConfig = Resources.GetBrandResourceManager(brand);
                        try {
                            if (!IsDebugMode && TestRailRepository.BlockedSet.Contains(caseID)) {
                                TestRailRepository.ReportTestRailResults(caseID, browser, brand, enviorment, ResultStatus.Blocked, "Automation is blocked depended test failed already");
                                return false;
                            }

                            actionBot = new ActionBot(Driver);

                            for (int i = Driver.WindowHandles.Count; i < 1; i--) {
                                Driver.SwitchTo().Window(Driver.WindowHandles[1]);
                                Driver.Close();
                            }

                            Driver.Manage().Cookies.DeleteAllCookies();

                            codeToExecute.Invoke(caseID.ToString() + (isDebugMode == false ? " - " + TestRailRepository.TestRailCaseName(caseID) : ""));

                            if (!IsDebugMode) {
                                TestRailRepository.ReportTestRailResults(caseID, browser, brand, enviorment, ResultStatus.Passed, "Automation run passed");
                            }
                        } catch (Exception ex) {
                            log.Error(String.Format("------------------Exception for CaseId{0}------------------\n{1}\n------------------{2}------------------\n".Replace("\n", Environment.NewLine), caseID.ToString(), ex.ToString(), DateTime.UtcNow.ToString("u")));
                            try {
                                string scrshtFileName = DateTime.Now.Ticks + " - " + DateTime.UtcNow.ToString("yyyy-MM-ddTHH.mm.ssZ") + " - CaseId_" + caseID.ToString() + " - Enviorment_" + EnvironmentConfig.BaseName.Split('.')[3] + " - Brand_" + BrandConfig.BaseName.Split('.')[3] + " - Browser_" + Driver.GetType().ToString().Split('.')[2] + ".png";
                                (Driver as ITakesScreenshot).GetScreenshot().SaveAsFile("C:\\Exception\\" + scrshtFileName, ImageFormat.Png);
                                log.Error("Screenshot of last screen was taken and saved in C:\\Exception\\" + scrshtFileName);
                            } catch (Exception e) {
                                log.Error("ERROR: failed to save screen shot.");
                            }
                            if (!IsDebugMode) {
                                UpdateBlockedList(caseID);
                                TestRailRepository.ReportTestRailResults(caseID, browser, brand, enviorment, ResultStatus.Failed, ex.StackTrace);
                            }
                            res = false;
                        }
                    }
                }
            }
            return res;
        }

        public bool IsNotValidConfigured(List<AutomationModels.Browser> browsers,
                                      List<AutomationModels.Brand> brands,
                                      List<AutomationModels.Environment> enviorments) {
            return ((browsers.Count == 1 && browsers.FirstOrDefault() == AutomationModels.Browser.None) ||
                   (brands.Count == 1 && brands.FirstOrDefault() == AutomationModels.Brand.None) ||
                   (enviorments.Count == 1 && enviorments.FirstOrDefault() == AutomationModels.Environment.None));
        }

        public void UpdateBlockedList(ulong caseID) {
            AtutomationCaseRun caseDependency = TestRailRepository.PlanRepository.FirstOrDefault(x => x.CaseBase.ID == caseID);
            if (caseDependency != null && caseDependency.IncludedIn != null)
                if (caseDependency.IncludedIn.Count > 0) {
                    TestRailRepository.BlockedSet.AddAll(caseDependency.IncludedIn);
                }
        }

        public List<AutomationModels.Browser> GetBrowsers(ulong caseID) {
            if (IsDebugMode) {
                return new List<AutomationModels.Browser>() {
                    AutomationModels.Browser.Chrome
                };
            }
            return TestRailRepository.PlanRepository.Where(x => x.CaseBase.ID == caseID).Select(x => x.Browser).Distinct().ToList();
        }

        public List<AutomationModels.Brand> GetBrands(ulong caseID) {
            if (IsDebugMode) {
                return new List<AutomationModels.Brand>() {
                    AutomationModels.Brand.Ezbob
                };
            }
            return TestRailRepository.PlanRepository.Where(x => x.CaseBase.ID == caseID).Select(x => x.Brand).Distinct().ToList();
        }

        public List<AutomationModels.Environment> GetEnviorments(ulong caseID) {
            if (IsDebugMode) {
                return new List<AutomationModels.Environment>() {
                    AutomationModels.Environment.QA
                };
            }
            return TestRailRepository.PlanRepository.Where(x => x.CaseBase.ID == caseID).Select(x => x.Environment).Distinct().ToList();
        }
    }
}
