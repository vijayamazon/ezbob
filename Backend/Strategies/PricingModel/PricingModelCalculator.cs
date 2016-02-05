namespace Ezbob.Backend.Strategies.PricingModel {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using ConfigManager;
	using Ezbob.Backend.Strategies.Exceptions;
	using Ezbob.Database;
	using Ezbob.ValueIntervals;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using PaymentServices.Calculators;

	public class PricingModelCalculator : AStrategy {
		public PricingModelCalculator(int customerId, PricingModelModel model) {
			ThrowExceptionOnError = true;
			TargetLoanSource = null;
			CalculateApr = true;

			Model = model;

			this.customerId = customerId;

			this.tag = string.Format("{0}_{1}", this.customerId, Guid.NewGuid().ToString("N").ToLowerInvariant());

			LogModel("at its initial state");
		} // constructor

		public bool ThrowExceptionOnError { get; set; }

		public LoanSourceName? TargetLoanSource { get; set; }

		public bool CalculateApr { get; set; }

		public override string Name { get { return "Pricing Model Calculator"; } }

		public override void Execute() {
			new GetPricingModelDefaultRate(this.customerId, Model).Execute();

			LogModel("with updated default rate");

			Model.PricingSourceModels = new List<PricingSourceModel>();

			if (CalculateInterestRate())
				CalculateFullModelAfterInterestRate();

			LogModel("just after calculations");

			SetCustomerOriginID();

			if (!string.IsNullOrEmpty(Error) && ThrowExceptionOnError)
				throw new StrategyWarning(this, Error);
		} // Execute

		public PricingModelModel Model { get; private set; }

		public string Error { get; private set; }

		private void SetCustomerOriginID() {
			Model.OriginID = DB.ExecuteScalar<int>(
				"GetCustomerOrigin",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId)
			);
		} // SetCustomerOriginID

		private bool CalculateInterestRate() {
			if (Model == null) {
				Error = "Model can't be empty.";
				return false;
			} // if

			if (Model.LoanAmount <= 0) {
				Error = "LoanAmount must be positive.";
				return false;
			} // if

			if (Model.TenureMonths <= 0) {
				Error = "TenureMonths must be positive.";
				return false;
			} // if

			Model.FeesRevenue = Model.SetupFeePounds;

			decimal balanceForUpperBoundary, upperBoundary;

			if (!FindUpperBoundary(out upperBoundary, out balanceForUpperBoundary)) {
				Error = string.Format(
					"Monthly interest rate should be over {0}%, calculation aborted.",
					InterestRateUpperWatermark
				);
				return false;
			} // if

			Model.MonthlyInterestRate = GetMonthlyInterestRate(upperBoundary, balanceForUpperBoundary);

			return true;
		} // CalculateInterestRate

		private void CalculateFullModelAfterInterestRate() {
			Loan loan = CreateLoan(Model.MonthlyInterestRate, Model.FeesRevenue + Model.BrokerSetupFeePounds);

			Model.CostOfDebtOutput = GetCostOfDebt(loan.Schedule);
			Model.InterestRevenue = GetInterestRevenue(loan.Schedule);
			Model.Revenue = Model.FeesRevenue + Model.InterestRevenue;
			Model.CogsOutput = Model.Cogs;
			Model.OpexAndCapexOutput = Model.OpexAndCapex;
			Model.GrossProfit = Model.Revenue - Model.Cogs;
			Model.Ebitda = Model.GrossProfit - Model.OpexAndCapex;
			Model.NetLossFromDefaults = (1 - Model.CollectionRate) * Model.LoanAmount * Model.DefaultRate;
			Model.ProfitMarkupOutput = Model.ProfitMarkup * Model.Revenue;
			Model.TotalCost = Model.CostOfDebtOutput + Model.Cogs + Model.OpexAndCapex + Model.NetLossFromDefaults;

			if ((TargetLoanSource == null) || (TargetLoanSource == LoanSourceName.Standard)) {
				CreateStandardOffer(loan);

				if (TargetLoanSource == LoanSourceName.Standard)
					return;
			} // if


			decimal preferableEUInterest = CreateEUOffers();

			if (preferableEUInterest <= 0)
				return;

			if ((TargetLoanSource == null) || (TargetLoanSource == LoanSourceName.COSME))
				CreateCosmeOffers(preferableEUInterest);
		} // CalculateFullModelAfterInterestRate

		private void CreateStandardOffer(Loan loan) {
			decimal air = 0;
			decimal apr = 0;

			if (CalculateApr) {
				air = Model.TenureMonths != 0
					? (Model.MonthlyInterestRate * 12) + (Model.SetupFeePercents * 12 / Model.TenureMonths)
					: 0;

				apr = GetApr(loan);
			} // if

			Model.PricingSourceModels.Add(new PricingSourceModel {
				LoanSource = LoanSourceName.Standard,
				Source = "Ezbob loan",
				InterestRate = Model.MonthlyInterestRate,
				SetupFee = Model.SetupFeePercents,
				AIR = air,
				APR = apr,
				IsPreferable = true,
			});
		} // CreateStandardOffer

		private decimal CreateEUOffers() {
			this.preferableEuInterestRates = new List<Tuple<IntInterval, decimal>>();

			DB.ForEachRowSafe(
				sr => {
					this.preferableEuInterestRates.Add(new Tuple<IntInterval, decimal>(
						new IntInterval(sr["Start"], sr["End"]), sr["Value"]
					));
				},
				"LoadEuLoanMonthlyInterest",
				CommandSpecies.StoredProcedure
			);

			decimal preferableEUInterest = GetEuLoanMonthlyInterest();

			if (preferableEUInterest <= 0) {
				Error = string.Format(
					"Could not find preferable EU loan interest rate for consumer score {0}.",
					Model.ConsumerScore
				);

				return preferableEUInterest;
			} // if

			if ((TargetLoanSource == null) || (TargetLoanSource == LoanSourceName.EU)) {
				foreach (var tpl in this.preferableEuInterestRates) {
					CreateOfferForNonStandardSource(
						LoanSourceName.EU,
						string.Format("EU Loan ({0})", tpl.Item2.ToString("P2")),
						tpl.Item2,
						Model.EuCollectionRate,
						preferableEUInterest
					);
				} // for each
			} // if

			return preferableEUInterest;
		} // CreateEUOffers

		private void CreateCosmeOffers(decimal preferableEUInterest) {
			if (preferableEUInterest <= 0)
				return;

			decimal preferableCOSMEInterest = GetCOSMELoanMonthlyInterest();

			Log.Info(
				"Pricing calculator: Customer {0} Consumer score: {1} Company Score: {2} " +
				"preferable COSME interest {3} preferable EU interest {4}",
				this.customerId,
				Model.ConsumerScore,
				Model.CompanyScore,
				preferableCOSMEInterest,
				preferableEUInterest
			);

			foreach (decimal interestRate in cosmeInterestRates) {
				CreateOfferForNonStandardSource(
					LoanSourceName.COSME,
					string.Format("COSME Loan ({0})", interestRate.ToString("P2")),
					interestRate,
					Model.CosmeCollectionRate,
					preferableCOSMEInterest
				);
			} // for each
		} // CreateCosmeOffers

		private void CreateOfferForNonStandardSource(
			LoanSourceName loanSource,
			string sourceName,
			decimal interestRate,
			decimal collectionRate,
			decimal preferableInterestRate
		) {
			decimal air;
			decimal apr;

			decimal setupFee = GetSetupFeeForNonStandardLoan(interestRate, collectionRate, out air, out apr);

			bool isPreferable = interestRate == preferableInterestRate;

			var model = new PricingSourceModel {
				LoanSource = loanSource,
				Source = sourceName,
				InterestRate = interestRate,
				SetupFee = setupFee,
				AIR = air,
				APR = apr,
				IsPreferable = isPreferable
			};

			Model.PricingSourceModels.Add(model);
		} // CreateOfferForNonStandardSource

		private static decimal GetApr(Loan loan) {
			if (loan.LoanAmount == 0)
				return 0;

			return (decimal)(new APRCalculator().Calculate(loan.LoanAmount, loan.Schedule, loan.SetupFee, loan.Date) / 100);
		} // GetApr

		private Loan CreateLoan(decimal interestRate, decimal setupFee) {
			var loan = new Loan {
				LoanAmount = Model.LoanAmount,
				Date = DateTime.UtcNow,
				LoanType = new StandardLoanType(),
				CashRequest = null,
				SetupFee = setupFee,
				LoanLegalId = 1
			};

			new LoanScheduleCalculator {
				Interest = interestRate,
				Term = (int)Model.TenureMonths,
			}.Calculate(Model.LoanAmount, loan, loan.Date, Model.InterestOnlyPeriod);

			var calc = new LoanRepaymentScheduleCalculator(loan, loan.Date, CurrentValues.Instance.AmountToChargeFrom);
			calc.GetState();

			return loan;
		} // CreateLoan

		private decimal GetSetupFeeForNonStandardLoan(
			decimal monthlyInterestRate,
			decimal collectionRate,
			out decimal annualizedInterestRate,
			out decimal apr
		) {
			Loan loan = CreateLoan(monthlyInterestRate, Model.FeesRevenue);

			decimal costOfDebt = GetCostOfDebt(loan.Schedule);
			decimal interestRevenue = GetInterestRevenue(loan.Schedule);
			decimal netLossFromDefaults = (1 - collectionRate) * Model.LoanAmount * Model.DefaultRate;
			decimal totalCost = Model.Cogs + Model.OpexAndCapex + netLossFromDefaults + costOfDebt;
			decimal profit = totalCost / (1 - Model.ProfitMarkup);
			decimal setupFeePounds = profit - interestRevenue;
			decimal setupFee = setupFeePounds / Model.LoanAmount;

			if (CalculateApr) {
				apr = GetApr(CreateLoan(monthlyInterestRate, setupFeePounds + Model.BrokerSetupFeePounds));

				annualizedInterestRate = Model.TenureMonths != 0
					? (monthlyInterestRate * 12) + (setupFee * 12 / Model.TenureMonths)
					: 0;
			} else {
				apr = 0;
				annualizedInterestRate = 0;
			} // if

			return setupFee;
		} // GetSetupFeeForNonStandardLoan

		/// <summary>
		/// Finds "first" interest rate for which loan has positive balance.
		/// </summary>
		/// <remarks>
		/// <para>"Loan has positive balance" means we should have profit from the loan.</para>
		/// <para>"First" means "first that we discover". It is not "the lowest possible", it is just "any that fits".</para>
		/// <para>Search process: try interest rates 100%, 1000%, etc. (multiplying by 10 every time) until positive
		/// loan balance is found. If interest rate reaches InterestRateUpperWaterMark, give up
		/// and raise an alert.</para>
		/// </remarks>
		/// <param name="boundary">"First" interest rate with positive loan balance or -1 if not found.</param>
		/// <param name="balance">That positive loan balance.</param>
		/// <returns>True, if completed successfully (i.e. found upper boundary); false, otherwise.</returns>
		private bool FindUpperBoundary(out decimal boundary, out decimal balance) {
			decimal monthlyInterestRate = 1;
			balance = CalculateBalance(monthlyInterestRate);

			while (balance < 0) {
				Log.Debug("Find upper boundary: monthly interest rate = {0}, balance = {1}", monthlyInterestRate, balance);

				if (monthlyInterestRate > InterestRateUpperWatermark) {
					boundary = -1;
					return false;
				} // if

				monthlyInterestRate *= 10;
				balance = CalculateBalance(monthlyInterestRate);
			} // while

			Log.Debug("Found upper boundary: monthly interest rate = {0}, balance = {1}", monthlyInterestRate, balance);

			boundary = monthlyInterestRate;
			return true;
		} // FindUpperBoundary

		private decimal GetMonthlyInterestRate(decimal upperBoundary, decimal balanceForUpperBoundary) {
			decimal lowerBoundary = 0;
			decimal interestRateForMinBalanceAbs;
			decimal minBalanceAbsValue;

			decimal balanceForLowerBoundary = CalculateBalance(lowerBoundary);

			Log.Debug(
				"GetMonthlyInterestRate: initial lower boundary is {0}, balance is {1}.",
				lowerBoundary,
				balanceForLowerBoundary
			);
			Log.Debug(
				"GetMonthlyInterestRate: initial upper boundary is {0}, balance is {1}",
				upperBoundary,
				balanceForUpperBoundary
			);

			// Balance for upper boundary is always positive.
			// When it is negative this function is not even called (and actually
			// entire process has completed with an error).

			if (balanceForUpperBoundary < Math.Abs(balanceForLowerBoundary)) {
				minBalanceAbsValue = balanceForUpperBoundary;
				interestRateForMinBalanceAbs = upperBoundary;
			} else {
				minBalanceAbsValue = Math.Abs(balanceForLowerBoundary);
				interestRateForMinBalanceAbs = lowerBoundary;
			} // if

			decimal monthlyInterestRate = Math.Round((lowerBoundary + upperBoundary) / 2, 4, MidpointRounding.AwayFromZero);

			while (minBalanceAbsValue != 0) {
				decimal balance = CalculateBalance(monthlyInterestRate);

				Log.Debug("GetMonthlyInterestRate: rate is {0}, balance is {1}", monthlyInterestRate, balance);

				if (Math.Abs(balance) < minBalanceAbsValue) {
					minBalanceAbsValue = Math.Abs(balance);
					interestRateForMinBalanceAbs = monthlyInterestRate;
				} // if

				if (lowerBoundary + 0.0001m == upperBoundary)
					return interestRateForMinBalanceAbs;

				if (balance < 0) {
					lowerBoundary = monthlyInterestRate;

					monthlyInterestRate = Math.Round((lowerBoundary + upperBoundary) / 2, 5, MidpointRounding.AwayFromZero);

					if ((monthlyInterestRate * 100000) % 10 != 0)
						monthlyInterestRate -= 0.00005m;
				} else {
					upperBoundary = monthlyInterestRate;
					monthlyInterestRate = Math.Round((lowerBoundary + upperBoundary) / 2, 4, MidpointRounding.AwayFromZero);
				} // if
			} // while

			return interestRateForMinBalanceAbs;
		} // GetMonthlyInterestRate

		private void LogModel(string occasion) {
			var os = new StringBuilder();

			foreach (string propName in this.modelFieldsToLog) {
				os.AppendFormat("\t{0}: {1}{2}",
					propName,
					Model.GetType().GetProperty(propName).GetValue(Model),
					System.Environment.NewLine
				);
			} // for each

			os.AppendLine("\tModels per loan source:");

			if (Model.PricingSourceModels == null)
				os.AppendLine("\t!!! NONE !!!");
			else {
				foreach (PricingSourceModel source in Model.PricingSourceModels) {
					os.AppendFormat("\tSource: {0}{1}", source.Source, System.Environment.NewLine);

					foreach (string propName in this.sourceFieldsToLog) {
						os.AppendFormat("\t\t{0}: {1}{2}",
							propName,
							source.GetType().GetProperty(propName).GetValue(source),
							System.Environment.NewLine
						);
					} // for each
				} // for each
			}  // if

			Log.Msg("Pricing calculator model {0} ({1}):\n{2}", occasion, this.tag, os);
		} // LogModel

		private readonly string[] modelFieldsToLog = {
			"FlowType", "LoanAmount", "DefaultRate", "DefaultRateCompanyShare", "DefaultRateCustomerShare", "SetupFeePounds",
			"SetupFeePercents", "BrokerSetupFeePounds", "BrokerSetupFeePercents", "LoanTerm", "InterestOnlyPeriod",
			"TenurePercents", "TenureMonths", "CollectionRate", "EuCollectionRate", "CosmeCollectionRate", "Cogs",
			"DebtPercentOfCapital", "CostOfDebt", "OpexAndCapex", "ProfitMarkup",
			"ConsumerScore", "ConsumerDefaultRate", "CompanyScore", "CompanyDefaultRate",
			"Grade", "GradeScore", "ProbabilityOfDefault",
		};

		private readonly string[] sourceFieldsToLog = { "InterestRate", "SetupFee", "AIR", "APR", "IsPreferable", };

		private decimal CalculateBalance(decimal interestRate) {
			Loan loan = CreateLoan(interestRate, Model.FeesRevenue);
			decimal costOfDebtOutput = GetCostOfDebt(loan.Schedule);
			decimal interestRevenue = GetInterestRevenue(loan.Schedule);
			decimal revenue = Model.FeesRevenue + interestRevenue;
			decimal netLossFromDefaults = (1 - Model.CollectionRate) * Model.LoanAmount * Model.DefaultRate;
			decimal profitMarkupOutput = Model.ProfitMarkup * revenue;
			return revenue - Model.Cogs - Model.OpexAndCapex - netLossFromDefaults - costOfDebtOutput - profitMarkupOutput;
		} // CalculateBalance

		private decimal GetCostOfDebt(IEnumerable<LoanScheduleItem> schedule) {
			decimal costOfDebtOutput = 0;
			decimal balanceAtBeginningOfMonth = Model.LoanAmount;

			foreach (LoanScheduleItem scheuldeItem in schedule) {
				costOfDebtOutput += balanceAtBeginningOfMonth * Model.DebtPercentOfCapital * Model.CostOfDebt / 12;
				balanceAtBeginningOfMonth = scheuldeItem.Balance;
			} // for each scheduled installment

			return costOfDebtOutput;
		} // GetCostOfDebt

		private decimal GetInterestRevenue(IEnumerable<LoanScheduleItem> schedule) {
			return schedule.Sum(scheuldeItem => scheuldeItem.Interest) * (1 - Model.DefaultRate);
		} // GetInterestRevenue

		private decimal GetEuLoanMonthlyInterest() {
			foreach (var tpl in this.preferableEuInterestRates)
				if (tpl.Item1.Contains(Model.ConsumerScore))
					return tpl.Item2;

			return 0;
		} // GetEuLoanMonthlyInterest

		/// <summary>
		/// Retrieve the preferable COSME interest rate based on customers personal and business score
		/// TODO make configurable in DB
		/// </summary>
		/// <returns>preferable interest rate</returns>
		private decimal GetCOSMELoanMonthlyInterest() {
			if (Model.ConsumerScore < 1040 && Model.CompanyScore == 0)
				return 0.0225M;

			if (Model.ConsumerScore >= 1040 && Model.CompanyScore == 0)
				return 0.0175M;

			if (Model.ConsumerScore < 1040 && Model.CompanyScore >= 50)
				return 0.02M;

			if (Model.ConsumerScore >= 1040 && Model.CompanyScore >= 50)
				return 0.0175M;

			// if company score < 50
			return 0.0225M;
		} // GetCOSMELoanMonthlyInterest

		private PricingModelModel JustCalculateForComparison() {
			var result = Model.Clone().ClearOutput();
			//TODO
			return result;
		} // JustCalculateForComparison

		private readonly int customerId;
		private readonly string tag;
		private List<Tuple<IntInterval, decimal>> preferableEuInterestRates;

		private readonly decimal[] cosmeInterestRates = { 0.0225M, 0.02M, 0.0175M, };

		private const decimal InterestRateUpperWatermark = 1000000;
	} // class PricingModelCalculator
} // namespace
