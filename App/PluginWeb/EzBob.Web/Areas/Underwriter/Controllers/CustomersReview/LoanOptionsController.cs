namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using ConfigManager;
    using DbConstants;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Utils;
    using EzBob.Web.Areas.Underwriter.Models;
    using EzBob.Web.Infrastructure;
    using EzBob.Web.Infrastructure.Attributes;
    using EzBob.Web.Infrastructure.csrf;
    using EZBob.DatabaseLib.Model.Database;
    using EZBob.DatabaseLib.Model.Database.Loans;
    using EZBob.DatabaseLib.Repository;
    using log4net;
    using ServiceClientProxy;
    using StructureMap;

    public class LoanOptionsController : Controller
    {
        private readonly ICustomerStatusHistoryRepository customerStatusHistoryRepository;
        private readonly CustomerStatusesRepository customerStatusesRepository;
        private readonly ILoanOptionsRepository _loanOptionsRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly ICaisFlagRepository _caisFlagRepository;
        private readonly ServiceClient serviceClient;
        private readonly IEzbobWorkplaceContext context;
        protected static readonly ILog Log = LogManager.GetLogger(typeof(LoanOptionsController));
        public LoanOptionsController(
            ILoanOptionsRepository loanOptionsRepository,
            ILoanRepository loanRepository,
            ICustomerStatusHistoryRepository customerStatusHistoryRepository,
            CustomerStatusesRepository customerStatusesRepository, IEzbobWorkplaceContext context)
        {
            this._loanOptionsRepository = loanOptionsRepository;
            this._loanRepository = loanRepository;
            this._caisFlagRepository = ObjectFactory.GetInstance<CaisFlagRepository>();
            this.customerStatusHistoryRepository = customerStatusHistoryRepository;
            this.customerStatusesRepository = customerStatusesRepository;
            this.context = context;
            this.serviceClient = new ServiceClient();
        }

        [Ajax]
        [HttpGet]
        public JsonResult Index(int loanId)
        {
            var options = this._loanOptionsRepository.GetByLoanId(loanId) ?? LoanOptions.GetDefault(loanId);
            var loan = this._loanRepository.Get(loanId);
            var flags = this._caisFlagRepository.GetForStatusType();
            var model = new LoanOptionsViewModel(options, loan, flags);
            return Json(model, JsonRequestBehavior.AllowGet);
        }


        [Transactional]
        [Ajax]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
		[Permission(Name = "LoanOptions")]
        public JsonResult Save(LoanOptions options)
        {
            if (options.ManualCaisFlag == "T")
                options.ManualCaisFlag = "Calculated value";

            this._loanOptionsRepository.SaveOrUpdate(options);
            Customer customer = this._loanRepository.Get(options.LoanId).Customer;
            NL_SaveLoanOptions(customer, options);

            if (options.CaisAccountStatus == "8")
            {
                int minDectForDefault = CurrentValues.Instance.MinDectForDefault;

                Loan triggeringLoan = null;

                // Update loan options
                foreach (Loan loan in customer.Loans)
                {
                    if (loan.Id == options.LoanId)
                    {
                        triggeringLoan = loan;
                        continue;
                    }

                    if (loan.Status == LoanStatus.PaidOff || loan.Balance < minDectForDefault)
                    {
                        continue;
                    }

                    LoanOptions currentOptions = this._loanOptionsRepository.GetByLoanId(loan.Id) ?? new LoanOptions
                    {
                        LoanId = loan.Id,
                        AutoPayment = true,
                        ReductionFee = true,
                        LatePaymentNotification = true,
                        EmailSendingAllowed = false,
                        MailSendingAllowed = false,
                        SmsSendingAllowed = false,
                        ManualCaisFlag = "Calculated value",
                        AutoLateFees = true
                    };

                    currentOptions.CaisAccountStatus = "8";
                    this._loanOptionsRepository.SaveOrUpdate(currentOptions);
                    NL_SaveLoanOptions(customer, options);
                }
                

                // Update customer status
                CustomerStatuses prevStatus = customer.CollectionStatus;
                customer.CollectionStatus = this.customerStatusesRepository.Get((int)CollectionStatusNames.Default);
                customer.CollectionDescription = string.Format("Triggered via loan options:{0}", triggeringLoan != null ? triggeringLoan.RefNumber : "unknown");

                // Update status history table
                var newEntry = new CustomerStatusHistory
                {
                    Username = User.Identity.Name,
                    Timestamp = DateTime.UtcNow,
                    CustomerId = customer.Id,
                    PreviousStatus = prevStatus,
                    NewStatus = customer.CollectionStatus,
                };
                this.customerStatusHistoryRepository.SaveOrUpdate(newEntry);
            }

            return Json(new { });
        }


        private void NL_SaveLoanOptions(Customer customer,
                                        LoanOptions options)
        {
            Log.DebugFormat("update loan options for loan {0}", options.LoanId);

            //NL Loan Options
            NL_LoanOptions nlOptions = new NL_LoanOptions()
            {
                LoanID = options.LoanId,
                CaisAccountStatus = options.CaisAccountStatus,
                EmailSendingAllowed = options.EmailSendingAllowed,
                LatePaymentNotification = options.LatePaymentNotification,
                LoanOptionsID = options.Id,
                MailSendingAllowed = options.MailSendingAllowed,
                ManualCaisFlag = options.ManualCaisFlag,
                PartialAutoCharging = options.ReductionFee,
                SmsSendingAllowed = options.SmsSendingAllowed,
                StopAutoChargeDate = MiscUtils.NL_GetStopAutoChargeDate(options.AutoPayment, options.StopAutoChargeDate),
                StopLateFeeFromDate = MiscUtils.NL_GetLateFeeDates(options.AutoLateFees, options.StopLateFeeFromDate, options.StopLateFeeToDate).Item1,
                StopLateFeeToDate = MiscUtils.NL_GetLateFeeDates(options.AutoLateFees, options.StopLateFeeFromDate, options.StopLateFeeToDate).Item2,
                UserID = this.context.UserId,
                InsertDate = DateTime.Now,
                IsActive = true,
                Notes = "From Loan Options Controller",
            };

            /*var PropertiesUpdateList = new List<String>() {
		        "StopAutoChargeDate",
                "StopLateFeeFromDate",
		        "StopLateFeeToDate",
		    };*/

            var nlStrategy = this.serviceClient.Instance.AddLoanOptions(this.context.UserId, customer.Id, nlOptions, options.LoanId, null); // , PropertiesUpdateList.ToArray()
            Log.DebugFormat("NL LoanOptions save: LoanOptionsID: {0}, Error: {1}", nlStrategy.Value, nlStrategy.Error);
        }


       
    }
}
