namespace AutomationAPI.Controllers
{
    using System.IO;
    using System.Web.Http;
    using AutomationAPI.Facade;

    public class AutomationController : ApiController
    {
        private AutomationFacade _automationFacade;

        public AutomationFacade AutomationFacade
        {
            get { return this._automationFacade ?? (this._automationFacade = new AutomationFacade()); }
        }

        [AcceptVerbs("GET", "POST")]
        public IHttpActionResult RunAutomationTests(ulong id)
        {
            AutomationFacade.CleanJobsFolder();
            var data = AutomationFacade.GetPlanAoutomationCases(id);
            var casesDistinct = AutomationFacade.GetRunCasesListIdsDistinct(data);
            DirectoryInfo folder = AutomationFacade.CreateNewJobFolder(id);
            AutomationFacade.SerializeData(folder, data);
            AutomationFacade.CreateTeamCityBuild(casesDistinct);
            return Ok();
        }

        //[AcceptVerbs("GET", "POST")]
        //public IHttpActionResult PrintAll(ulong id) {
        //    AutomationFacade.TestRailEngine.PrintAll();
        //    return Ok();
        //}


    }
}
