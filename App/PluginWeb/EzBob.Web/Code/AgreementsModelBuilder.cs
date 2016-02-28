namespace EzBob.Web.Code {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Logger;
	using Ezbob.Utils.Extensions;
	using EzBob.Backend.Models;
	using EzBob.Models;
	using EzBob.Web.Infrastructure;
	using EzBob.Web.Models;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using EZBob.DatabaseLib.Repository;
	using log4net;
	using PaymentServices.Calculators;
	using ServiceClientProxy;
	using StructureMap;

	public class AgreementsModelBuilder {
		public AgreementsModelBuilder(ILoanLegalRepository llrepo, IEzbobWorkplaceContext context = null) {
			this.loanLegalRepo = llrepo;
			this.aprCalc = new APRCalculator();

			this.serviceClient = new ServiceClient();
			this.repaymentCalculator = new RepaymentCalculator();
			this.context = context;

			this.log = new SafeILog(LogManager.GetLogger(GetType()));
		} // constructor

		/// <summary>
		/// Either customer.LastCashRequest or Loan should be available
		/// </summary>
		/// <param name="customer"></param>
		/// <param name="amount"></param>
		/// <param name="loan"></param>
		/// <returns></returns>
		public virtual AgreementModel Build(Customer customer, decimal amount, Loan loan) {
			var now = DateTime.UtcNow;

			if (customer.LastCashRequest == null && loan == null)
				throw new ArgumentException("LastCashRequest or Loan is required");

			var apr = this.aprCalc.Calculate(amount, loan.Schedule, loan.SetupFee, loan.Date);
			return GenerateAgreementModel(customer, loan, now, apr);
		} // Build

		public virtual AgreementModel ReBuild(Customer customer, Loan loan) {
			return GenerateAgreementModel(customer, loan, loan.Date, (double)loan.APR);
		} // ReBuild

		private AgreementModel GenerateAgreementModel(Customer customer, Loan loan, DateTime now, double apr) {
			var model = new AgreementModel();
			model.Schedule = loan.Schedule.Select(LoanScheduleExtention.FromLoanScheduleItem).ToList();
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
					var experianCompanyAddress = company.ExperianCompanyAddress.LastOrDefault();
					if (experianCompanyAddress != null && !String.IsNullOrEmpty(experianCompanyAddress.Postcode)) {
						companyAddress = company.ExperianCompanyAddress.LastOrDefault();
					} else {
						companyAddress = company.CompanyAddress.LastOrDefault();
					}
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
			model.LatePaymentCharge = String.Format("{0:0.00}", Convert.ToInt32(ConfigManager.CurrentValues.Instance.LatePaymentCharge.Value));
			model.AdministrationCharge = String.Format("{0:0.00}", Convert.ToInt32(ConfigManager.CurrentValues.Instance.AdministrationCharge.Value)); 

			// According to new logic the setup fee is always percent and min setup fee is amount SetupFeeFixed

			decimal manualSetupFeePct;
			decimal brokerSetupFeePct;

			GetSetupFees(customer, loan, out manualSetupFeePct, out brokerSetupFeePct);

			bool shouldHaveSetupFee =
				(!loan.CashRequest.SpreadSetupFee.HasValue || !loan.CashRequest.SpreadSetupFee.Value) &&
				((manualSetupFeePct > 0) || (brokerSetupFeePct > 0));

			this.log.Debug(
				"Customer {0} agreement model: spread setup fee '{1}', manual {2}, broker {3}.",
				customer.Id,
				loan.CashRequest.SpreadSetupFee == null ? "null" : (loan.CashRequest.SpreadSetupFee.Value ? "spread" : "no"),
				manualSetupFeePct.ToString("P2"),
				brokerSetupFeePct.ToString("P2")
			);

			if (shouldHaveSetupFee) {
				decimal setupFeePercent = manualSetupFeePct + brokerSetupFeePct;

				model.SetupFeePercent = (setupFeePercent * 100).ToString(CultureInfo.InvariantCulture);
				model.SetupFeeAmount = FormattingUtils.NumericFormats((int)CurrentValues.Instance.SetupFeeFixed);
			} else {
				model.SetupFeePercent = "0";
				model.SetupFeeAmount = FormattingUtils.NumericFormats(0);
			} // if

			model.IsBrokerFee = false;
			model.IsManualSetupFee = false;

			model.APR = apr;

			// var start = loan.Schedule.First().Date.AddMonths(-1);
			// var end = loan.Schedule.Last().Date;
			// var days = (end - start).TotalDays;
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
			model.CountRepayment = this.repaymentCalculator.CalculateCountRepayment(loan);
			model.Term = this.repaymentCalculator.CalculateCountRepayment(loan);

			model.TotalPrincipalWithSetupFee = FormattingUtils.NumericFormats(loan.Schedule.Sum(a => a.LoanRepayment) - loan.SetupFee);
			return model;
		} // GenerateAgreementModel

		private void GetSetupFees(
			Customer customer,
			Loan loan,
			out decimal manualSetupFeePct,
			out decimal brokerSetupFeePct
		) {
			if (loan.LoanLegalId != null) {
				var loanLegal = this.loanLegalRepo.GetAll().FirstOrDefault(ll => ll.Id == loan.LoanLegalId.Value);

				if (loanLegal != null) {
					manualSetupFeePct = loanLegal.ManualSetupFeePercent ?? 0;
					brokerSetupFeePct = loanLegal.BrokerSetupFeePercent ?? 0;
					return;
				} // if
			} // if

			manualSetupFeePct = loan.CashRequest.ManualSetupFeePercent ?? 0;
			brokerSetupFeePct = loan.CashRequest.BrokerSetupFeePercent ?? 0;

			decimal approvedAmount =
				(decimal)(loan.CashRequest.ManagerApprovedSum ?? loan.CashRequest.SystemCalculatedSum ?? 0);

			if ((customer.Broker != null) && (approvedAmount != loan.LoanAmount)) {
				this.log.Debug(
					"GetSetupFees: broker customer '{0}', broker fee in cash request with approved amount {1} is {2}.",
					customer.Stringify(),
					approvedAmount.ToString("C2"),
					brokerSetupFeePct.ToString("P2")
				);

				Loan firstLoan = customer.Loans.OrderBy(l => l.Date).FirstOrDefault();

				brokerSetupFeePct = new CommissionCalculator(
					loan.LoanAmount,
					firstLoan == null ? (DateTime?)null : firstLoan.Date
				)
					.Calculate()
					.BrokerCommission;

				this.log.Debug(
					"CreateNewLoan: broker customer '{0}', broker fee adjusted to loan amount {1} is {2}.",
					customer.Stringify(),
					loan.LoanAmount.ToString("C2"),
					brokerSetupFeePct.ToString("P2")
				);
			} // if broker customer
		} // GetSetupFees

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
		} // CreateSchedule

		public void CalculateTotal(decimal fee, List<LoanScheduleItem> schedule, AgreementModel model) {
			model.TotalAmount = FormattingUtils.NumericFormats(schedule.Sum(a => a.AmountDue));
			model.TotalPrincipal = FormattingUtils.NumericFormats(schedule.Sum(a => a.LoanRepayment));
			model.TotalInterest = FormattingUtils.NumericFormats(schedule.Sum(a => a.Interest));
			model.TotalAmoutOfCredit = FormattingUtils.NumericFormats(schedule.Sum(a => a.LoanRepayment));
			model.TotalFees = FormattingUtils.NumericFormats(fee);

			//var currencyRateRepository = ObjectFactory.GetInstance<CurrencyRateRepository>();
			//decimal currencyRate = (decimal)currencyRateRepository.GetCurrencyHistoricalRate(DateTime.UtcNow, "USD");
			decimal currencyRate = GetUSDCurrencyRate();
			model.TotalPrincipalUsd = "$ " + (CurrentValues.Instance.AlibabaCurrencyConversionCoefficient * currencyRate *
									   schedule.Sum(a => a.LoanRepayment)).ToString("N", CultureInfo.CreateSpecificCulture("en-gb"));
		} // CalculateTotal

		/// <exception cref="OverflowException">The number of elements in is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
		/// <exception cref="NullReferenceException"><paramref /> is null. </exception>
		/// <exception cref="NoScheduleException">Condition. </exception>
		public AgreementModel NL_BuildAgreementModel(Customer customer, NL_Model nlModel) {
			// fill in loan+history with offer data
			nlModel = this.serviceClient.Instance.BuildLoanFromOffer(this.context != null ? this.context.UserId : customer.Id, nlModel.CustomerID, nlModel).Value;

			ALoanCalculator nlCalculator = null;
			// 2. get Schedule and Fees
			try {
				// init calculator
				nlCalculator = new LegacyLoanCalculator(nlModel);
				// model should contain Schedule and Fees after this invocation
				nlCalculator.CreateSchedule(); // create primary dates/p/r/f distribution of schedules (P/n) and setup/servicing fees. 7 September - fully completed schedule + fee + amounts due, without payments.
			} catch (NoInitialDataException noDataException) {
				this.log.Alert("CreateSchedule failed: {0}", noDataException.Message);
			} catch (InvalidInitialAmountException amountException) {
				this.log.Alert("CreateSchedule failed: {0}", amountException.Message);
			} catch (InvalidInitialInterestRateException interestRateException) {
				this.log.Alert("CreateSchedule failed: {0}", interestRateException.Message);
			} catch (InvalidInitialRepaymentCountException paymentsException) {
				this.log.Alert("CreateSchedule failed: {0}", paymentsException.Message);
			} catch (Exception ex) {
				this.log.Alert("Failed to create Schedule for customer {0}, err: {1}", nlModel.CustomerID, ex);
			} finally {
				if (nlCalculator == null) {
					this.log.Alert("failed to get nlCalculator for customer: {0}", nlModel.CustomerID);
				} // if
			} // try

			var history = nlModel.Loan.LastHistory();

			// no Schedule
			if (history.Schedule.Count == 0) {
				this.log.Alert("No Schedule. Customer: {0}", nlModel.CustomerID);
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
					Fees = s.Fees,
					InterestRate = s.InterestRate
				};
				item.StatusDescription = item.Status;
				model.Schedule.Add(item);
			} // for each

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
				} // switch
			} // if

			model.CompanyAdress = companyAddress.GetFormatted();
			model.PersonAddress = customer.AddressInfo.PersonalAddress.FirstOrDefault().GetFormatted();

			// collect all setup/servicing "spreaded" fees
			List<NL_LoanFees> loanSetupFees = nlModel.Loan.Fees.Where(f => f.LoanFeeTypeID == (int)NLFeeTypes.SetupFee || f.LoanFeeTypeID == (int)NLFeeTypes.ServicingFee).ToList();
			// get fees sum
			decimal totalFees = 0m;
			loanSetupFees.ForEach(f => totalFees += f.Amount);

			decimal totalPrincipal = model.Schedule.Sum(a => a.LoanRepayment);

			// formatted totals
			model.TotalAmount = FormattingUtils.NumericFormats(model.Schedule.Sum(a => a.AmountDue));
			model.TotalPrincipal = FormattingUtils.NumericFormats(totalPrincipal);
			model.TotalInterest = FormattingUtils.NumericFormats(model.Schedule.Sum(a => a.Interest));
			model.TotalAmoutOfCredit = model.TotalPrincipal; //FormattingUtils.NumericFormats(model.Schedule.Sum(a => a.LoanRepayment));
			model.TotalFees = FormattingUtils.NumericFormats(totalFees);

			decimal currencyRate = GetUSDCurrencyRate();
			model.TotalPrincipalUsd = "$ " + (CurrentValues.Instance.AlibabaCurrencyConversionCoefficient * currencyRate * totalPrincipal).ToString("N", CultureInfo.CreateSpecificCulture("en-gb"));

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
			if ((totalFees > 0) || (nlModel.Offer.BrokerSetupFeePercent.HasValue && nlModel.Offer.BrokerSetupFeePercent.Value > 0)) {
				decimal setupFeePercent = totalFees + nlModel.Offer.BrokerSetupFeePercent ?? 0M;
				model.SetupFeePercent = (setupFeePercent * 100).ToString(CultureInfo.InvariantCulture);
			} // if

			// TODO was is das?
			model.IsBrokerFee = false;
			model.IsManualSetupFee = false;

			if (nlCalculator != null)
				model.APR = nlCalculator.CalculateApr(history.EventTime);

			model.InterestRatePerDay = model.Schedule[1].InterestRate / 30; // For first month
			model.InterestRatePerDayFormatted = string.Format("{0:0.00}", model.InterestRatePerDay);
			model.InterestRatePerYearFormatted = string.Format("{0:0.00}", model.InterestRate * 12);

			model.LoanType = Enum.GetName(typeof(NLLoanTypes), nlModel.Loan.LoanTypeID);
			model.TermOnlyInterest = model.Schedule.Count(s => s.LoanRepayment == 0);
			model.TermOnlyInterestWords = FormattingUtils.ConvertToWord(model.TermOnlyInterest).ToLower();
			model.TermInterestAndPrincipal = model.Schedule.Count(s => s.LoanRepayment != 0 && s.Interest != 0);
			model.TermInterestAndPrincipalWords = FormattingUtils.ConvertToWord(model.TermInterestAndPrincipal).ToLower();
			model.isHalwayLoan = Enum.GetName(typeof(NLLoanTypes), nlModel.Loan.LoanTypeID) == NLLoanTypes.HalfWayLoanType.ToString();
			model.CountRepayment = model.Schedule.Count;
			model.Term = model.Schedule.Count;

			model.TotalPrincipalWithSetupFee = FormattingUtils.NumericFormats(totalPrincipal - totalFees);

			return model;
		} // NL_BuildAgreementModel

		public decimal GetUSDCurrencyRate() {
			var currencyRateRepository = ObjectFactory.GetInstance<CurrencyRateRepository>();
			return (decimal)currencyRateRepository.GetCurrencyHistoricalRate(DateTime.UtcNow, "USD");
		} // GetUSDCurrencyRate

		protected readonly ServiceClient serviceClient;

		private readonly ILoanLegalRepository loanLegalRepo;
		private readonly APRCalculator aprCalc;
		private readonly RepaymentCalculator repaymentCalculator;
		private readonly IEzbobWorkplaceContext context;
		private readonly SafeILog log;
	} // class AgreementsModelBuilder
} // namespace