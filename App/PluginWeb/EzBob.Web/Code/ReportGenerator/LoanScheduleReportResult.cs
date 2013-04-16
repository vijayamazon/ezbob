using System.Web.Mvc;
using EzBob.Web.Areas.Customer.Models;

namespace EzBob.Web.Code.ReportGenerator
{
    public class LoanScheduleReportResult:ActionResult
    {
         private readonly LoanDetails _loanDetails;
         private readonly bool _isExcell;

         public LoanScheduleReportResult(LoanDetails loanDetails, bool isExcell)
         {
             _loanDetails = loanDetails;
             _isExcell = isExcell;
         }

        public override void ExecuteResult(ControllerContext context)
        {
            var fileFormat = _isExcell ? "xls" : "pdf";
            var generator = new LoanScheduleReportGenerator();
            var content = generator.GenerateReport(_loanDetails, _isExcell);
            var f = new FileContentResult(content, "application/"+fileFormat) { FileDownloadName = "LoanShedule."+fileFormat };
            f.ExecuteResult(context);
        }
    }
}