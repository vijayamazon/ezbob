namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Implement {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using ConfigManager;
	using Ezbob.Backend.Models.Investor;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts;
	using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
	using Ezbob.Backend.Strategies.OpenPlatform.Models;
	using log4net;
	using StructureMap.Attributes;

	public class InvestorParametersBLL : IInvestorParametersBLL {
		[SetterProperty]
		public IInvestorParametersDAL InvestorParametersDAL { get; set; }

		public List<int> GetInvestorsIds() {
			return InvestorParametersDAL.GetInvestorsIds();
		}

		public InvestorParameters GetInvestorParameters(int investorId, RuleType ruleType) {
			var iInvestorParameters = InvestorParametersDAL.GetInvestorParametersDB(investorId, ruleType);
			var investorParameters = new InvestorParameters();
			investorParameters.Balance = 0;
			decimal value;
			investorParameters.Balance = InvestorParametersDAL.InvestorsBalance.TryGetValue(investorId, out value) ? value : 0;
			if (iInvestorParameters == null)
				return investorParameters;
			var dailyInvestmentAllowed = GetParameterValue("DailyInvestmentAllowed", iInvestorParameters);
			var weeklyInvestmentAllowed = GetParameterValue("WeeklyInvestmentAllowed", iInvestorParameters);
			investorParameters.DailyAvailableAmount = dailyInvestmentAllowed == null ? investorParameters.Balance : (decimal)dailyInvestmentAllowed - InvestorParametersDAL.GetFundedAmountPeriod(investorId, InvesmentPeriod.Day);
			investorParameters.WeeklyAvailableAmount = weeklyInvestmentAllowed == null ? investorParameters.Balance : (decimal)weeklyInvestmentAllowed - InvestorParametersDAL.GetFundedAmountPeriod(investorId, InvesmentPeriod.Week);
			investorParameters.InvestorID = investorId;
			return investorParameters;
		}

		private object GetParameterValue(string parameterName, List<I_InvestorParams> iInvestorParameters) {
			var parameterId = InvestorParametersDAL.InvestorsParameters.FirstOrDefault(x => x.Value.Name == parameterName).Key;
			var param = iInvestorParameters.FirstOrDefault(x => x.InvestorParamsID == parameterId);
			if (InvestorParametersDAL.InvestorsParameters.ContainsKey(parameterId))
				if (param != null)
					return Convert.ChangeType(param.Value, Type.GetType("System." + InvestorParametersDAL.InvestorsParameters[parameterId].ValueType));
			return null;
		}

		public decimal GetGradeAvailableAmount(int investorId, InvestorLoanCashRequest investorLoanCashRequest, int ruleType) {

			//The amount that investor invest on grade this month.                                   
			decimal investorGradeInvestedAmount = InvestorParametersDAL.GetGradeMonthlyInvestedAmount(investorId, (Grade)investorLoanCashRequest.GradeID);

			//Sum of already funded this month + current
			var totalFunded = investorGradeInvestedAmount;

		    var amplitude = Convert.ToDecimal(CurrentValues.Instance.InvestorBudgetAmplitude.Value);

			//Calc max percent for grade.
			var gradePercent = InvestorParametersDAL.GetGradePercent(investorId, investorLoanCashRequest.GradeID, ruleType) + amplitude;

			//Calc total sum of positive transactions this month
			var investorMonthlyBalance = InvestorParametersDAL.GetInvestorTotalMonthlyDeposits(investorId);

			var monthlyFundingCapital = InvestorParametersDAL.GetInvestorMonthlyFundingCapital(investorId);

			var monthlyMax = Math.Max(investorMonthlyBalance, monthlyFundingCapital);

			var gradeAvailableAmount = gradePercent * monthlyMax - totalFunded;
			Log.InfoFormat("GetGradeAvailableAmount investorID {0} grade {1} GradeAvailableAmount: {2} Funded amount {3} gradePercent {4} investorMonthlyBalance {5} monthlyFundingCapital {6}",
				investorId, investorLoanCashRequest.GradeID, gradeAvailableAmount, totalFunded, gradePercent, investorMonthlyBalance, monthlyFundingCapital);
			
            return gradeAvailableAmount;
		}

		public int GetInvestorWithLatestLoanDate(List<int> investorsList) {
			return InvestorParametersDAL.GetInvestorWithLatestLoanDate(investorsList);
		}

		protected static ILog Log = LogManager.GetLogger(typeof(InvestorParametersBLL));
	}
}
