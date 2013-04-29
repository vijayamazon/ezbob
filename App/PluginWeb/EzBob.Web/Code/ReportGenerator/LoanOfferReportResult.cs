using System.Web.Mvc; 
using EZBob.DatabaseLib.Model.Database;
using EzBob.Models;

namespace EzBob.Web.Code.ReportGenerator
{
    public class LoanOfferReportResult:ActionResult
    {
         private readonly LoanOffer _loanOffer; 
         private readonly bool _isExcel;
         private readonly bool _isShowDetails;
         private readonly Customer _customer;
        
        public LoanOfferReportResult(LoanOffer loanOffer, bool isExcel, bool isShowDetails, Customer customer)
        {
            _loanOffer = loanOffer;
            _isExcel = isExcel;
            _isShowDetails = isShowDetails;
            _customer = customer;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var fileFormat = _isExcel ? "xls" : "pdf";
            var generator = new LoanOfferReportGenerator();
            var header = string.Format("LoanOffer ({0}, {1}, {2})",
                                       (_customer.PersonalInfo.FirstName +
                                        ((_customer.PersonalInfo.MiddleInitial == null) ? " ": (" " + _customer.PersonalInfo.MiddleInitial) + " ") + 
                                        _customer.PersonalInfo.Surname
                ),
                _customer.Id, _loanOffer.Details.Date.ToString("dd_MM_yyyy"));
            var content = generator.GenerateReport(_loanOffer, _isExcel, _isShowDetails, header);
            var fileName = header+"."+fileFormat;
            var f = new FileContentResult(content, "application/"+fileFormat) { FileDownloadName = fileName };
            f.ExecuteResult(context);
        }
    }
}