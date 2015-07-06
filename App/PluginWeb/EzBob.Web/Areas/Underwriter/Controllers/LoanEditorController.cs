namespace EzBob.Web.Areas.Underwriter.Controllers
{
    using System;
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
    using NHibernate;
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
	    private readonly ISession session;
        private static readonly ILog Log = LogManager.GetLogger(typeof(LoanEditorController));

        public LoanEditorController(
            ILoanRepository loans,
            ChangeLoanDetailsModelBuilder builder,
            ICashRequestRepository cashRequests,
            ChangeLoanDetailsModelBuilder loanModelBuilder,
            LoanBuilder loanBuilder,
            ILoanChangesHistoryRepository history,
            IWorkplaceContext context,
            ILoanOptionsRepository loanOptionsRepository, 
			ISession session)
        {
			this._loans = loans;
			this._builder = builder;
			this._cashRequests = cashRequests;
			this._loanModelBuilder = loanModelBuilder;
			this._loanBuilder = loanBuilder;
			this._history = history;
			this._context = context;
            this.loanOptionsRepository = loanOptionsRepository;
	        this.session = session;
	        this.serviceClient = new ServiceClient();
        }

        [Ajax]
        [HttpGet]
        [NoCache]
        public JsonResult Loan(int id)
        {
			var loan = this._loans.Get(id);

            var calc = new LoanRepaymentScheduleCalculator(loan, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);
            calc.GetState();

			var model = this._builder.BuildModel(loan);

            model.Options = this.loanOptionsRepository.GetByLoanId(id) ?? LoanOptions.GetDefault(id);

            ReschedulingArgument reModel = new ReschedulingArgument();
            reModel.LoanType = loan.GetType().AssemblyQualifiedName;
            reModel.LoanID = id;
            reModel.SaveToDB = false;
            reModel.ReschedulingDate = DateTime.UtcNow;
            reModel.ReschedulingRepaymentIntervalType = DbConstants.RepaymentIntervalTypes.Month;
            reModel.RescheduleIn = true;

            try
            {
                ReschedulingActionResult result = this.serviceClient.Instance.RescheduleLoan(this._context.User.Id, loan.Customer.Id, reModel);
                model.ReResultIn = result.Value;
                //model.ReschedulingINNotification = result.Value.Error;
                Log.Debug(string.Format("IN=={0}, {1}", reModel, result.Value));
            }
            catch (Exception editex)
            {
                Log.Error(editex);
            }

            reModel.RescheduleIn = false;
            reModel.PaymentPerInterval = 0m;
            try
            {
                ReschedulingActionResult result = this.serviceClient.Instance.RescheduleLoan(this._context.User.Id, loan.Customer.Id, reModel);
                model.ReResultOut = result.Value;
                //model.OutsideAmount = reModel.PaymentPerInterval;
                //model.DefaultPaymentPerInterval = result.Value.DefaultPaymentPerInterval;
                Log.Debug(string.Format("OUT=={0}, {1}", reModel, result.Value));
            }
            catch (Exception editex)
            {
                Log.Error(editex);
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [Ajax]
        [HttpPost]
        [Transactional]
        public JsonResult RecalculateCR(EditLoanDetailsModel model)
        {
			var cr = this._cashRequests.Get(model.CashRequestId);
            return Json(RecalculateModel(model, cr, model.Date));
        }

        /// <summary>
        /// called on deleting/adding one installment etc.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [Ajax]
        [HttpPost]
        [Transactional]
        public JsonResult Recalculate(int id, EditLoanDetailsModel model)
        {
			var cr = this._loans.Get(id).CashRequest;
            return Json(RecalculateModel(model, cr, DateTime.UtcNow));
        }

        [Ajax]
        [HttpGet]
        [Transactional]
        public JsonResult LoanCR(long id)
        {
			var cr = this._cashRequests.Get(id);
            var amount = cr.ApprovedSum();
			var loan = this._loanBuilder.CreateLoan(cr, amount, DateTime.UtcNow);
			var model = this._builder.BuildModel(loan);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [Ajax]
        [HttpPost]
        [Transactional]
        public JsonResult LoanCR(EditLoanDetailsModel model)
        {
			var cr = this._cashRequests.Get(model.CashRequestId);

            model = RecalculateModel(model, cr, model.Date);

            cr.LoanTemplate = model.ToJSON();

            return Json(model);
        }

        /// <summary>
        /// saving all modifications - deleting/adding of installments etc.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Ajax]
        [HttpPost]
        [Transactional]
        public JsonResult Loan(EditLoanDetailsModel model)
        {
			var loan = this._loans.Get(model.Id);

            var historyItem = new LoanChangesHistory
            {
				Data = this._loanModelBuilder.BuildModel(loan).ToJSON(),
                Date = DateTime.UtcNow,
                Loan = loan,
				User = this._context.User
            };
			this._history.Save(historyItem);

			this._loanModelBuilder.UpdateLoan(model, loan);

            //TODO update loan (apply all modifications)
            Log.DebugFormat("apply loan modifications for customer {0}", loan.Customer.Id);

            var calc = new LoanRepaymentScheduleCalculator(loan, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);
            calc.GetState();

			return Json(this._loanModelBuilder.BuildModel(loan));
        }

        private EditLoanDetailsModel RecalculateModel(EditLoanDetailsModel model, CashRequest cr, DateTime now)
        {
            model.Validate();

            if (model.HasErrors)
                return model;

			var loan = this._loanModelBuilder.CreateLoan(model);
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
            //Log.DebugFormat("calculate offer for customer {0}", loan.Customer.Id);

			return this._loanModelBuilder.BuildModel(loan);
        }

        [Ajax]
        [HttpPost]
        public JsonResult RescheduleLoan(
            int loanID,
            DbConstants.RepaymentIntervalTypes? intervalType,  // month/week
            decimal? AmountPerInterval, // for "out" reschedule
            bool? stopAutoCharge, // "flag" - checkbox
            bool? stopLateFee, // checkbox
            bool? freezeInterest, //  checkbox
            int? stopAutoChargePayment,
            DateTime? lateFeeStartDate,
            DateTime? lateFeeEndDate,
            DateTime? freezeStartDate,
            DateTime? freezeEndDate,
            bool? rescheduleIn,
            bool save = false
            ){

            try
            {
                Loan loan = this._loans.Get(loanID);
                DateTime now = DateTime.UtcNow;

                //  loan options
                if (save) {
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
	                    this.session.Flush();
                    }
                }
                //  ### loan options
	            ReschedulingActionResult result = null;

                if (rescheduleIn != null)
                {
                    ReschedulingArgument reModel = new ReschedulingArgument();
                    reModel.LoanType = loan.GetType()
                        .AssemblyQualifiedName;
                    reModel.LoanID = loanID;
                    reModel.SaveToDB = save;
                    reModel.ReschedulingDate = now;
                    reModel.ReschedulingRepaymentIntervalType = (DbConstants.RepaymentIntervalTypes)intervalType;
                    reModel.RescheduleIn = (bool)rescheduleIn;

                    if (reModel.RescheduleIn == false) // "out"
                        reModel.PaymentPerInterval = AmountPerInterval;

                    // re strategy
                    result = this.serviceClient.Instance.RescheduleLoan(this._context.User.Id, loan.Customer.Id, reModel);

                    Log.Debug(string.Format("ReschedulingResult: {0}, {1}", reModel, result.Value));

                    
                }

	            if (save) {
		            this.session.Refresh(loan);
		            UpdateLoanOptions(loan, stopAutoCharge, stopLateFee, stopAutoChargePayment, lateFeeStartDate, lateFeeEndDate, now);
	            }
	            return Json(result.Value);

            }
            catch (Exception editex)
            {
                Log.Error("rescheduling editor EXCEPTION: " + editex);
            }

            return null;
        }

        [NonAction]
        private void UpdateLoanOptions(Loan loan, bool? stopAutoCharge, bool? stopLateFee, int? stopAutoChargePayment, DateTime? lateFeeStartDate, DateTime? lateFeeEndDate, DateTime now)
        {
            if (stopAutoCharge != null || stopLateFee != null)
            {
                LoanOptions options = this.loanOptionsRepository.GetByLoanId(loan.Id) ?? LoanOptions.GetDefault(loan.Id);

                if (stopLateFee == true)
                {
                    options.AutoLateFees = false;
                    options.StopLateFeeFromDate = lateFeeStartDate;
                    options.StopLateFeeToDate = lateFeeEndDate;
                }
                else
                {
                    options.AutoLateFees = true;
                    options.StopLateFeeFromDate = null;
                    options.StopLateFeeToDate = null;
                }

                if (stopAutoCharge == true)
                {
                    options.AutoPayment = false;
                    DateTime? stopAutoChargeDate = null;

                    if (stopAutoChargePayment.HasValue && stopAutoChargePayment.Value > 0) {
	                    var loanSchedulesOrdered = loan.Schedule.Where(x => x.Date > now).OrderBy(x => x.Date).ToArray();
						if (loanSchedulesOrdered.Any() && loanSchedulesOrdered.Count() >= stopAutoChargePayment.Value) {
							stopAutoChargeDate = loanSchedulesOrdered[stopAutoChargePayment.Value-1].Date.Date.AddDays(1);
						} else {
							Log.ErrorFormat("Stop payment after {0} payments is impossible, new schedule have only {1} payments left. LoanID {2}",
								stopAutoChargePayment.Value, loanSchedulesOrdered.Count(), loan.Id);
						}
                    }
                    
                    if (stopAutoChargePayment.HasValue && stopAutoChargePayment.Value == 0)
                    {
                        stopAutoChargeDate = now.Date;
                    }

                    options.StopAutoChargeDate = stopAutoChargeDate;
                }
                else
                    options.AutoPayment = true;

                this.loanOptionsRepository.SaveOrUpdate(options);
	            this.session.Flush();
            }
        }

        //[Ajax]
        //[HttpPost]
        //[Transactional]
        //public JsonResult AddFreezeInterval(int id, string startdate, string enddate, decimal rate)
        //{
        //	Loan oLoan = _loans.Get(id);

        //	oLoan.InterestFreeze.Add(new LoanInterestFreeze
        //	{
        //		Loan = oLoan,
        //		StartDate = (startdate == string.Empty) ? (DateTime?)null : DateTime.ParseExact(startdate, "yyyy-MM-dd", CultureInfo.InvariantCulture),
        //		EndDate = (enddate == string.Empty) ? (DateTime?)null : DateTime.ParseExact(enddate, "yyyy-MM-dd", CultureInfo.InvariantCulture),
        //		InterestRate = rate,
        //		ActivationDate = DateTime.UtcNow,
        //		DeactivationDate = null
        //	});

        //	_loans.SaveOrUpdate(oLoan);

        //	//TODO update loan (apply add freeze)
        //	Log.DebugFormat("apply loan modifications for customer {0}", oLoan.Customer.Id);

        //	var calc = new LoanRepaymentScheduleCalculator(oLoan, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);
        //	calc.GetState();

        //	EditLoanDetailsModel model = _builder.BuildModel(oLoan);

        //	return Json(model);
        //} // AddFreezeInterval

        [Ajax]
        [HttpPost]
        [Transactional]
        public JsonResult RemoveFreezeInterval(int id, int intervalid)
        {
            Loan oLoan = this._loans.Get(id);
            LoanInterestFreeze lif = oLoan.InterestFreeze.FirstOrDefault(v => v.Id == intervalid);
            if (lif != null)
                lif.DeactivationDate = DateTime.UtcNow;
			this._loans.SaveOrUpdate(oLoan);
            //TODO update loan (apply remove freeze)
            Log.DebugFormat("apply loan modifications for customer {0}", oLoan.Customer.Id);

            var calc = new LoanRepaymentScheduleCalculator(oLoan, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);
            calc.GetState();
			EditLoanDetailsModel model = this._builder.BuildModel(oLoan);

            model.Options = this.loanOptionsRepository.GetByLoanId(id) ?? LoanOptions.GetDefault(id);

            ReschedulingArgument reModel = new ReschedulingArgument();
            reModel.LoanType = oLoan.GetType().AssemblyQualifiedName;
            reModel.LoanID = id;
            reModel.SaveToDB = false;
            reModel.ReschedulingDate = DateTime.UtcNow;
            reModel.ReschedulingRepaymentIntervalType = DbConstants.RepaymentIntervalTypes.Month;
            reModel.RescheduleIn = true;

            try
            {
                ReschedulingActionResult result = this.serviceClient.Instance.RescheduleLoan(this._context.User.Id, oLoan.Customer.Id, reModel);
                model.ReResultIn = result.Value;
                //model.ReschedulingINNotification = result.Value.Error;
                Log.Debug(string.Format("IN=={0}, {1}", reModel, result.Value));
            }
            catch (Exception editex)
            {
                Log.Error(editex);
            }

            reModel.RescheduleIn = false;
            reModel.PaymentPerInterval = 0m;
            try
            {
                ReschedulingActionResult result = this.serviceClient.Instance.RescheduleLoan(this._context.User.Id, oLoan.Customer.Id, reModel);
                model.ReResultOut = result.Value;
                //model.OutsideAmount = reModel.PaymentPerInterval;
                //model.DefaultPaymentPerInterval = result.Value.DefaultPaymentPerInterval;
                Log.Debug(string.Format("OUT=={0}, {1}", reModel, result.Value));
            }
            catch (Exception editex)
            {
                Log.Error(editex);
            }


            return Json(model);
        } // RemoveFreezeInterval
    }
}