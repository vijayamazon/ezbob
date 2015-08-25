namespace AutomationAPI.Controllers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Web.Http;
    using TeamCity;
    using TeamCity.Locators;
    using TestRailEngine;

    public class AutomationController : ApiController
    {

        private TestRailEngine _TestRailEngineManager;

        public TestRailEngine TestRailEngineManager
        {
            get { return this._TestRailEngineManager ?? (this._TestRailEngineManager = new TestRailEngine()); }
        }

        [AcceptVerbs("GET", "POST")]
        public IHttpActionResult RunAutomationTests(ulong id)
        {
            var data = TestRailEngineManager.GetPlanAoutomationCases(id);

            var casesDistinct = data.OrderBy(x => x.Dependencies.Count > 0).Select(x => x.CaseBase.ID).Distinct().ToList();

            var casesString = @"echo ""##teamcity[setParameter name='env.INCLUDED_TESTS' value='" + string.Join(",", casesDistinct) + @"']""" ;


            var jobsFolder = @"E:\Jobs\JobsIn";
            var folder = System.IO.Directory.CreateDirectory(string.Format(@"{0}\{1}_{2}", jobsFolder, id.ToString(), DateTime.Now.ToFileTime()));

            System.IO.File.WriteAllText(string.Format(@"{0}\script.cmd",folder.FullName) , casesString);

            using (Stream stream = File.Open(string.Format(@"{0}\data.bin", folder.FullName), FileMode.Create))
            {
                BinaryFormatter bin = new BinaryFormatter();
                bin.Serialize(stream, data);
            }

            var tcc = new TeamCityClient("localhost:49152", false);
            tcc.Connect("admin", "123456");
            
            tcc.BuildConfigs.SetConfigurationParameter(BuildTypeLocator.WithId("EzbobSite1_Regression"), "env.INCLUDED_TESTS", casesString);
            tcc.Builds.Add2QueueBuildByBuildConfigId("EzbobSite1_Regression");

            return Ok();
        }

        //[AcceptVerbs("GET", "POST")]
        //public IHttpActionResult CreateTestRailPlan(int labelId)
        //{
        //    var plan = TestRailEngineManager.CreateTestRailPlan(labelId);
        //    return Ok();
        //}

    }
}
