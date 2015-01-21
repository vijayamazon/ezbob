namespace EzBob.Web.Code
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using Backend.Models;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using EZBob.DatabaseLib.Repository;
	using EzBob.Models;
	using Ezbob.Backend.Models;
	using PaymentServices.Calculators;
	using StructureMap;

	public class AgreementsModelBuilder
	{
		private readonly APRCalculator _aprCalc;
		private readonly RepaymentCalculator _repaymentCalculator = new RepaymentCalculator();

		public AgreementsModelBuilder()
		{
			_aprCalc = new APRCalculator();
		}

		/// <summary>
		/// Either customer.LastCashRequest or Loan should be available
		/// </summary>
		/// <param name="customer"></param>
		/// <param name="amount"></param>
		/// <param name="loan"></param>
		/// <returns></returns>
		public virtual AgreementModel Build(Customer customer, decimal amount, Loan loan)
		{
			var now = DateTime.UtcNow;

			if (customer.LastCashRequest == null && loan == null) throw new ArgumentException("LastCashRequest or Loan is required");
			
			var apr = _aprCalc.Calculate(amount, loan.Schedule, loan.SetupFee, loan.Date);
			return GenerateAgreementModel(customer, loan, now, apr);
		}

		public virtual AgreementModel ReBuild(Customer customer, Loan loan)
		{
			return GenerateAgreementModel(customer, loan, loan.Date, (double)loan.APR);
		}

		private AgreementModel GenerateAgreementModel(Customer customer, Loan loan, DateTime now, double apr)
		{
			var model = new AgreementModel();
			model.Schedule = loan.Schedule.Select(LoanScheduleItemModel.FromLoanScheduleItem).ToList();
			model.CustomerEmail = customer.Name;
			model.FullName = customer.PersonalInfo.Fullname;
			model.TypeOfBusinessName = customer.PersonalInfo.TypeOfBusinessName;

			var businessType = customer.PersonalInfo.TypeOfBusiness;
			var company = customer.Company;
			CustomerAddress companyAddress = null;
			if (company != null)
			{
				switch (businessType.Reduce())
				{
					case TypeOfBusinessReduced.Limited:
						model.CompanyNumber = company.ExperianRefNum ?? company.CompanyNumber;
						goto case TypeOfBusinessReduced.NonLimited;
					case TypeOfBusinessReduced.NonLimited:
						model.CompanyName = company.ExperianCompanyName ?? company.CompanyName;
						companyAddress = company.ExperianCompanyAddress.LastOrDefault() ?? company.CompanyAddress.LastOrDefault();
						break;
				}
			}

			model.CompanyAdress = companyAddress.GetFormatted();
			model.PersonAddress = customer.AddressInfo.PersonalAddress.FirstOrDefault().GetFormatted();

			CalculateTotal(loan.SetupFee, loan.Schedule.ToList(), model);

			model.CurentDate = FormattingUtils.FormatDateTimeToString(now);
			model.CurrentDate = now;

			model.FormattedSchedules = CreateSchedule(loan.Schedule.ToList()).ToList();

			model.InterestRate = loan.InterestRate * 100;
			model.SetupFee = FormattingUtils.NumericFormats(loan.SetupFee);
			model.IsBrokerFee = loan.CashRequest.UseBrokerSetupFee;
			model.SetupFeeAmount = FormattingUtils.NumericFormats((int)CurrentValues.Instance.SetupFeeFixed);
			model.SetupFeePercent = CurrentValues.Instance.SetupFeePercent;
			bool isManualSetupFee;
			model.ManualSetupFee = SetupFeeText(loan.CashRequest.ManualSetupFeeAmount, loan.CashRequest.ManualSetupFeePercent, out isManualSetupFee);
			model.IsManualSetupFee = isManualSetupFee;
			model.APR = apr;

			var start = loan.Schedule.First().Date.AddMonths(-1);
			var end = loan.Schedule.Last().Date;
			var days = (end - start).TotalDays;

			model.Term = _repaymentCalculator.ReCalculateRepaymentPeriod(loan.CashRequest);

			//model.InterestRatePerDay = (model.Schedule.Count * model.InterestRate) / (decimal)days;
			model.InterestRatePerDay = model.Schedule[1].InterestRate / 30; // For first month
			model.InterestRatePerDayFormatted = string.Format("{0:0.00}", model.InterestRatePerDay);
			model.InterestRatePerYearFormatted = string.Format("{0:0.00}", model.InterestRate * 12);

			var loanType = customer.LastCashRequest.LoanType ?? new StandardLoanType();

			model.LoanType = loanType.Type;
			model.TermOnlyInterest = loan.Schedule.Count(s => s.LoanRepayment == 0);
			model.TermOnlyInterestWords = FormattingUtils.ConvertToWord(model.TermOnlyInterest).ToLower();
			model.TermInterestAndPrincipal = loan.Schedule.Count(s => s.LoanRepayment != 0 && s.Interest != 0);
			model.TermInterestAndPrincipalWords = FormattingUtils.ConvertToWord(model.TermInterestAndPrincipal).ToLower();
			model.isHalwayLoan = loanType.IsHalwayLoan;
			model.CountRepayment = _repaymentCalculator.CalculateCountRepayment(loan);

			model.TotalPrincipalWithSetupFee = FormattingUtils.NumericFormats(loan.Schedule.Sum(a => a.LoanRepayment) - loan.SetupFee);
			return model;
		}

		private string SetupFeeText(int? amount, decimal? percent, out bool isManualSetupFee)
		{
			isManualSetupFee = true;
			if (amount.HasValue && amount > 0 && percent.HasValue && percent > 0)
			{
				return string.Format("{1}% of the loan amount (but in no case less than {0})", FormattingUtils.NumericFormats(amount.Value), percent.Value*100);
			}

			if (amount.HasValue && amount > 0)
			{
				return string.Format("{0}", FormattingUtils.NumericFormats(amount.Value));
			}

			if (percent.HasValue && percent > 0)
			{
				return string.Format("{0}% of the loan amount", percent.Value*100);
			}
			isManualSetupFee = false;
			return null;
		}

		private IList<FormattedSchedule> CreateSchedule(IEnumerable<LoanScheduleItem> schedule)
		{
			return schedule.Select((installment, i) => new FormattedSchedule
			{
				AmountDue = FormattingUtils.NumericFormats(installment.AmountDue),
				Principal = FormattingUtils.NumericFormats(installment.LoanRepayment),
				Interest = FormattingUtils.NumericFormats(installment.Interest),
				Fees = "0",
				Date = FormattingUtils.FormatDateToString(installment.Date),
				StringNumber = FormattingUtils.ConvertingNumberToWords(i + 1),
				InterestRate = string.Format("{0:0.00}", installment.InterestRate * 100),
				Iterration = i + 1,
			}).ToList();
		}

		public void CalculateTotal(decimal fee, List<LoanScheduleItem> schedule, AgreementModel model)
		{
			model.TotalAmount = FormattingUtils.NumericFormats(schedule.Sum(a => a.AmountDue));
			model.TotalPrincipal = FormattingUtils.NumericFormats(schedule.Sum(a => a.LoanRepayment));
			model.TotalInterest = FormattingUtils.NumericFormats(schedule.Sum(a => a.Interest));
			model.TotalAmoutOfCredit = FormattingUtils.NumericFormats(schedule.Sum(a => a.LoanRepayment));
			model.TotalFees = FormattingUtils.NumericFormats(fee);

			var currencyRateRepository = ObjectFactory.GetInstance<CurrencyRateRepository>();
			decimal currencyRate = (decimal)currencyRateRepository.GetCurrencyHistoricalRate(DateTime.UtcNow, "USD");
			model.TotalPrincipalUsd = "$ " +
			                          (CurrentValues.Instance.AlibabaCurrencyConversionCoefficient*currencyRate*
			                           schedule.Sum(a => a.LoanRepayment)).ToString("N", CultureInfo.CreateSpecificCulture("en-gb"));
		}
	}
}