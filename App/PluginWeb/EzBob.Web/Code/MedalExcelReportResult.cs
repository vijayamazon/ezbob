using System.Web.Mvc;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.Areas.Underwriter.Models;

namespace EzBob.Web.Code
{
    public class MedalExcelReportResult: ActionResult
    {
        private readonly Customer _customer;

        public MedalExcelReportResult(Customer customer)
        {
            _customer = customer;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var medalCalculator = new MedalCalculators(_customer);
            var generator = new MedalExcelReportGenerator();
            var content = generator.GenerateReport(medalCalculator, _customer.PersonalInfo.Fullname);
            var f = new FileContentResult(content, "application/xls") {FileDownloadName = "Medal Calculations(" + _customer.PersonalInfo.Fullname + ").xls"};
            f.ExecuteResult(context);
        }
    }
}