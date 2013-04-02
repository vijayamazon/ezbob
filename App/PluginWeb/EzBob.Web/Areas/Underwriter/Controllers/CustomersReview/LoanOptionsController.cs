using System.Web.Mvc;
using EZBob.DatabaseLib.Model;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Repository;
using EzBob.Web.Areas.Underwriter.Models;
using EzBob.Web.Infrastructure.csrf;
using Scorto.Web;
using StructureMap;

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
    public class LoanOptionsController : Controller
    {
        private readonly ILoanOptionsRepository _loanOptionsRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly ICaisFlagRepository _caisFlagRepository;

        public LoanOptionsController (ILoanOptionsRepository loanOptionsRepository, ILoanScheduleRepository loanScheduleRepository, ILoanRepository loanRepository)
        {
            _loanOptionsRepository = loanOptionsRepository;
            _loanRepository = loanRepository;
            _caisFlagRepository = ObjectFactory.GetInstance<CaisFlagRepository>();
        }

        [Ajax]
        [HttpGet]
        [Transactional]
        public JsonNetResult Index(int loanId)
        {
            var options = _loanOptionsRepository.GetByLoanId(loanId) ?? SetDefaultStatus(loanId);
            var loan = _loanRepository.Get(loanId);
            var flags = _caisFlagRepository.GetForStatusType(loan.Customer.CollectionStatus.CurrentStatus);
            var model = new LoanOptionsViewModel(options, loan, flags);
            return this.JsonNet(model);
        }

        private LoanOptions SetDefaultStatus(int loanid)
        {
            var options = new LoanOptions
                              {
                                  AutoPayment = true,
                                  LatePaymentNotification = true,
                                  ReductionFee = true,
                                  StopSendingEmails = true,
                                  CaisAccountStatus = "Calculated value",
                                  LoanId = loanid
                              };
            return options;
        }

        [Transactional]
        [Ajax]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public JsonNetResult Save(LoanOptions options)
         {
             if (options.ManulCaisFlag == "T")
                 options.ManulCaisFlag = "Calculated value";

             _loanOptionsRepository.SaveOrUpdate(options);
             return this.JsonNet(new { });
         }
    }
}
