namespace UIAutomationTests.Core {
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;
    using Iesi.Collections.Generic;
    using TestRailEngine;
    using TestRailModels.Automation;
    using TestRailModels.TestRail;

    class TestRailRepository {
        private static List<AtutomationCaseRun> _PlanRepository;
        private static Set<ulong> _BlockedSet;

        public static Set<ulong> BlockedSet {
            get { return _BlockedSet ?? (_BlockedSet = new HashedSet<ulong>()); }
        }

        public static List<AtutomationCaseRun> PlanRepository {
            get {
                if (_PlanRepository == null) {
                    DirectoryInfo folder = new DirectoryInfo(ConfigurationManager.AppSettings["teamcityJobsInFolder"]).GetDirectories().FirstOrDefault();
                    if (folder != null) {
                        using (Stream stream = File.Open(string.Format(@"{0}\data.bin", folder.FullName), FileMode.Open)) {
                            BinaryFormatter bin = new BinaryFormatter();
                            _PlanRepository = (List<AtutomationCaseRun>)bin.Deserialize(stream);
                        }
                    }
                }
                return _PlanRepository;
            }
        }

        internal static void ReportTestRailResults(ulong caseID,
                                                    AutomationModels.Browser browser,
                                                    AutomationModels.Brand brand,
                                                    AutomationModels.Environment enviorment,
                                                    TestRailModels.TestRail.ResultStatus resultStatus,
                                                    string messege) {
            AtutomationCaseRun automationCase = PlanRepository
                .Where(x => x.CaseBase.ID == caseID)
                .Where(x => x.Browser == browser)
                .Where(x => x.Brand == brand)
                .FirstOrDefault(x => x.Environment == enviorment);
            if (automationCase != null)
                TestRailManager.Instance.AddResultForCase(automationCase.RunId, caseID, resultStatus, messege);
        }

        internal static void ReportTestRailBlockedNotConfiguredResults(ulong caseID) {
            foreach (var singleCase in PlanRepository.Where(x => x.CaseBase.ID == caseID).ToList())
                TestRailManager.Instance.AddResultForCase(singleCase.RunId, caseID, ResultStatus.Blocked, "Could not find valid configiration for this run, make sure run has {browser, brand, enviorment}");
        }

        internal static string TestRailCaseName(ulong caseID) {
            AtutomationCaseRun atutomationCaseRun = PlanRepository.FirstOrDefault(x => x.CaseBase.ID == caseID);

            return atutomationCaseRun == null ? null : atutomationCaseRun.CaseBase.Title;
        }
    }
}

