namespace EzBob.Web.Code {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.Linq;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Utils.Extensions;
	using EzBob.Backend.Models;
	using EzBob.Models;
	using EzBob.Web.Infrastructure;
	using EzBob.Web.Models;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using EZBob.DatabaseLib.Repository;
	using PaymentServices.Calculators;
	using ServiceClientProxy;
	using StructureMap;

	public class AgreementsModelBuilder {
		private readonly APRCalculator _aprCalc;
	    private readonly RepaymentCalculator _repaymentCalculator;
	    protected readonly ServiceClient serviceClient;
        private readonly IEzbobWorkplaceContext _context;


		public AgreementsModelBuilder(IEzbobWorkplaceContext context = null) {
			_aprCalc = new APRCalculator();

			serviceClient = new ServiceClient();
			_repaymentCalculator = new RepaymentCalculator();
			_context = context;
		}

		/// <summary>
		/// Either customer.LastCashRequest or Loan should be available
		/// </summary>
		/// <param name="customer"></param>
		/// <param name="amount"></param>
		/// <param name="loan"></param>
		/// <returns></returns>
		public virtual AgreementModel Build(Customer customer, decimal amount, Loan loan) {
			var now = DateTime.UtcNow;

			if (customer.LastCashRequest == null && loan == null) throw new ArgumentException("LastCashRequest or Loan is required");
			
			var apr = _aprCalc.Calculate(amount, loan.Schedule, loan.SetupFee, loan.Date);
			return GenerateAgreementModel(customer, loan, now, apr);
		}

		public virtual AgreementModel ReBuild(Customer customer, Loan loan){
			return GenerateAgreementModel(customer, loan, loan.Date, (double)loan.APR);
		}

