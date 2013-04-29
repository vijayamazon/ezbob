using System;
using System.Web.Mvc;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.Areas.Customer.Models;

namespace EzBob.Web.Code.ReportGenerator
{
    public class LoanScheduleReportResult:ActionResult
    {
         private readonly LoanDetails _loanDetails;
         private readonly bool _isExcell;
         private readonly Customer _customer;

         public LoanScheduleReportResult(LoanDetails loanDetails, bool isExcell, Customer customer)
         {
             _loanDetails = loanDetails;
             _isExcell = isExcell;
             _customer = customer;
         }

        public override void ExecuteResult(ControllerContext context)
        {
            var fileFormat = _isExcell ? "xls" : "pdf";
            var header = string.Format("Payment Schedule ({0}, {1}, {2})",
                            (_customer.PersonalInfo.FirstName +
                            ((_customer.PersonalInfo.MiddleInitial == null) ? " " : (" " + _customer.PersonalInfo.MiddleInitial) + " ") +
                            _customer.PersonalInfo.Surname),
                            _customer.Id, 
                            DateTime.Now.ToString("dd/MM/yyyy"));

            var generator = new LoanScheduleReportGenerator();
            var content = generator.GenerateReport(_loanDetails, _isExcell, header);
            var fileName = header + "." + fileFormat;
            var f = new FileContentResult(content, "application/"+fileFormat) { FileDownloadName = fileName };
            f.ExecuteResult(context);
        }
    }
}