namespace TestRailEngine
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using TestRailData;
    using TestRailModels.TestRail;

    public class TestRailManager
    {
        public static volatile TestRailData instance;
        private static readonly object syncRoot = new Object();

        private static Dictionary<ulong, List<Case>> casesRepository;
        public static Dictionary<ulong, List<Case>> CasesRepository
        {
            get
            {
                if (casesRepository == null) {
                    casesRepository = new Dictionary<ulong, List<Case>>();
                    var ezbobProject = Instance.Projects.FirstOrDefault(x => x.Name == "EZbob");
                    if (ezbobProject != null)
                    {
                        var suites = Instance.GetSuites(ezbobProject.ID);
                        foreach (var suiteItem in suites)
                        {
                            if (suiteItem.ID != null)
                            {
                                var cases = Instance.GetCases(ezbobProject.ID, suiteItem.ID.Value);
                                casesRepository.Add((ulong)suiteItem.ID, cases);
                            }
                        }
                    }
                }
                return casesRepository;
            }
            set { casesRepository = value; }
        }


        public static TestRailData Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        var url = ConfigurationManager.AppSettings["testRailUrl"];
                        var user = ConfigurationManager.AppSettings["testRailUser"];
                        var apikey = ConfigurationManager.AppSettings["testRailApiKey"];
                        instance = new TestRailData(url, user, apikey);
                    }
                }

                return instance;
            }
        }

    }
}