﻿
namespace AutomationCalculator.OfferCalculation {
	using System;
	using Common;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class PricingCalculator {
		public PricingCalculator(AConnection db, ASafeLog log) {
			this.db = db;
			Log = log;
		}

		/// <summary>
		/// Calculating default rate:
		/// retrieve min (consumer,directors) score, retrieve company score
		/// find default rate for company and consumer scores from DB
		/// default rate = company default rate * company share + consumer default rate * (1 - company share)
		/// </summary>
		/// <returns>default rate</returns>
		public decimal GetDefaultRate(int customerId, PricingScenarioModel pricingScenario) {
			var dbHelper = new DbHelper(this.db, Log);
			var customerDefaultRate = dbHelper.GetOfferDefaultRate(customerId);

			var defaultRate = customerDefaultRate.Item1 * (1 - pricingScenario.DefaultRateCompanyShare) +
				customerDefaultRate.Item2 * pricingScenario.DefaultRateCompanyShare;

			return defaultRate;
		}

		/// <summary>
		/// Calculate interest rate for specific offer amount, setup fee, repayment period and pricing scenario
		/// </summary>
		/// <returns>interest rate</returns>
		public decimal GetInterestRate(int loanAmount, int repaymentPeriod, int customerId, decimal defaultRate, PricingScenarioModel pricingScenario) {
			var setupFee = (pricingScenario.SetupFee / 100) * loanAmount;                                                            //f
			int realRepaymentPeriod = (int)Math.Floor(repaymentPeriod * pricingScenario.TenurePercents);                           //ñ
			int realInterestOnlyPeriod = (int)Math.Ceiling(pricingScenario.InterestOnlyPeriod * pricingScenario.TenurePercents);   //õ
			int realRestPeriod = realRepaymentPeriod - realInterestOnlyPeriod;                                                     //ḿ
			var cost = GetTotalCost(loanAmount, repaymentPeriod, customerId, pricingScenario);                                     //C
			var profit = pricingScenario.ProfitMarkupPercentsOfRevenue;                                                            //p
#pragma warning disable 1717
			defaultRate = defaultRate;                                                                                             //d
			loanAmount = loanAmount;                                                                                               //A
#pragma warning restore 1717

			//formula is r = 2(C/(1-p) - f)/(A(1-d)(2õ + ḿ + 1))
			decimal interestRate = (2 * (cost / (1 - profit) - setupFee)) / (loanAmount * (1 - defaultRate) * (2 * realInterestOnlyPeriod + realRestPeriod + 1));
			return interestRate;
		}

		/// <summary>
		/// Calculate setup fee for specific offer amount, interest rate, repayment period and pricing scenario
		/// </summary>
		/// <returns>setup fee</returns>
		public decimal GetSetupfee(int loanAmount, int repaymentPeriod, decimal interestRate, int customerId, decimal defaultRate, PricingScenarioModel pricingScenario) {
			int realRepaymentPeriod = (int)Math.Floor(repaymentPeriod * pricingScenario.TenurePercents);                           //ñ
			int realInterestOnlyPeriod = (int)Math.Ceiling(pricingScenario.InterestOnlyPeriod * pricingScenario.TenurePercents);   //õ
			int realRestPeriod = realRepaymentPeriod - realInterestOnlyPeriod;                                                     //ḿ
			var cost = GetTotalCost(loanAmount, repaymentPeriod, customerId, pricingScenario);                                     //C
			var profit = pricingScenario.ProfitMarkupPercentsOfRevenue;                                                            //p
#pragma warning disable 1717
			defaultRate = defaultRate;                                                                                             //d
			interestRate = interestRate;                                                                                           //r
			loanAmount = loanAmount;                                                                                               //A
#pragma warning restore 1717

			//formula is f = C/(1-p) - rA(1-d)(2õ + ḿ + 1)/2
			decimal setupFee =  (cost / (1 - profit)) - ((interestRate * loanAmount * (1 - defaultRate) * (2 * realInterestOnlyPeriod + realRestPeriod + 1)) / 2);
			return setupFee / loanAmount;
		}

		public decimal GetTotalCost(int loanAmount, int repaymentPeriod, int customerId, PricingScenarioModel pricingScenario) {
#pragma warning disable 1717
			loanAmount = loanAmount;                                                                                    //A
			repaymentPeriod = repaymentPeriod;                                                                          //n
#pragma warning restore 1717
			var interestOnlyMonths = pricingScenario.InterestOnlyPeriod;                                                //o
			var tenure = pricingScenario.TenurePercents;                                                                //t
			int realRepaymentPeriod = (int)Math.Floor(repaymentPeriod * tenure);                                        //ñ
			int realInterestOnlyPeriod = (int)Math.Ceiling(interestOnlyMonths * tenure);                                //õ
			int realRestPeriod = realRepaymentPeriod - realInterestOnlyPeriod;                                          //ḿ
			var debtOfTotalCapital = pricingScenario.DebtPercentOfCapital;                                              //δ
			var costOfDebt = pricingScenario.CostOfDebtPA;                                                              //γ
			var defaultRate = GetDefaultRate(customerId, pricingScenario);                                              //d
			var collectionRate = pricingScenario.CollectionRate;                                                        //ρ
			var opexCapex = pricingScenario.OpexAndCapex;                                                               //Ω
			var cogs = pricingScenario.Cogs;                                                                            //ξ

			//formula: (Aδγ(2õ + ḿ + 1)/24)
			var debtCost = loanAmount * debtOfTotalCapital * costOfDebt * (2 * realInterestOnlyPeriod + realRestPeriod + 1) / 24; //Δ

			//formula: N = Ad(1-ρ)
			var netLossFromDefault = loanAmount * defaultRate * (1 - collectionRate);                                   //N

			//formula: C = Δ + N + Ω +ξ;
			var totalCost = debtCost + netLossFromDefault + opexCapex + cogs;                                           //C

			return totalCost;
		}

		protected readonly ASafeLog Log;
		protected readonly AConnection db;
	}
}
