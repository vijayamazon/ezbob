using System;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EzBob.Models;
using PaymentServices.Calculators;

namespace EzBob.Web.Code
{
	using ConfigManager;

	public class LoanBuilder
	{
		private readonly ChangeLoanDetailsModelBuilder _builder;

		public LoanBuilder(ChangeLoanDetailsModelBuilder builder)
		{
			_builder = builder;
		}

		public Loan CreateLoan(CashRequest cr, decimal amount, DateTime now, int interestOnlyTerm = 0) {
			var l = string.IsNullOrEmpty(cr.LoanTemplate)
				? CreateNewLoan(cr, amount, now, cr.RepaymentPeriod, interestOnlyTerm)
				: CreateLoanFromTemplate(cr, amount, now);

			l.CustomerSelectedTerm = cr.RepaymentPeriod;

			return l;
		} // CreateLoan

		public Loan CreateNewLoan(CashRequest cr, decimal amount, DateTime now, int term, int interestOnlyTerm = 0)
		{
			var sfc = new SetupFeeCalculator(cr.UseSetupFee, cr.UseBrokerSetupFee, cr.ManualSetupFeeAmount, cr.ManualSetupFeePercent);
			var setupFee = sfc.Calculate(amount);

			var calculator = new LoanScheduleCalculator { Interest = cr.InterestRate, Term = term };
			var loanLegal = cr.LoanLegals.LastOrDefault();
			var loan = new Loan
				{
					LoanAmount = amount,
					Date = now,
					LoanType = cr.LoanType,
					CashRequest = cr,
					SetupFee = setupFee,
					LoanLegalId = loanLegal == null ? (int?)null : loanLegal.Id
				};
			calculator.Calculate(amount, loan, loan.Date, interestOnlyTerm);
			loan.LoanSource = cr.LoanSource;
			return loan;
		}

		private Loan CreateLoanFromTemplate(CashRequest cr, decimal amount, DateTime now)
		{
			var model = EditLoanDetailsModel.Parse(cr.LoanTemplate);
			var loan = _builder.CreateLoan(model);
			loan.LoanType = cr.LoanType;
			loan.CashRequest = cr;

			AdjustDates(now, loan);
			AdjustBalances(amount, loan);

			var c = new LoanRepaymentScheduleCalculator(loan, now, CurrentValues.Instance.AmountToChargeFrom);
			c.GetState();
			loan.LoanSource = cr.LoanSource;
			return loan;
		}

		private void AdjustBalances(decimal amount, Loan loan)
		{
			if (!_builder.IsAmountChangingAllowed(loan.CashRequest)) return;

			var balances = loan.LoanType.GetBalances(amount, loan.Schedule.Count).ToArray();
			for (int i = 0; i < loan.Schedule.Count; i++)
			{
				loan.Schedule[i].Balance = balances[i];
			}

			loan.LoanAmount = amount;
		}

		private static void AdjustDates(DateTime now, Loan loan)
		{
			var diff = now.Subtract(loan.Date);
			loan.Date = now;

			foreach (var item in loan.Schedule)
			{
				item.Date = item.Date.AddDays(diff.TotalDays);
			}

			foreach (var rollover in loan.Schedule.SelectMany(s => s.Rollovers))
			{
				rollover.Created = rollover.Created.AddDays(diff.TotalDays);
				rollover.ExpiryDate.Value.AddDays(diff.TotalDays);
			}

			foreach (var item in loan.Charges)
			{
				item.Date = item.Date.AddDays(diff.TotalDays);
			}
		}
	}
}