namespace EzBob.Web.Areas.Underwriter.Controllers
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;
    using ConfigManager;
    using Ezbob.Backend.Models.NewLoan;
    using EzBob.Models;
    using EzBob.Web.Code;
    using EzBob.Web.Infrastructure;
    using EzBob.Web.Infrastructure.Attributes;
    using EZBob.DatabaseLib.Model.Database;
    using EZBob.DatabaseLib.Model.Database.Loans;
    using EZBob.DatabaseLib.Model.Loans;
    using EZBob.DatabaseLib.Repository;
    using log4net;
    using PaymentServices.Calculators;
    using ServiceClientProxy;
    using ServiceClientProxy.EzServiceReference;

    [RestfullErrorHandling]
    public class LoanEditorController : Controller
    {
        private readonly ILoanRepository _loans;
        private readonly ChangeLoanDetailsModelBuilder _builder;
        private readonly ICashRequestRepository _cashRequests;
        private readonly ChangeLoanDetailsModelBuilder _loanModelBuilder;
        private readonly LoanBuilder _loanBuilder;
        private readonly ILoanChangesHistoryRepository _history;
	    private ILoanOptionsRepository loanOptionsRepository;
        private readonly IWorkplaceContext _context;
	    private readonly ServiceClient serviceClient;
        private static readonly ILog Log = LogManager.GetLogger(typeof(LoanEditorController));

        public LoanEditorController(
			ILoanRepository loans, 
			ChangeLoanDetailsModelBuilder builder, 
			ICashRequestRepository cashRequests, 
			ChangeLoanDetailsModelBuilder loanModelBuilder, 
			LoanBuilder loanBuilder, 
			ILoanChangesHistoryRepository history, 
			IWorkplaceContext context, 
			ILoanOptionsRepository loanOptionsRepository)
        {
            _loans = loans;
            _builder = builder;
            _cashRequests = cashRequests;
            _loanModelBuilder = loanModelBuilder;
            _loanBuilder = loanBuilder;
            _history = history;
            _context = context;
	        this.loanOptionsRepository = loanOptionsRepository;
	        this.serviceClient = new ServiceClient();
        }

        [Ajax]
        [HttpGet]
        public JsonResult Loan(int id)
        {
            var loan = _loans.Get(id);

            var calc = new LoanRepaymentScheduleCalculator(loan, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);
            calc.GetState();

            var model = _builder.BuildModel(loan);

            model.Options = this.loanOptionsRepository.GetByLoanId(id) ?? LoanOptions.GetDefault(id);
            //TODO build loan model
            Log.DebugFormat("calculate offer for customer {0}", loan.Customer.Id);

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [Ajax]
        [HttpPost]
        [Transactional]
        public JsonResult RecalculateCR(EditLoanDetailsModel model)
        {
            var cr = _cashRequests.Get(model.CashRequestId);
            return Json(RecalculateModel(model, cr, model.Date));
        }

        [Ajax]
        [HttpPost]
        [Transactional]
        public JsonResult Recalculate(int id, EditLoanDetailsModel model)
        {
            var cr = _loans.Get(id).CashRequest;
            return Json(RecalculateModel(model, cr, DateTime.UtcNow));
        }

        [Ajax]
        [HttpPost]
        [Transactional]
        public JsonResult AddFreezeInterval(int id, string startdate, string enddate, decimal rate)
        {
            Loan oLoan = _loans.Get(id);

            oLoan.InterestFreeze.Add(new LoanInterestFreeze
            {
                Loan = oLoan,
                StartDate = (startdate == string.Empty) ? (DateTime?)null : DateTime.ParseExact(startdate, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                EndDate = (enddate == string.Empty) ? (DateTime?)null : DateTime.ParseExact(enddate, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                InterestRate = rate,
                ActivationDate = DateTime.UtcNow,
                DeactivationDate = null
            });

            _loans.SaveOrUpdate(oLoan);

            //TODO update loan (apply add freeze)
            Log.DebugFormat("apply loan modifications for customer {0}", oLoan.Customer.Id);


            var calc = new LoanRepaymentScheduleCalculator(oLoan, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);
            calc.GetState();

            EditLoanDetailsModel model = _builder.BuildModel(oLoan);

            return Json(model);
        } // AddFreezeInterval

        [Ajax]
        [HttpPost]
        [Transactional]
        public JsonResult RemoveFreezeInterval(int id, int intervalid) {
            Loan oLoan = _loans.Get(id);
            LoanInterestFreeze lif = oLoan.InterestFreeze.FirstOrDefault(v => v.Id == intervalid);
            if (lif != null)
                lif.DeactivationDate = DateTime.UtcNow;
            _loans.SaveOrUpdate(oLoan);
            //TODO update loan (apply remove freeze)
            Log.DebugFormat("apply loan modifications for customer {0}", oLoan.Customer.Id);

            var calc = new LoanRepaymentScheduleCalculator(oLoan, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);
            calc.GetState();
            EditLoanDetailsModel model = _builder.BuildModel(oLoan);
            return Json(model);
        } // RemoveFreezeInterval

        [Ajax]
        [HttpGet]
        [Transactional]
        public JsonResult LoanCR(long id)
        {
            var cr = _cashRequests.Get(id);
            var amount = cr.ApprovedSum();
            var loan = _loanBuilder.CreateLoan(cr, amount, DateTime.UtcNow);
            var model = _builder.BuildModel(loan);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [Ajax]
        [HttpPost]
        [Transactional]
        public JsonResult LoanCR(EditLoanDetailsModel model)
        {
            var cr = _cashRequests.Get(model.CashRequestId);

            model = RecalculateModel(model, cr, model.Date);

            cr.LoanTemplate = model.ToJSON();

            return Json(model);
        }

        [Ajax]
        [HttpPost]
        [Transactional]
        public JsonResult Loan(EditLoanDetailsModel model)
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

            //TODO update loan (apply all modifications)
            Log.DebugFormat("apply loan modifications for customer {0}", loan.Customer.Id);

            var calc = new LoanRepaymentScheduleCalculator(loan, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);
            calc.GetState();

            return Json(_loanModelBuilder.BuildModel(loan));
        }

        private EditLoanDetailsModel RecalculateModel(EditLoanDetailsModel model, CashRequest cr, DateTime now)
        {
            model.Validate();

            if (model.HasErrors)
                return model;

            var loan = _loanModelBuilder.CreateLoan(model);
            loan.LoanType = cr.LoanType;
            loan.CashRequest = cr;

            try
            {
                var calc = new LoanRepaymentScheduleCalculator(loan, now, CurrentValues.Instance.AmountToChargeFrom);
                calc.GetState();
            }
            catch (Exception e)
            {
                model.Errors.Add(e.Message);
                return model;
            }

            //TODO build loan model
            Log.DebugFormat("calculate offer for customer {0}", loan.Customer.Id);

            return _loanModelBuilder.BuildModel(loan);
        }

        [Ajax]
        [HttpPost]
        [Transactional]
		public JsonResult RescheduleLoan(
            int loanID,
            DbConstants.RepaymentIntervalTypes intervalType,  // month/week
            decimal? AmountPerInterval, // for "out" reschedule
            bool? stopAutoCharge, // "flag" - checkbox
            bool? stopLateFee, // checkbox
            bool? freezeInterest, //  checkbox
            int? stopAutoChargePayment,
            DateTime? lateFeeStartDate,
            DateTime? lateFeeEndDate,
            DateTime? freezeStartDate,
            DateTime? freezeEndDate,
			bool rescheduleIn = true,
            bool save = false
            ) {

            Loan loan = this._loans.Get(loanID);
	        DateTime now = DateTime.UtcNow;

            // loan options
            if (save)
            {
				// save LoanChangesHistory (loan state before changes) before new schedule
				var historyItem = new LoanChangesHistory {
					Data = this._loanModelBuilder.BuildModel(loan).ToJSON(),
					Date = DateTime.UtcNow,
					Loan = loan,
					User = this._context.User
				};
				this._history.Save(historyItem);

				if (freezeInterest == true) {
					loan.InterestFreeze.Add(new LoanInterestFreeze {
						Loan = loan,
						StartDate = freezeStartDate,
						EndDate = freezeEndDate,
						InterestRate = 0,
						ActivationDate = DateTime.UtcNow,
						DeactivationDate = null
					});
					this._loans.SaveOrUpdate(loan);
				}

                UpdateLoanOptions(loanID, intervalType, stopAutoCharge, stopLateFee, stopAutoChargePayment, lateFeeStartDate, lateFeeEndDate, now);
            }

	        save = false; // TEMPORARY - DONT'T SPOIL DB
			ReschedulingArgument reModel = new ReschedulingArgument();
			reModel.LoanType = loan.GetType().AssemblyQualifiedName;
			reModel.LoanID = loanID;
			reModel.SaveToDB = save;
			reModel.ReschedulingDate = now;
			reModel.ReschedulingRepaymentIntervalType = intervalType;
			reModel.RescheduleIn = rescheduleIn;
	        if(reModel.RescheduleIn==false) // "out"
				reModel.PaymentPerInterval = AmountPerInterval;

	        try {

		        // re strategy

				ReschedulingActionResult result = this.serviceClient.Instance.RescheduleLoan(this._context.User.Id, loan.Customer.Id, reModel);
				
				return Json(result.Value);

	        } catch (Exception editex) {
		        Log.Error(editex);
	        }

	        return null;
        }

		[NonAction]
	    private void UpdateLoanOptions(int loanID, DbConstants.RepaymentIntervalTypes intervalType, bool? stopAutoCharge, bool? stopLateFee, int? stopAutoChargePayment, DateTime? lateFeeStartDate, DateTime? lateFeeEndDate, DateTime now) {
		    if (stopAutoCharge != null || stopLateFee != null) {
				LoanOptions options = this.loanOptionsRepository.GetByLoanId(loanID) ?? LoanOptions.GetDefault(loanID);

			    if (stopLateFee == true) {
				    options.AutoLateFees = false;
				    options.StopLateFeeFromDate = lateFeeStartDate;
				    options.StopLateFeeToDate = lateFeeEndDate;
			    } else {
				    options.AutoLateFees = true;
				    options.StopLateFeeFromDate = null;
				    options.StopLateFeeToDate = null;
			    }

			    if (stopAutoCharge == true) {
				    options.AutoPayment = false;
				    DateTime? stopAutoChargeDate = null;

					if (stopAutoChargePayment.HasValue && stopAutoChargePayment.Value > 0) {
					    switch (intervalType) {
					    case DbConstants.RepaymentIntervalTypes.Month:
						    stopAutoChargeDate = now.AddMonths(stopAutoChargePayment.Value + 1);
						    break;
					    case DbConstants.RepaymentIntervalTypes.Week:
						    const int week = 7;
						    stopAutoChargeDate = now.AddDays((stopAutoChargePayment.Value + 1) * week);
						    break;
					    default:
						    Log.Warn("Not supported period " + intervalType);
						    break;
					    }
				    }

					if (stopAutoChargePayment.HasValue && stopAutoChargePayment.Value == 0) {
						stopAutoChargeDate = now.Date;
					}

				    options.StopAutoChargeDate = stopAutoChargeDate;
			    } else
				    options.AutoPayment = true;

			    this.loanOptionsRepository.SaveOrUpdate(options);
		    }
	    }
    }
}