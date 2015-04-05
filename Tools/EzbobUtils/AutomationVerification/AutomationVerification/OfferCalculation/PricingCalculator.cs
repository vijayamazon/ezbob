namespace AutomationCalculator.OfferCalculation {
	using System;
	using Common;
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class PricingCalculator {
		public PricingCalculator(
			int customerID,
			PricingScenarioModel pricingScenario,
			int loanAmount,
			int repaymentPeriod,
			AConnection db,
			ASafeLog log
		) {
			this.log = log.Safe();
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
			decimal setupFee = // f
				(this.pricingScenario.SetupFee / 100) * this.loanAmount;

			int realRepaymentPeriod = // ñ
				(int)Math.Floor(this.repaymentPeriod * this.pricingScenario.TenurePercents);

			int realInterestOnlyPeriod = // õ
				(int)Math.Ceiling(this.pricingScenario.InterestOnlyPeriod * this.pricingScenario.TenurePercents);

			int realRestPeriod = // ḿ
				realRepaymentPeriod - realInterestOnlyPeriod;

			decimal cost = // C
				GetTotalCost();

			decimal profit = // p
				this.pricingScenario.ProfitMarkupPercentsOfRevenue;

			decimal defaultRate = // d
				this.calculatedDefaultRate;

			// this.loanAmount; // A

			decimal r = (2 * (cost / (1 - profit) - setupFee)) /
				(this.loanAmount * (1 - defaultRate) * (2 * realInterestOnlyPeriod + realRestPeriod + 1));

			this.log.Debug(@"PricingCalculator.GetInterestRate() - formula is:
           C
      2(------- - f)
         1 - p
r = ----------------------
     A(1 - d)(2õ + ḿ + 1)
");
			this.log.Debug("Terms in the above formula are:\n\t{0}\n", string.Join("\n\t",
				DisplayFormulaTerm(setupFee, "f", "setup fee"),
				DisplayFormulaTerm(realRepaymentPeriod, "ñ", "repayment months after tenure"),
				DisplayFormulaTerm(realInterestOnlyPeriod, "õ", "interest only months after tenure"),
				DisplayFormulaTerm(realRestPeriod, "ḿ", "ñ - õ"),
				DisplayFormulaTerm(cost, "C", "total loan cost"),
				DisplayFormulaTerm(profit, "p", "profit rate"),
				DisplayFormulaTerm(defaultRate, "d", "default rate"),
				DisplayFormulaTerm(this.loanAmount, "A", "loan amount"),
				DisplayFormulaTerm(r, "r", "interest rate")
			));

			return r;
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

			decimal cost = // C
				GetTotalCost();

			decimal profit = // p
				this.pricingScenario.ProfitMarkupPercentsOfRevenue;

			decimal defaultRate = // d
				this.calculatedDefaultRate;

			// interestRate; // r

			decimal setupFee = // f
				(cost / (1 - profit)) - (
					(
						interestRate *
						this.loanAmount *
						(1 - defaultRate) *
						(2 * realInterestOnlyPeriod + realRestPeriod + 1)
					) / 2
				);

			this.log.Debug(@"PricingCalculator.GetSetupFee() - formula is:
       C       rA(1 - d)(2õ + ḿ + 1)
f = ------- - -----------------------
     1 - p              2
");

			this.log.Debug("Terms in the above formula are:\n\t{0}\n", string.Join("\n\t", 
				DisplayFormulaTerm(this.loanAmount, "A", "loan amount"),
				DisplayFormulaTerm(realRepaymentPeriod, "ñ", "repayment months after tenure"),
				DisplayFormulaTerm(realInterestOnlyPeriod, "õ", "interest only months after tenure"),
				DisplayFormulaTerm(realRestPeriod, "ḿ", "ñ - õ"),
				DisplayFormulaTerm(cost, "C", "total loan cost"),
				DisplayFormulaTerm(profit, "p", "profit rate"),
				DisplayFormulaTerm(defaultRate, "d", "default rate"),
				DisplayFormulaTerm(interestRate, "r", "interest rate"),
				DisplayFormulaTerm(setupFee, "f", "setup fee")
			));

			return setupFee / this.loanAmount;
		} // GetSetupFee

		private decimal GetTotalCost() {
			// this.loanAmount;                                                          // A
			// this.repaymentPeriod;                                                     // n
			decimal interestOnlyMonths = this.pricingScenario.InterestOnlyPeriod;        // o
			decimal tenure = this.pricingScenario.TenurePercents;                        // t
			int realRepaymentPeriod = (int)Math.Floor(this.repaymentPeriod * tenure);    // ñ
			int realInterestOnlyPeriod = (int)Math.Ceiling(interestOnlyMonths * tenure); // õ
			int realRestPeriod = realRepaymentPeriod - realInterestOnlyPeriod;           // ḿ
			decimal debtOfTotalCapital = this.pricingScenario.DebtPercentOfCapital;      // δ
			decimal costOfDebt = this.pricingScenario.CostOfDebtPA;                      // γ
			decimal defaultRate = this.calculatedDefaultRate;                            // d
			decimal collectionRate = this.pricingScenario.CollectionRate;                // ρ
			decimal opexCapex = this.pricingScenario.OpexAndCapex;                       // Ω
			decimal cogs = this.pricingScenario.Cogs;                                    // ξ

			// formula:
			//      Aδγ(2õ + ḿ + 1)
			// Δ = -----------------
			//            24
			decimal debtCost = // Δ
				this.loanAmount * debtOfTotalCapital * costOfDebt * (2 * realInterestOnlyPeriod + realRestPeriod + 1) / 24;

			// formula: N = Ad(1 - ρ)
			decimal netLossFromDefault = // N
				this.loanAmount * defaultRate * (1 - collectionRate);

			// formula: C = Δ + N + Ω + ξ
			decimal totalCost = // C
				debtCost + netLossFromDefault + opexCapex + cogs;

			this.log.Debug(@"PricingCalculator.GetSetupFee() - formulae is:
     Aδγ(2õ + ḿ + 1)
Δ = -----------------
           24

N = Ad(1 - ρ)

C = Δ + N + Ω + ξ   <-- returned value
");

			this.log.Debug("Terms in the above formulae are:\n\t{0}\n", string.Join("\n\t", 
				DisplayFormulaTerm(this.loanAmount, "A", "loan amount"),
				DisplayFormulaTerm(this.repaymentPeriod, "n", "repayment months"),
				DisplayFormulaTerm(interestOnlyMonths, "o", "interest only months"),
				DisplayFormulaTerm(tenure, "t", "tenure rate"),
				DisplayFormulaTerm(realRepaymentPeriod, "ñ", "repayment months after tenure"),
				DisplayFormulaTerm(realInterestOnlyPeriod, "õ", "interest only months after tenure"),
				DisplayFormulaTerm(realRestPeriod, "ḿ", "ñ - õ"),
				DisplayFormulaTerm(debtOfTotalCapital, "δ", "debt/total capital"),
				DisplayFormulaTerm(costOfDebt, "γ", "cost of debt"),
				DisplayFormulaTerm(defaultRate, "d", "default rate"),
				DisplayFormulaTerm(collectionRate, "ρ", "collection rate"),
				DisplayFormulaTerm(opexCapex, "Ω", "OPEX/CAPEX"),
				DisplayFormulaTerm(cogs, "ξ", "COGS"),
				DisplayFormulaTerm(debtCost, "Δ", "debt cost"),
				DisplayFormulaTerm(netLossFromDefault, "N", "net loss"),
				DisplayFormulaTerm(totalCost, "C", "total loan cost")
			));

			return totalCost;
		} // GetTotalCost

		private string DisplayFormulaTerm(int v, string name, string fullName) {
			return string.Format("{0} = {1} ({2})", name, v, fullName);
		} // DisplayFormulaTerm

		private string DisplayFormulaTerm(decimal v, string name, string fullName) {
			return string.Format("{0} = {1} ({2})", name, v, fullName);
		} // DisplayFormulaTerm

		private readonly PricingScenarioModel pricingScenario;
		private readonly decimal calculatedDefaultRate;
		private readonly int loanAmount;
		private readonly int repaymentPeriod;
		private readonly ASafeLog log;
	} // class PricingCalculator
} // namespace