		private AgreementModel GenerateAgreementModel(Customer customer, Loan loan, DateTime now, double apr)
		{
			var model = new AgreementModel();
			model.Schedule = loan.Schedule.Select(LoanScheduleExtention.FromLoanScheduleItem).ToList();
			model.CustomerEmail = customer.Name;
			model.FullName = customer.PersonalInfo.Fullname;
			model.TypeOfBusinessName = customer.PersonalInfo.TypeOfBusinessName;

			var businessType = customer.PersonalInfo.TypeOfBusiness;
			var company = customer.Company;
			CustomerAddress companyAddress = null;
			if (company != null){
				switch (businessType.Reduce()){
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
			
			model.SetupFeeAmount = FormattingUtils.NumericFormats((int)CurrentValues.Instance.SetupFeeFixed);
			model.SetupFeePercent = CurrentValues.Instance.SetupFeePercent;

            //According to new logic the setup fee is always percent and min setup fee is amount SetupFeeFixed
            if((loan.CashRequest.ManualSetupFeePercent.HasValue && loan.CashRequest.ManualSetupFeePercent.Value > 0) || 
               (loan.CashRequest.BrokerSetupFeePercent.HasValue && loan.CashRequest.BrokerSetupFeePercent.Value > 0)) {
                decimal setupFeePercent = (loan.CashRequest.ManualSetupFeePercent ?? 0M) +  (loan.CashRequest.BrokerSetupFeePercent ?? 0M);
                model.SetupFeePercent = (setupFeePercent * 100).ToString(CultureInfo.InvariantCulture);
            }
            model.IsBrokerFee = false;
            model.IsManualSetupFee = false;
			
            model.APR = apr;

			var start = loan.Schedule.First().Date.AddMonths(-1);
			var end = loan.Schedule.Last().Date;
			var days = (end - start).TotalDays;

			

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
            model.Term = _repaymentCalculator.CalculateCountRepayment(loan);

			model.TotalPrincipalWithSetupFee = FormattingUtils.NumericFormats(loan.Schedule.Sum(a => a.LoanRepayment) - loan.SetupFee);

            if (customer.CustomerOrigin.Name == CustomerOriginEnum.everline.ToString()) {
                CreateEverlineRefinance(model, customer.Name);
            }
			return model;
		}

        /// <summary>
        /// TODO This method should be removed after refinansing of everline customers will be complete
        /// executes everline api each time the agreement is generated
        /// </summary>
	    private void CreateEverlineRefinance(AgreementModel model, string email) {
            try {
                EverlineLoginLoanChecker checker = new EverlineLoginLoanChecker();
                var details = checker.GetLoanDetails(email);
                var evlOpenLoan = details.LoanApplications.FirstOrDefault(x => !x.ClosedOn.HasValue && x.BalanceDetails.TotalOutstandingBalance.HasValue && x.BalanceDetails.TotalOutstandingBalance > 0);
                if (evlOpenLoan != null) {
                    model.IsEverlineRefinanceLoan = true;
                    model.EverlineRefinanceLoanRef = evlOpenLoan.LoanId.ToString();
                    model.EverlineRefinanceLoanDate = FormattingUtils.FormatDateToString(evlOpenLoan.FundedOn);
                    model.EverlineRefinanceLoanOutstandingAmount = FormattingUtils.NumericFormats(evlOpenLoan.BalanceDetails.TotalOutstandingBalance.Value);
                }
            } catch {
                //failed to build everline refinance 
            }
        }

        private IList<FormattedSchedule> CreateSchedule(IEnumerable<LoanScheduleItem> schedule) {
            return schedule.Select((installment, i) => new FormattedSchedule {
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

		/// <exception cref="ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
		/// <exception cref="OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
		/// <exception cref="NullReferenceException"><paramref name="name" /> is null. </exception>
		public void CalculateTotal(decimal fee, List<LoanScheduleItem> schedule, AgreementModel model) {
			model.TotalAmount = FormattingUtils.NumericFormats(schedule.Sum(a => a.AmountDue));
			model.TotalPrincipal = FormattingUtils.NumericFormats(schedule.Sum(a => a.LoanRepayment));
			model.TotalInterest = FormattingUtils.NumericFormats(schedule.Sum(a => a.Interest));
			model.TotalAmoutOfCredit = FormattingUtils.NumericFormats(schedule.Sum(a => a.LoanRepayment));
			model.TotalFees = FormattingUtils.NumericFormats(fee);

			//var currencyRateRepository = ObjectFactory.GetInstance<CurrencyRateRepository>();
			//decimal currencyRate = (decimal)currencyRateRepository.GetCurrencyHistoricalRate(DateTime.UtcNow, "USD");
			decimal currencyRate = GetUSDCurrencyRate();
			model.TotalPrincipalUsd = "$ " + (CurrentValues.Instance.AlibabaCurrencyConversionCoefficient*currencyRate*
			                           schedule.Sum(a => a.LoanRepayment)).ToString("N", CultureInfo.CreateSpecificCulture("en-gb"));
		}

		/// <exception cref="OverflowException">The number of elements in is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
		public AgreementModel NL_BuildAgreementModel(Customer customer, NL_Model nlModel) {
			
			var history = nlModel.Loan.LastHistory();

			var result = this.serviceClient.Instance.CalculateLoanSchedule(this._context != null ? this._context.UserId : customer.Id, nlModel.CustomerID, nlModel).Value;

			// faile to create Schedule
			if (history.Schedule.Count == 0) {
				// TODO log error - result.Error
				return null;
			}

			var model = new AgreementModel() {
				Schedule = new List<LoanScheduleItemModel>()
			};

			// fill in AgreementModel schedules list
			foreach (NL_LoanSchedules s in history.Schedule) {
				var item = new LoanScheduleItemModel {
					Id = s.LoanScheduleID,
					AmountDue = s.AmountDue, 
					Date = s.PlannedDate,
					Interest = s.Interest, 
					Status = Enum.GetName(typeof(NLScheduleStatuses), s.LoanScheduleStatusID).DescriptionAttr(),
					LoanRepayment = s.Principal,
					Balance = s.Balance,
					Fees = s.FeesAmount, 
					InterestRate = s.InterestRate
				};
				item.StatusDescription = item.Status;
				model.Schedule.Add(item);
			}

			model.CustomerEmail = customer.Name;
			model.FullName = customer.PersonalInfo.Fullname;
			model.TypeOfBusinessName = customer.PersonalInfo.TypeOfBusinessName;

			var businessType = customer.PersonalInfo.TypeOfBusiness;
			var company = customer.Company;
			CustomerAddress companyAddress = null;
			if (company != null) {
				switch (businessType.Reduce()) {
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

			// collect all setup/servicing "spreaded" fees
			List<NL_LoanFees> loanSetupFees = nlModel.Loan.Fees.Where(f => f.LoanFeeTypeID == (int)NLFeeTypes.SetupFee || f.LoanFeeTypeID == (int)NLFeeTypes.ServicingFee).ToList();
			// get fees sum
			decimal totalFees = 0m;
			loanSetupFees.ForEach(f => totalFees += f.Amount);

			// formatted totals
			model.TotalAmount = FormattingUtils.NumericFormats(model.Schedule.Sum(a => a.AmountDue));
			model.TotalPrincipal = FormattingUtils.NumericFormats(model.Schedule.Sum(a => a.LoanRepayment));
			model.TotalInterest = FormattingUtils.NumericFormats(model.Schedule.Sum(a => a.Interest));
			model.TotalAmoutOfCredit = model.TotalPrincipal; //FormattingUtils.NumericFormats(model.Schedule.Sum(a => a.LoanRepayment));
			model.TotalFees = FormattingUtils.NumericFormats(totalFees);

			decimal currencyRate = GetUSDCurrencyRate();
			////TODO - check culture info 
			model.TotalPrincipalUsd = "$ " + (CurrentValues.Instance.AlibabaCurrencyConversionCoefficient * currencyRate * model.Schedule.Sum(a => a.LoanRepayment)).ToString("N", CultureInfo.GetCultureInfo("en-USD"));

			model.CurentDate = FormattingUtils.FormatDateTimeToString(DateTime.UtcNow);
			model.CurrentDate = DateTime.UtcNow;

			model.FormattedSchedules = model.Schedule.Select((installment, i) => new FormattedSchedule {
				AmountDue = FormattingUtils.NumericFormats(installment.AmountDue),
				Principal = FormattingUtils.NumericFormats(installment.LoanRepayment),
				Interest = FormattingUtils.NumericFormats(installment.Interest),
				Fees = FormattingUtils.NumericFormats(installment.Fees),
				Date = FormattingUtils.FormatDateToString(installment.Date),
				StringNumber = FormattingUtils.ConvertingNumberToWords(i + 1),
				InterestRate = string.Format("{0:0.00}", installment.InterestRate * 100),
				Iterration = i + 1,
			}).ToList();

			// TODO update from history?
			model.InterestRate = history.InterestRate * 100;
			model.SetupFee = FormattingUtils.NumericFormats(totalFees);

			model.SetupFeeAmount = FormattingUtils.NumericFormats((int)CurrentValues.Instance.SetupFeeFixed);
			model.SetupFeePercent = CurrentValues.Instance.SetupFeePercent;

			//// FEES TODO
			////According to new logic the setup fee is always percent and min setup fee is amount SetupFeeFixed  ????
			if ((totalFees > 0) || (result.Offer.BrokerSetupFeePercent.HasValue && result.Offer.BrokerSetupFeePercent.Value > 0)) {
				decimal setupFeePercent = totalFees + result.Offer.BrokerSetupFeePercent ?? 0M;
				model.SetupFeePercent = (setupFeePercent * 100).ToString(CultureInfo.InvariantCulture);
			}

			// TODO was is das?
			model.IsBrokerFee = false;
			model.IsManualSetupFee = false;

			model.APR = result.APR == null ? 0 : (double)result.APR;

			model.InterestRatePerDay = model.Schedule[1].InterestRate / 30; // For first month
			model.InterestRatePerDayFormatted = string.Format("{0:0.00}", model.InterestRatePerDay);
			model.InterestRatePerYearFormatted = string.Format("{0:0.00}", model.InterestRate * 12);

			model.LoanType = Enum.GetName(typeof(NLLoanTypes), result.Loan.LoanTypeID);
			model.TermOnlyInterest = model.Schedule.Count(s => s.LoanRepayment == 0);
			model.TermOnlyInterestWords = FormattingUtils.ConvertToWord(model.TermOnlyInterest).ToLower();
			model.TermInterestAndPrincipal = model.Schedule.Count(s => s.LoanRepayment != 0 && s.Interest != 0);
			model.TermInterestAndPrincipalWords = FormattingUtils.ConvertToWord(model.TermInterestAndPrincipal).ToLower();
			model.isHalwayLoan = Enum.GetName(typeof(NLLoanTypes), result.Loan.LoanTypeID) == NLLoanTypes.HalfWayLoanType.ToString();
			model.CountRepayment = model.Schedule.Count;
			model.Term = model.Schedule.Count;

			model.TotalPrincipalWithSetupFee = FormattingUtils.NumericFormats(model.Schedule.Sum(a => a.LoanRepayment) - totalFees);

			return model;
		}

		

		public decimal GetUSDCurrencyRate() {
			var currencyRateRepository = ObjectFactory.GetInstance<CurrencyRateRepository>();
			return (decimal)currencyRateRepository.GetCurrencyHistoricalRate(DateTime.UtcNow, "USD");
		}
	}
}