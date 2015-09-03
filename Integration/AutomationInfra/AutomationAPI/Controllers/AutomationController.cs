namespace AutomationAPI.Controllers
{
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Web.Http;
    using AutomationAPI.Facade;
    using Microsoft.Win32.SafeHandles;

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

        [AcceptVerbs("GET", "POST")]
        public IHttpActionResult CreateAutomationTestPlan(int id)
        {
            AutomationFacade.CreateAutomationTestPlan(id);
            return Ok();
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetDependenciesReport() {
            Stream stream = AutomationFacade.GetDependenciesReport();
            var httpResponseMessage = new HttpResponseMessage();
            httpResponseMessage.Content = new StreamContent(stream);
            httpResponseMessage.Content.Headers.Add("Content-Type", "application/octet-stream");
            httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "DependenciesReport.csv"
            };
            return httpResponseMessage;
        }

        //[AcceptVerbs("GET", "POST")]
        //public IHttpActionResult PrintAll(ulong id) {
        //    AutomationFacade.TestRailEngine.PrintAll();
        //    return Ok();
        //}


    }
}
