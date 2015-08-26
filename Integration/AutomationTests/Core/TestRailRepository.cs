namespace UIAutomationTests.Core
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;
    using Iesi.Collections.Generic;
    using TestRailEngine;
    using TestRailModels.Automation;

    class TestRailRepository {
        private static List<AtutomationCaseRun> _PlanRepository;
        private static Set<ulong> _BlockedSet;

        public static Set<ulong> BlockedSet
        {
            get { return _BlockedSet ?? (_BlockedSet = new HashedSet<ulong>());}
        }

        public static List<AtutomationCaseRun> PlanRepository
        {
            get
            {
                if (_PlanRepository == null) {
                    DirectoryInfo  folder =new DirectoryInfo(@"E:\Jobs\JobsIn").GetDirectories().FirstOrDefault();
                    if (folder != null) {
                        using (Stream stream = File.Open(string.Format(@"{0}\data.bin",folder.FullName), FileMode.Open))
                        {
                            BinaryFormatter bin = new BinaryFormatter();
                            _PlanRepository = (List<AtutomationCaseRun>)bin.Deserialize(stream);
                        }
                    }
                }
                return _PlanRepository;
            }
        }

        internal static void ReportTestRailResults(ulong caseID,
                                                    AutomationModels.Browser  browser, 
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
                    TestRailManager.Instance.AddResultForCase(automationCase.RunId,caseID, resultStatus, messege);
        }
    }
}
