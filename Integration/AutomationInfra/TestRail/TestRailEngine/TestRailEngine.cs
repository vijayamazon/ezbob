﻿namespace TestRailEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json.Linq;
    using TestRailModels.Automation;
    using TestRailModels.TestRail;

    public interface ITestRailEngine
    {
        bool AddResultForCase(ulong runID, ulong caseID, ResultStatus? status, string comment = null, string version = null,
            TimeSpan? elapsed = null, string defects = null, ulong? assignedToID = null, JObject customs = null);

        AtutomationCaseRun BildCaseAtutomation(Case caseItem,
            List<ulong> configs,
            ulong? suiteId,
            ulong? runId);

        List<AtutomationCaseRun> GetPlanAoutomationCases(ulong planId);

    }

    public class TestRailEngine : ITestRailEngine
    {

        public bool AddResultForCase(ulong runID, ulong caseID, ResultStatus? status, string comment = null, string version = null,
            TimeSpan? elapsed = null, string defects = null, ulong? assignedToID = null, JObject customs = null)
        {
            var result = TestRailManager.Instance.AddResultForCase(runID,
                                                              caseID,
                                                              status,
                                                              comment,
                                                              version,
                                                              elapsed,
                                                              defects,
                                                              assignedToID,
                                                              customs);
            return result.WasSuccessful;
        }

        public AtutomationCaseRun BildCaseAtutomation(Case caseItem,
                                                   List<ulong> configs,
                                                   ulong? suiteId,
                                                   ulong? runId)
        {
            var caseAtutomation = new AtutomationCaseRun();
            caseAtutomation.CaseBase = caseItem;
            if (suiteId != null)
                caseAtutomation.SuiteId = (ulong)suiteId;
            caseAtutomation.Dependencies = TestRailDependencies.GetDependencies(caseItem.ID);
            if (runId != null)
                caseAtutomation.RunId = (ulong)runId;

            if (configs != null)
            {
                if (configs.Count > 0)
                {
                    caseAtutomation.Brand = (AutomationModels.Brand)configs[0];
                }
                if (configs.Count > 1)
                {
                    caseAtutomation.Browser = (AutomationModels.Browser)configs[1];
                }
                if (configs.Count > 2)
                {
                    caseAtutomation.Environment = (AutomationModels.Environment)configs[2];
                }
            }

            return caseAtutomation;
        }

        public string PrintAll() {
            var sb = new StringBuilder();
            foreach (var key in TestRailManager.CasesRepository.Keys) {
                sb.Append("123456123456123456123456 + " + key + "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!123456123456");
                foreach (var caseItem in TestRailManager.CasesRepository[key]) {
                    sb.Append(string.Format(@"[Test][Category(""{0}"")]public void Dummy{0}(){{{{this.ExecuteTest<object>(() => {{Assert.IsTrue(false); return null;}});}}}}123456123456", caseItem.ID.ToString()));
                }
            }
            return sb.ToString();
        }


        public List<AtutomationCaseRun> GetPlanAoutomationCases(ulong planId)
        {
            List<AtutomationCaseRun> caseAtutomationList = new List<AtutomationCaseRun>();
            var ezbobProject = TestRailManager.Instance.Projects.FirstOrDefault(x => x.Name == "EZbob");
            var plan = TestRailManager.Instance.GetPlan(planId);
            if (ezbobProject != null)
            {
                if (plan != null)
                {
                    foreach (var entryItem in plan.Entries)
                    {
                        foreach (var runItem in entryItem.RunList)
                        {
                            var configs = runItem.ConfigIDs;
                            if (runItem.SuiteID != null)
                            {
                                var cases = TestRailManager.CasesRepository[(ulong)runItem.SuiteID];
                                foreach (var caseItem in cases)
                                {
                                    if (entryItem.SuiteID != null)
                                    {
                                        var caseAtutomation = BildCaseAtutomation(caseItem,
                                                                                    configs,
                                                                                    entryItem.SuiteID,
                                                                                    runItem.ID);
                                        caseAtutomationList.Add(caseAtutomation);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return caseAtutomationList;
        }
    }
}

