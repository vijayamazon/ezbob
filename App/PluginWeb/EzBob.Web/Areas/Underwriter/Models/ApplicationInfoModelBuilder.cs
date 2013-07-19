namespace EzBob.Web.Areas.Underwriter.Models
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using Configuration;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Loans;
	using EzBob.Models;
	using Code;
	using Infrastructure;
	using MailApi;
	using StructureMap;
	using log4net;

	public class ApplicationInfoModelBuilder
	{
		private readonly IPacNetBalanceRepository _funds;
		private readonly IPacNetManualBalanceRepository _manualFunds;
        private readonly RepaymentCalculator _repaymentCalculator = new RepaymentCalculator();
        private readonly ILoanTypeRepository _loanTypes;
		private readonly IDiscountPlanRepository _discounts;
		private static readonly IEzBobConfiguration config = ObjectFactory.GetInstance<IEzBobConfiguration>();
		private Mail _mail;
		private static readonly ILog log = LogManager.GetLogger(typeof(ApplicationInfoModelBuilder));

		public ApplicationInfoModelBuilder(
			IPacNetBalanceRepository funds,
			IPacNetManualBalanceRepository manualFunds, 
            IDiscountPlanRepository discounts, 
            ILoanTypeRepository loanTypes)
		{
			_funds = funds;
			_manualFunds = manualFunds;
            _discounts = discounts;
            _loanTypes = loanTypes;

			var config = new MandrillConfig();
			_mail = new Mail(config);
		}

		public DateTime? InitApplicationInfo(ApplicationInfoModel model, Customer customer, CashRequest cr, DateTime? timeOfLastAlert)
        {
            if (customer == null) return null;

            model.Id = customer.Id;

            if (cr != null)
            {
                model.InterestRate = cr.InterestRate;
                model.CashRequestId = cr.Id;
                model.UseSetupFee = cr.UseSetupFee;
                model.AllowSendingEmail = !cr.EmailSendingBanned;

                var loanType = cr.LoanType ?? _loanTypes.GetDefault();

                model.LoanType = loanType.Name;
                model.LoanTypeId = loanType.Id;

                cr.OfferStart = cr.OfferStart ?? customer.OfferStart;
                cr.OfferValidUntil = cr.OfferValidUntil ?? customer.OfferValidUntil;

                model.RepaymentPerion = _repaymentCalculator.ReCalculateRepaymentPeriod(cr);
            }

            model.CustomerId = customer.Id;
            model.IsTest = customer.IsTest;
            model.IsAvoid = customer.IsAvoid;
            model.SystemDecision = customer.Status.ToString();

            if (cr.SystemCalculatedSum != null && cr.SystemCalculatedSum.Value != 0)
            {
                model.SystemCalculatedAmount = Convert.ToDecimal(cr.SystemCalculatedSum.Value);
            }

            model.OfferedCreditLine = Convert.ToDecimal(cr.ManagerApprovedSum ?? cr.SystemCalculatedSum);
            model.BorrowedAmount = customer.Loans.Where(x => x.CashRequest != null && x.CashRequest.Id == cr.Id).Sum(x => x.LoanAmount);
            model.AvaliableAmount = customer.CreditSum ?? 0M;
            model.OfferExpired = cr.OfferValidUntil <= DateTime.UtcNow;

            model.StartingFromDate = FormattingUtils.FormatDateToString(cr.OfferStart);
            model.OfferValidateUntil = FormattingUtils.FormatDateToString(cr.OfferValidUntil);

			var balance = _funds.GetBalance();
			var manualBalance = _manualFunds.GetBalance();
	        var fundsAvailable = balance.Adjusted + manualBalance;
			model.FundsAvaliable = FormattingUtils.FormatPounds(fundsAvailable);

	        DateTime today = DateTime.UtcNow;
	        model.FundsAvailableUnderLimitClass = string.Empty;
	        int relevantLimit = (today.DayOfWeek == DayOfWeek.Thursday || today.DayOfWeek == DayOfWeek.Friday) ? config.PacnetBalanceWeekendLimit : config.PacnetBalanceWeekdayLimit;
	        if (fundsAvailable < relevantLimit)
			{
				model.FundsAvailableUnderLimitClass = "red_cell";
				timeOfLastAlert = SendMail(fundsAvailable, relevantLimit, timeOfLastAlert);
			}

	        model.FundsReserved = FormattingUtils.FormatPounds(balance.ReservedAmount);
            //Status = "Active";
            model.Details = customer.Details;
            var isWaitingOrEscalated = customer.CreditResult == CreditResultStatus.WaitingForDecision ||
                                       customer.CreditResult == CreditResultStatus.Escalated;

            var isEnabled = customer.CollectionStatus.CurrentStatus == CollectionStatusType.Enabled;
            model.Editable = isWaitingOrEscalated && cr != null && isEnabled;

            model.IsModified = !string.IsNullOrEmpty(cr.LoanTemplate);

            model.LoanTypes = _loanTypes.GetAll().Select(t => LoanTypesModel.Create(t)).ToArray();

            model.DiscountPlans = _discounts.GetAll().Select(d => DiscountPlanModel.Create(d)).ToArray();
            var discountPlan = (cr.DiscountPlan ?? _discounts.GetDefault());
            model.DiscountPlan = discountPlan.Name;
            model.DiscountPlanPercents = discountPlan.Discounts.Any(d => d != 0) ? string.Format("({0})", discountPlan.ValuesStr) : "";
            model.DiscountPlanId = discountPlan.Id;

            model.Reason = cr.UnderwriterComment;

            model.IsLoanTypeSelectionAllowed = cr.IsLoanTypeSelectionAllowed;

			return timeOfLastAlert;
        }

		private DateTime? SendMail(decimal currentFunds, int requiredFunds, DateTime? timeOfLastAlert)
		{
			if (timeOfLastAlert.HasValue && timeOfLastAlert.Value.AddSeconds(config.NotEnoughFundsInterval) > DateTime.UtcNow)
			{
				return timeOfLastAlert;
			}

			var vars = new Dictionary<string, string>
                {
                    {"CurrentFunds", currentFunds.ToString("N2", CultureInfo.InvariantCulture)},
                    {"RequiredFunds", requiredFunds.ToString("N", CultureInfo.InvariantCulture)} 
                };

			var result = _mail.Send(vars, config.NotEnoughFundsToAddess, config.NotEnoughFundsTemplateName);
			if (result == "OK")
			{
				timeOfLastAlert = DateTime.UtcNow;
			}
			else
			{
				log.ErrorFormat("Failed sending alert mail");
			}

			return timeOfLastAlert;
		}
	}
}