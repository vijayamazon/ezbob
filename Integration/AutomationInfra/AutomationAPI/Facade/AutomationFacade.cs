namespace AutomationAPI.Facade
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;
    using TeamCityEngine;
    using TestRailEngine;
    using TestRailModels.Automation;

    public class AutomationFacade 
    {

        private TestRailEngine _testRailEngine;
        private TeamCityEngine _teamCityEngine;

        public TestRailEngine TestRailEngine
        {
            get { return this._testRailEngine ?? (this._testRailEngine = new TestRailEngine()); }
        }

        public TeamCityEngine TeamCityEngine
        {
            get { return this._teamCityEngine ?? (this._teamCityEngine = new TeamCityEngine()); }
        }

        public void SerializeData(DirectoryInfo folder, List<AtutomationCaseRun> data)
        {
            using (Stream stream = File.Open(string.Format(@"{0}\data.bin", folder.FullName), FileMode.Create))
            {
                BinaryFormatter bin = new BinaryFormatter();
                bin.Serialize(stream, data);
            }
        }

        public DirectoryInfo CreateNewJobFolder(ulong runId)
        {
            var jobsInFolder = ConfigurationManager.AppSettings["teamcityJobsInFolder"];
            var folder = Directory.CreateDirectory(string.Format(@"{0}\{1}_{2}", jobsInFolder, runId.ToString(), DateTime.Now.ToFileTime()));
            return folder;
        }

        public void CleanJobsFolder()
        {
            var jobsInFolder = ConfigurationManager.AppSettings["teamcityJobsInFolder"];
            var jobsOutFolder = ConfigurationManager.AppSettings["teamcityJobsOutFolder"];
            DirectoryInfo folderDest = new DirectoryInfo(jobsOutFolder);
            List<DirectoryInfo> sourceFolders = new DirectoryInfo(jobsInFolder).GetDirectories().ToList();

            foreach (DirectoryInfo folder in sourceFolders)
            {
                Directory.Move(folder.FullName, string.Format(@"{0}\{1}", folderDest.FullName, folder.Name));
            }
        }

        public void CreateTeamCityBuild(List<ulong?> casesDistinct)
        {
            var buildRegressionId = ConfigurationManager.AppSettings["buildRegressionId"];
            var teamcityIncludeParameterName = ConfigurationManager.AppSettings["teamcityIncludeParameterName"];
            var includingCases = string.Join(",", casesDistinct);

            TeamCityEngine.AddJob(buildRegressionId, teamcityIncludeParameterName, includingCases);
        }

        public List<AtutomationCaseRun> GetPlanAoutomationCases(ulong runId)
        {
            return TestRailEngine.GetPlanAoutomationCases(runId);
        }

        public List<ulong?> GetRunCasesListIdsDistinct( List<AtutomationCaseRun> data ) {
            return data.OrderBy(x => x.IncludedIn.Count > 0).Select(x => x.CaseBase.ID).Distinct().ToList();
        }
    }
}
