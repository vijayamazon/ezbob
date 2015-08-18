﻿namespace EzBob.Web.Code {
	using System;
	using System.Collections.Generic;
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

        private IList<FormattedSchedule> NL_CreateSchedule(IEnumerable<LoanScheduleItemModel> schedule) {
            return schedule.Select((installment, i) => new FormattedSchedule {
                AmountDue = FormattingUtils.NumericFormats(installment.AmountDue),
                Principal = FormattingUtils.NumericFormats(installment.LoanRepayment),
                Interest = FormattingUtils.NumericFormats(installment.Interest),
                Fees = FormattingUtils.NumericFormats(installment.Fees),
                Date = FormattingUtils.FormatDateToString(installment.Date),
                StringNumber = FormattingUtils.ConvertingNumberToWords(i + 1),
                InterestRate = string.Format("{0:0.00}", installment.InterestRate * 100),
                Iterration = i + 1,
            }).ToList();
        }

		public void CalculateTotal(decimal fee, List<LoanScheduleItem> schedule, AgreementModel model) {
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

        public void NL_CalculateTotal(decimal fee, AgreementModel model) {
            model.TotalAmount = FormattingUtils.NumericFormats(model.Schedule.Sum(a => a.AmountDue));
            model.TotalPrincipal = FormattingUtils.NumericFormats(model.Schedule.Sum(a => a.LoanRepayment));
            model.TotalInterest = FormattingUtils.NumericFormats(model.Schedule.Sum(a => a.Interest));
            model.TotalAmoutOfCredit = FormattingUtils.NumericFormats(model.Schedule.Sum(a => a.LoanRepayment));
            model.TotalFees = FormattingUtils.NumericFormats(fee);

            // TODO - to SP
            var currencyRateRepository = ObjectFactory.GetInstance<CurrencyRateRepository>();
            decimal currencyRate = (decimal)currencyRateRepository.GetCurrencyHistoricalRate(DateTime.UtcNow, "USD");
            //TODO - check culture info 
            model.TotalPrincipalUsd = "$ " +
                                      (CurrentValues.Instance.AlibabaCurrencyConversionCoefficient * currencyRate *
                                       model.Schedule.Sum(a => a.LoanRepayment)).ToString("N", CultureInfo.CreateSpecificCulture("en-gb"));
        }


        public AgreementModel NL_BuildAgreementModel(Customer customer, NL_Model nlModel) {
            var model = new AgreementModel();
			var result = this.serviceClient.Instance.CalculateLoanSchedule(this._context != null ? this._context.UserId : customer.Id, nlModel.CustomerID, nlModel).Value;

			// TODO REPLACE BY full Schedule data: p, i, p*i, f, amountDue; paid-p, paid-i, paid-f, 
			foreach (var s in result.Schedule) {
				var fee = result.Fees.FirstOrDefault(f => f.Fee.AssignTime == s.ScheduleItem.PlannedDate);
				var item=new LoanScheduleItemModel {
					Id = s.ScheduleItem.LoanScheduleID,
					AmountDue = s.ScheduleItem.Principal + s.ScheduleItem.InterestRate * s.ScheduleItem.Principal + (fee == null ? 0 : fee.Fee.Amount), // P+I=(r*P)+F
					Date = s.ScheduleItem.PlannedDate,
					Interest = s.ScheduleItem.InterestRate * s.ScheduleItem.Principal,  // I=(r*P)
					Status = Enum.GetName(typeof(NLScheduleStatuses), s.ScheduleItem.LoanScheduleStatusID),
					StatusDescription = Enum.Parse(typeof(NLScheduleStatuses), Enum.GetName(typeof(NLScheduleStatuses), s.ScheduleItem.LoanScheduleStatusID).ToString()).DescriptionAttr(),
					LoanRepayment = s.ScheduleItem.Principal, //P
					//Balance = nlModel.InitialAmount - s.ScheduleItem.Principal * (s.ScheduleItem.Position - 1),
					Fees = fee == null ? 0 : fee.Fee.Amount, //F
					InterestRate = s.ScheduleItem.InterestRate //r
				};
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

			// TODO see original implementation
			decimal setupFeeAmount = nlModel.Fees.Where(f => f.Fee.LoanFeeTypeID == (int)FeeTypes.SetupFee).Sum(f => f.Fee.Amount);
			NL_CalculateTotal(setupFeeAmount, model); //loanSetupFee, model);

			model.CurentDate = FormattingUtils.FormatDateTimeToString(DateTime.UtcNow);
			model.CurrentDate = DateTime.UtcNow;

			model.FormattedSchedules = NL_CreateSchedule(model.Schedule).ToList();

			// TODO update from history?
			//model.InterestRate = nlModel.InitialInterestRate * 100;
			model.SetupFee = FormattingUtils.NumericFormats(setupFeeAmount); //loanSetupFee);

			model.SetupFeeAmount = FormattingUtils.NumericFormats((int)CurrentValues.Instance.SetupFeeFixed);
			model.SetupFeePercent = CurrentValues.Instance.SetupFeePercent;

			// FEES TODO
			//According to new logic the setup fee is always percent and min setup fee is amount SetupFeeFixed
			/*if ((result.Offer.SetupFeePercent.HasValue && result.Offer.SetupFeePercent.Value > 0) ||
			   (result.Offer.BrokerSetupFeePercent.HasValue && result.Offer.BrokerSetupFeePercent.Value > 0)) {
				decimal setupFeePercent = (result.Offer.SetupFeePercent ?? 0M) + (result.Offer.BrokerSetupFeePercent ?? 0M);
				model.SetupFeePercent = (setupFeePercent * 100).ToString(CultureInfo.InvariantCulture);
			}*/

			model.IsBrokerFee = false;
			model.IsManualSetupFee = false;

			model.APR = result.APR == null ? 0 : (double)result.APR;

			var start = model.Schedule.First().Date.AddMonths(-1);
			var end = model.Schedule.Last().Date;
			var days = (end - start).TotalDays;

			model.InterestRatePerDay = model.Schedule[1].InterestRate / 30; // For first month
			model.InterestRatePerDayFormatted = string.Format("{0:0.00}", model.InterestRatePerDay);
			model.InterestRatePerYearFormatted = string.Format("{0:0.00}", model.InterestRate * 12);

			model.LoanType = Enum.GetName(typeof(NLLoanTypes), result.Loan.LoanTypeID);
			model.TermOnlyInterest = model.Schedule.Count(s => s.LoanRepayment == 0);
			model.TermOnlyInterestWords = FormattingUtils.ConvertToWord(model.TermOnlyInterest).ToLower();
			model.TermInterestAndPrincipal = model.Schedule.Count(s => s.LoanRepayment != 0 && s.Interest != 0);
			model.TermInterestAndPrincipalWords = FormattingUtils.ConvertToWord(model.TermInterestAndPrincipal).ToLower();
			model.isHalwayLoan = Enum.GetName(typeof(NLLoanTypes), result.Loan.LoanTypeID) == NLLoanTypes.HalfWayLoanType.ToString() ? true : false;
			model.CountRepayment = model.Schedule.Count;
			model.Term = model.Schedule.Count;

			model.TotalPrincipalWithSetupFee = FormattingUtils.NumericFormats(model.Schedule.Sum(a => a.LoanRepayment) - setupFeeAmount);

			if (customer.CustomerOrigin.Name == CustomerOriginEnum.everline.ToString()) {
				CreateEverlineRefinance(model, customer.Name);
			}

            return model;
        }
	}
}