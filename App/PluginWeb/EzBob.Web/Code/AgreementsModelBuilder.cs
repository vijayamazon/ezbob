using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Model;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Models;
using EzBob.Web.Areas.Customer.Models;
using PaymentServices.Calculators;

namespace EzBob.Web.Code.Agreements
{
	using EZBob.DatabaseLib.Model;
	using StructureMap;

	public class AgreementsModelBuilder
	{
		private readonly CustomerModelBuilder _customerModelBuilder;
		private readonly APRCalculator _aprCalc;
		private readonly RepaymentCalculator _repaymentCalculator = new RepaymentCalculator();

		public AgreementsModelBuilder(CustomerModelBuilder customerModelBuilder)
		{
			_aprCalc = new APRCalculator();
			_customerModelBuilder = customerModelBuilder;
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
			model.Customer = _customerModelBuilder.BuildWizardModel(customer);
			model.TypeOfBusinessName = model.Customer.CustomerPersonalInfo.TypeOfBusinessName;

			var businessType = model.Customer.CustomerPersonalInfo.TypeOfBusiness;
			var company = customer.Company;
			if (businessType.Reduce() != TypeOfBusinessReduced.Limited && company != null)
			{
				model.CompanyName = company.ExperianCompanyName ?? company.CompanyName;
				model.CompanyNumber = company.ExperianRefNum ?? company.CompanyNumber;
				model.Address = company.ExperianCompanyAddress.LastOrDefault() ?? company.CompanyAddress.LastOrDefault();
			}
			else if (businessType.Reduce() == TypeOfBusinessReduced.NonLimited && company != null)
			{
				model.CompanyName = company.ExperianCompanyName ?? company.CompanyName;
				model.Address = company.ExperianCompanyAddress.LastOrDefault() ?? company.CompanyAddress.LastOrDefault();
			}

			model.CustomerAddress = model.Customer.PersonalAddress.FirstOrDefault();

			model.CompanyAdress = model.Address.GetFormatted();
			model.PersonAddress = model.CustomerAddress.GetFormatted();

			CalculateTotal(loan.SetupFee, loan.Schedule.ToList(), model);

			model.CurentDate = FormattingUtils.FormatDateTimeToString(now);
			model.CurrentDate = now;

			model.FormattedSchedules = CreateSchedule(loan.Schedule.ToList());

			model.InterestRate = loan.InterestRate * 100;
			model.SetupFee = FormattingUtils.NumericFormats(loan.SetupFee);
			model.IsBrokerFee = loan.CashRequest.UseBrokerSetupFee;
			var configVariables = ObjectFactory.TryGetInstance<IConfigurationVariablesRepository>();
			model.SetupFeeAmount = FormattingUtils.NumericFormats(configVariables.GetByNameAsInt("SetupFeeFixed"));
			model.SetupFeePercent = configVariables.GetByName("SetupFeePercent").Value;
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
			if (amount.HasValue && percent.HasValue)
			{
				return string.Format("{1}% of the loan amount (but in no case less than {0})", FormattingUtils.NumericFormats(amount.Value), percent.Value);
			}

			if (amount.HasValue)
			{
				return string.Format("{0}", FormattingUtils.NumericFormats(amount.Value));
			}

			if (percent.HasValue)
			{
				return string.Format("{0}% of the loan amount", percent.Value);
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
				InterestRate = string.Format("{0:0.0}", installment.InterestRate * 100),
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
		}
	}
}