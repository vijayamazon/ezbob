namespace AutomationCalculator.OfferCalculation {
	using System;
	using Common;
	using Ezbob.Database;

	internal class PricingCalculator {
		public PricingCalculator(
			int customerID,
			PricingScenarioModel pricingScenario,
			int loanAmount,
			int repaymentPeriod,
			AConnection db
		) {
			this.pricingScenario = pricingScenario;
			this.loanAmount = loanAmount;
			this.repaymentPeriod = repaymentPeriod;

			// Calculating default rate: retrieve min (consumer,directors) score, retrieve company score
			// find default rate for company and consumer scores from DB
			// default rate = company default rate * company share + consumer default rate * (1 - company share)

			var sr = db.GetFirst(
				"GetOfferConsumerBusinessDefaultRates",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerID)
			);

			decimal consumerDefautRate = sr["ConsumerDefaultRate"];
			decimal businessDefautRate = sr["BusinessDefaultRate"];

			this.calculatedDefaultRate =
				consumerDefautRate * (1 - this.pricingScenario.DefaultRateCompanyShare) +
				businessDefautRate * this.pricingScenario.DefaultRateCompanyShare;
		} // constructor

		/// <summary>
		/// Calculate interest rate for specific offer amount, setup fee, repayment period and pricing scenario
		/// </summary>
		/// <returns>interest rate</returns>
		public decimal GetInterestRate() {
			var setupFee = // f
				(this.pricingScenario.SetupFee / 100) * this.loanAmount;

			int realRepaymentPeriod = // ñ
				(int)Math.Floor(this.repaymentPeriod * this.pricingScenario.TenurePercents);

			int realInterestOnlyPeriod = // õ
				(int)Math.Ceiling(this.pricingScenario.InterestOnlyPeriod * this.pricingScenario.TenurePercents);

			int realRestPeriod = // ḿ
				realRepaymentPeriod - realInterestOnlyPeriod;

			var cost = // C
				GetTotalCost();

			var profit = // p
				this.pricingScenario.ProfitMarkupPercentsOfRevenue;

			var defaultRate = // d
				this.calculatedDefaultRate;

			// this.loanAmount; // A

			// formula is
			//           C
			//      2(------- - f)
			//         1 - p
			// r = ----------------------
			//      A(1 - d)(2õ + ḿ + 1)

			return (2 * (cost / (1 - profit) - setupFee)) /
				(this.loanAmount * (1 - defaultRate) * (2 * realInterestOnlyPeriod + realRestPeriod + 1));
		} // GetInterestRate

		/// <summary>
		/// Calculate setup fee for specific offer amount, interest rate, repayment period and pricing scenario
		/// </summary>
		/// <returns>setup fee</returns>
		public decimal GetSetupfee(decimal interestRate) {
			// this.loanAmount; // A

			int realRepaymentPeriod = // ñ
				(int)Math.Floor(this.repaymentPeriod * this.pricingScenario.TenurePercents);

			int realInterestOnlyPeriod = // õ
				(int)Math.Ceiling(this.pricingScenario.InterestOnlyPeriod * this.pricingScenario.TenurePercents);

			int realRestPeriod = // ḿ
				realRepaymentPeriod - realInterestOnlyPeriod;

			var cost = // C
				GetTotalCost();

			var profit = // p
				this.pricingScenario.ProfitMarkupPercentsOfRevenue;

			var defaultRate = // d
				this.calculatedDefaultRate;

			// interestRate; // r

			// formula is
			//        C       rA(1 - d)(2õ + ḿ + 1)
			// f = ------- - -----------------------
			//      1 - p              2

			decimal setupFee = // f
				(cost / (1 - profit)) - (
					(
						interestRate *
						this.loanAmount *
						(1 - defaultRate) *
						(2 * realInterestOnlyPeriod + realRestPeriod + 1)
					) / 2
				);

			return setupFee / this.loanAmount;
		} // GetSetupFee

		private decimal GetTotalCost() {
			// this.loanAmount;                                                          // A
			// this.repaymentPeriod;                                                     // n
			var interestOnlyMonths = this.pricingScenario.InterestOnlyPeriod;            // o
			var tenure = this.pricingScenario.TenurePercents;                            // t
			int realRepaymentPeriod = (int)Math.Floor(this.repaymentPeriod * tenure);    // ñ
			int realInterestOnlyPeriod = (int)Math.Ceiling(interestOnlyMonths * tenure); // õ
			int realRestPeriod = realRepaymentPeriod - realInterestOnlyPeriod;           // ḿ
			var debtOfTotalCapital = this.pricingScenario.DebtPercentOfCapital;          // δ
			var costOfDebt = this.pricingScenario.CostOfDebtPA;                          // γ
			var defaultRate = this.calculatedDefaultRate;                                // d
			var collectionRate = this.pricingScenario.CollectionRate;                    // ρ
			var opexCapex = this.pricingScenario.OpexAndCapex;                           // Ω
			var cogs = this.pricingScenario.Cogs;                                        // ξ

			// formula:
			//      Aδγ(2õ + ḿ + 1)
			// Δ = -----------------
			//            24
			var debtCost = // Δ
				this.loanAmount * debtOfTotalCapital * costOfDebt * (2 * realInterestOnlyPeriod + realRestPeriod + 1) / 24;

			// formula: N = Ad(1 - ρ)
			var netLossFromDefault = // N
				this.loanAmount * defaultRate * (1 - collectionRate);

			// formula: C = Δ + N + Ω + ξ
			var totalCost = // C
				debtCost + netLossFromDefault + opexCapex + cogs;

			return totalCost;
		} // GetTotalCost

		private readonly PricingScenarioModel pricingScenario;
		private readonly decimal calculatedDefaultRate;
		private readonly int loanAmount;
		private readonly int repaymentPeriod;
	} // class PricingCalculator
} // namespace
