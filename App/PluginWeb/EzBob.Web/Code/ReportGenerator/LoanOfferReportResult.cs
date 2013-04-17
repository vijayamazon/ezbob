using System.Web.Mvc;
using EzBob.Models;


namespace EzBob.Web.Code.ReportGenerator
{
    public class LoanOfferReportResult:ActionResult
    {
         private readonly LoanOffer _loanOffer; 
         private readonly bool _isExcel;
         private readonly bool _isShowDetails;
        
        public LoanOfferReportResult(LoanOffer loanOffer, bool isExcel, bool isShowDetails)
        {
            _loanOffer = loanOffer;
            _isExcel = isExcel;
            _isShowDetails = isShowDetails;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var fileFormat = _isExcel ? "xls" : "pdf";
            var generator = new LoanOfferReportGenerator();
            var content = generator.GenerateReport(_loanOffer, _isExcel, _isShowDetails);
            var f = new FileContentResult(content, "application/"+fileFormat) { FileDownloadName = "LoanOffer."+fileFormat };
            f.ExecuteResult(context);
        }
    }
}