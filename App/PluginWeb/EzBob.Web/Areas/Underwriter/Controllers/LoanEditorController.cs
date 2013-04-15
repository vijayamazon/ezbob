using System;
using System.Web.Mvc;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Models;
using EzBob.Web.Code;
using EzBob.Web.Infrastructure;
using EzBob.Web.Models;
using PaymentServices.Calculators;
using Scorto.Web;

namespace EzBob.Web.Areas.Underwriter.Controllers
{
    [RestfullErrorHandlingAttribute]
    public class LoanEditorController : Controller
    {
        private readonly ILoanRepository _loans;
        private readonly ChangeLoanDetailsModelBuilder _builder;
        private readonly ICashRequestRepository _cashRequests;
        private readonly ChangeLoanDetailsModelBuilder _loanModelBuilder;
        private readonly LoanBuilder _loanBuilder;
        private readonly ILoanChangesHistoryRepository _history;
        private readonly IWorkplaceContext _context;

        public LoanEditorController(ILoanRepository loans, ChangeLoanDetailsModelBuilder builder, ICashRequestRepository cashRequests, ChangeLoanDetailsModelBuilder loanModelBuilder, LoanBuilder loanBuilder, ILoanChangesHistoryRepository history, IWorkplaceContext context)
        {
            _loans = loans;
            _builder = builder;
            _cashRequests = cashRequests;
            _loanModelBuilder = loanModelBuilder;
            _loanBuilder = loanBuilder;
            _history = history;
            _context = context;
        }

        [Ajax]
        [HttpGet]
        [Transactional]
        public JsonNetResult Loan(int id)
        {
            var loan = _loans.Get(id);

            var calc = new PayEarlyCalculator2(loan, DateTime.UtcNow);
            calc.GetState();

            var model = _builder.BuildModel(loan);
            return this.JsonNet(model);
        }

        [Ajax]
        [HttpPost]
        [Transactional]
        public JsonNetResult RecalculateCR(EditLoanDetailsModel model)
        {
            var cr = _cashRequests.Get(model.CashRequestId);
            return this.JsonNet(RecalculateModel(model, cr, model.Date));
        }

        [Ajax]
        [HttpPost]
        [Transactional]
        public JsonNetResult Recalculate(int id, EditLoanDetailsModel model)
        {
            var cr = _loans.Get(id).CashRequest;
            return this.JsonNet(RecalculateModel(model, cr, DateTime.UtcNow));
        }

        [Ajax]
        [HttpGet]
        [Transactional]
        public JsonNetResult LoanCR(long id)
        {
            var cr = _cashRequests.Get(id);
            var amount = cr.ApprovedSum();
            var loan = _loanBuilder.CreateLoan(cr, amount, DateTime.UtcNow);
            var model = _builder.BuildModel(loan);
            return this.JsonNet(model);
        }

        [Ajax]
        [HttpPost]
        [Transactional]
        public JsonNetResult LoanCR(EditLoanDetailsModel model)
        {
            var cr = _cashRequests.Get(model.CashRequestId);

            model = RecalculateModel(model, cr, model.Date);

            cr.LoanTemplate = model.ToJSON();
            return this.JsonNet(model);
        }

        [Ajax]
        [HttpPost]
        [Transactional]
        public JsonNetResult Loan(EditLoanDetailsModel model)
        {
            var loan = _loans.Get(model.Id);

            var historyItem = new LoanChangesHistory
            {
                Data = _loanModelBuilder.BuildModel(loan).ToJSON(),
                Date = DateTime.UtcNow,
                Loan = loan,
                User = _context.User
            };
            _history.Save(historyItem);

            _loanModelBuilder.UpdateLoan(model, loan);

            var calc = new PayEarlyCalculator2(loan, DateTime.UtcNow);
            calc.GetState();

            return this.JsonNet(_loanModelBuilder.BuildModel(loan));
        }

        private EditLoanDetailsModel RecalculateModel(EditLoanDetailsModel model, CashRequest cr, DateTime now)
        {
            model.Validate();

            if (model.HasErrors) return model;

            var loan = _loanModelBuilder.CreateLoan(model);
            loan.LoanType = cr.LoanType;
            loan.CashRequest = cr;

            try
            {
                var calc = new PayEarlyCalculator2(loan, now);
                calc.GetState();
            }
            catch (Exception e)
            {
                model.Errors.Add(e.Message);
            }

            model = _loanModelBuilder.BuildModel(loan);

            return model;
        }
    }
}