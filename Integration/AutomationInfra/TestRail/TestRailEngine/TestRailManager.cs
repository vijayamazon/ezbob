namespace TestRailEngine {
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using TestRailData;
    using TestRailModels.TestRail;

    public class TestRailManager {
        public static volatile TestRailData instance;
        private static readonly object syncRoot = new Object();
        private static Dictionary<ulong, Suite> suiteRepository;


        public static TestRailData Instance {
            get {
                if (instance == null) {
                    lock (syncRoot) {
                        var url = ConfigurationManager.AppSettings["testRailUrl"];
                        var user = ConfigurationManager.AppSettings["testRailUser"];
                        var apikey = ConfigurationManager.AppSettings["testRailApiKey"];
                        instance = new TestRailData(url, user, apikey);
                        CasesRepository = new Dictionary<ulong, List<Case>>();
                        var ezbobProject = instance.Projects.FirstOrDefault(x => x.Name == "EZbob");
                        if (ezbobProject != null) {
                            var suites = instance.GetSuites(ezbobProject.ID);
                            foreach (var suiteItem in suites) {
                                if (suiteItem.ID != null) {
                                    var cases = instance.GetCases(ezbobProject.ID, suiteItem.ID.Value);
                                    CasesRepository.Add((ulong)suiteItem.ID, cases);
                                }
                            }
                        }

                    }
                }
                return instance;
            }
        }

        public static Dictionary<ulong, Suite> SuiteRepository {
            get {
                if (suiteRepository == null) {
                    suiteRepository = new Dictionary<ulong, Suite>();
                    var ezbobProject = Instance.Projects.FirstOrDefault(x => x.Name == "EZbob");
                    if (ezbobProject != null) {
                        var suites = Instance.GetSuites(ezbobProject.ID);
                        foreach (var suiteItem in suites) {
                            if (suiteItem.ID != null)
                                suiteRepository.Add((ulong)suiteItem.ID, suiteItem);
                        }
                    }
                }
                return suiteRepository;
            }
            set { suiteRepository = value; }
        }

        public static Dictionary<ulong, List<Case>> CasesRepository { get; set; }
    }
}