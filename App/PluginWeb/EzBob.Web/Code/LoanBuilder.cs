namespace EzBob.Web.Code {
	using System;
	using System.Linq;
	using ConfigManager;
	using Ezbob.Logger;
	using EzBob.Models;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using PaymentServices.Calculators;
	using ServiceClientProxy;

	public class LoanBuilder {
		public LoanBuilder(ChangeLoanDetailsModelBuilder builder) {
			_builder = builder;
		} // constructor

		public Loan CreateLoan(CashRequest cr, decimal amount, DateTime now, int interestOnlyTerm = 0) {
			Loan l = string.IsNullOrEmpty(cr.LoanTemplate)
				? CreateNewLoan(cr, amount, now, cr.RepaymentPeriod, interestOnlyTerm)
				: CreateLoanFromTemplate(cr, amount, now);

			l.CustomerSelectedTerm = cr.RepaymentPeriod;

			return l;
		} // CreateLoan

		public Loan CreateNewLoan(CashRequest cr, decimal amount, DateTime now, int term, int interestOnlyTerm = 0) {
			return BuildLoan(cr, amount, now, term, interestOnlyTerm).Loan;
		} // CreateNewLoan

		public BuiltLoan BuildLoan(CashRequest cr, decimal amount, DateTime now, int term, int interestOnlyTerm = 0) {
			decimal setupFeePct = cr.ManualSetupFeePercent ?? 0;
			decimal brokerFeePct = cr.BrokerSetupFeePercent ?? 0;

			decimal approvedAmount = (decimal)(cr.ManagerApprovedSum ?? cr.SystemCalculatedSum ?? 0);

			if ((cr.Customer.Broker != null) && (approvedAmount != amount)) {
				log.Debug(
					"CreateNewLoan: broker customer '{0}', broker fee in cash request with approved amount {1} is {2}.",
					cr.Customer.Stringify(),
					approvedAmount.ToString("C2"),
					brokerFeePct.ToString("P2")
				);

				Loan firstLoan = cr.Customer.Loans.OrderBy(l => l.Date).FirstOrDefault();

				brokerFeePct = new CommissionCalculator(amount, firstLoan == null ? (DateTime?)null : firstLoan.Date)
					.Calculate()
					.BrokerCommission;

				log.Debug(
					"CreateNewLoan: broker customer '{0}', broker fee adjusted to loan amount {1} is {2}.",
					cr.Customer.Stringify(),
					amount.ToString("C2"),
					brokerFeePct.ToString("P2")
				);
			} // if broker customer

			var fees = new SetupFeeCalculator(setupFeePct, brokerFeePct).Calculate(amount);

			decimal setupFee = fees.Total;
			decimal brokerFee = fees.Broker;

			var calculator = new LoanScheduleCalculator { Interest = cr.InterestRate, Term = term };

			LoanLegal loanLegal = cr.LoanLegals.LastOrDefault();

			var loan = new Loan {
				LoanAmount = amount,
				Date = now,
				LoanType = cr.LoanType,
				CashRequest = cr,
				SetupFee = setupFee,
				LoanLegalId = loanLegal == null ? (int?)null : loanLegal.Id
			};

			calculator.Calculate(amount, loan, loan.Date, interestOnlyTerm, cr.SpreadSetupFee());

			loan.LoanSource = cr.LoanSource;

			if (brokerFee > 0 && cr.Customer.Broker != null) {
				loan.BrokerCommissions.Add(new LoanBrokerCommission {
					Broker = cr.Customer.Broker,
					CardInfo = cr.Customer.Broker.BankAccounts.FirstOrDefault(
						x => x.IsDefault.HasValue && x.IsDefault.Value
					),
					CommissionAmount = brokerFee,
					CreateDate = now,
					Loan = loan,
				});
			} // if broker fee & broker

			return new BuiltLoan {
				Loan = loan,
				BrokerFeePercent = brokerFeePct,
				ManualSetupFeePercent = setupFeePct,
			};
		} // BuildLoan

		/// <summary>
		/// Looks like this method is not used any more (after removing edit loan button from underwriter's
		/// approval dialogue. The button was located in _ProfileTemplateMain.cshtml.
		/// </summary>
		/// <param name="cr"></param>
		/// <param name="amount"></param>
		/// <param name="now"></param>
		/// <returns></returns>
		private Loan CreateLoanFromTemplate(CashRequest cr, decimal amount, DateTime now) {
			var model = EditLoanDetailsModel.Parse(cr.LoanTemplate);
			var loan = _builder.CreateLoan(model);
			loan.LoanType = cr.LoanType;
			loan.CashRequest = cr;

			AdjustDates(now, loan);
			AdjustBalances(amount, loan);

			var c = new LoanRepaymentScheduleCalculator(loan, now, CurrentValues.Instance.AmountToChargeFrom);
			c.GetState();

			if (loan.Id > 0) {
				try {
					ServiceClient serviceClient = new ServiceClient();
					long nlLoanId = serviceClient.Instance.GetLoanByOldID(loan.Id, cr.Customer.Id, 1)
						.Value;
					if (nlLoanId > 0) {
						var nlModel = serviceClient.Instance.GetLoanState(loan.Customer.Id, nlLoanId, DateTime.UtcNow, 1, true).Value;
						log.Msg("<<< NL_Compare: {0}\n===============loan: {1}  >>>", nlModel, loan);
					}
					// ReSharper disable once CatchAllClause
				} catch (Exception ex) {
					log.Msg("<<< NL_Compare fail at: {0}, err: {1}", Environment.StackTrace, ex.Message);
				} // try
			} // if

			loan.LoanSource = cr.LoanSource;
			return loan;
		} // CreateLoanFromTemplate

		private void AdjustBalances(decimal amount, Loan loan) {
			if (!_builder.IsAmountChangingAllowed(loan.CashRequest))
				return;

			var balances = loan.LoanType.GetBalances(amount, loan.Schedule.Count).ToArray();

			for (int i = 0; i < loan.Schedule.Count; i++)
				loan.Schedule[i].Balance = balances[i];

			loan.LoanAmount = amount;
		} // AdjustBalances

		private static void AdjustDates(DateTime now, Loan loan) {
			var diff = now.Subtract(loan.Date);
			loan.Date = now;

			foreach (var item in loan.Schedule)
				item.Date = item.Date.AddDays(diff.TotalDays);

			foreach (PaymentRollover rollover in loan.Schedule.SelectMany(s => s.Rollovers)) {
				rollover.Created = rollover.Created.AddDays(diff.TotalDays);
				rollover.ExpiryDate.Value.AddDays(diff.TotalDays);
			} // for

			foreach (var item in loan.Charges)
				item.Date = item.Date.AddDays(diff.TotalDays);
		} // AdjustDates

		private readonly ChangeLoanDetailsModelBuilder _builder;
		private static readonly ASafeLog log = new SafeILog(typeof(LoanBuilder));
	} // class LoanBuilder
} // namespace