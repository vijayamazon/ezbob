
namespace AutomationCalculator.OfferCalculation
{
	using System;
	using Common;

	public class PricingCalculator
	{
		/// <summary>
		/// Calculate interest rate for specific offer amount, setup fee, repayment period and pricing scenario
		/// </summary>
		/// <returns>interest rate</returns>
		public decimal GetInterestRate(int loanAmount, int repaymentPeriod, PricingScenarioModel pricingScenario) {
			var setupFee = (pricingScenario.SetupFee/100) * loanAmount;                                                            //f
			int realRepaymentPeriod = (int)Math.Floor(repaymentPeriod * pricingScenario.TenurePercents);                           //ñ
			int realInterestOnlyPeriod = (int)Math.Ceiling(pricingScenario.InterestOnlyPeriod * pricingScenario.TenurePercents);   //õ
			int realRestPeriod = realRepaymentPeriod - realInterestOnlyPeriod;                                                     //ḿ
			var cost = loanAmount * pricingScenario.CostOfDebtPA;                                                                  //C
			var profit = pricingScenario.ProfitMarkupPercentsOfRevenue;                                                            //p
			var defaultRate = pricingScenario.CollectionRate;                                                                      //d
			loanAmount = loanAmount;                                                                                               //A


			//formula is r = 2(C/(1-p) - f)/(A(1-d)(2õ + ḿ + 1))
			decimal interestRate = (2*(cost/(1 - profit) - setupFee))/(loanAmount*(1 - defaultRate)*(2*realInterestOnlyPeriod + realRestPeriod + 1));
			return interestRate;
		}

		/// <summary>
		/// Calculate setup fee for specific offer amount, interest rate, repayment period and pricing scenario
		/// </summary>
		/// <returns>setup fee</returns>
		public decimal GetSetupfee(int loanAmount, int repaymentPeriod, decimal interestRate, PricingScenarioModel pricingScenario) {
			int realRepaymentPeriod = (int)Math.Floor(repaymentPeriod * pricingScenario.TenurePercents);                           //ñ
			int realInterestOnlyPeriod = (int)Math.Ceiling(pricingScenario.InterestOnlyPeriod * pricingScenario.TenurePercents);   //õ
			int realRestPeriod = realRepaymentPeriod - realInterestOnlyPeriod;                                                     //ḿ
			var cost = loanAmount * pricingScenario.CostOfDebtPA;                                                                  //C
			var profit = pricingScenario.ProfitMarkupPercentsOfRevenue;                                                            //p
			var defaultRate = pricingScenario.CollectionRate;                                                                      //d
			interestRate = interestRate;                                                                                           //r
			loanAmount = loanAmount;                                                                                               //A
			
			//formula is f = C/(1-p) - rA(1-d)(2õ + ḿ + 1)/2
			decimal setupFee =  (cost / (1 - profit)) - ((interestRate * loanAmount * (1 - defaultRate) * (2 * realInterestOnlyPeriod + realRestPeriod + 1)) / 2);
			return setupFee / loanAmount;
		}
	}
}
