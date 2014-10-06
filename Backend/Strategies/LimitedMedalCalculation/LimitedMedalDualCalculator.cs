namespace EzBob.Backend.Strategies.LimitedMedalCalculation
{
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;

	public class LimitedMedalDualCalculator
	{
		private readonly ASafeLog log;
		private readonly AConnection db;
		private readonly NewMedalScoreCalculator1 limitedMedalCalculator1;
		private readonly NewMedalScoreCalculator2 limitedMedalCalculator2;

		public ScoreResult Results { get; set; }

		public LimitedMedalDualCalculator(AConnection db, ASafeLog log)
		{
			this.log = log;
			this.db = db;
			limitedMedalCalculator1 = new NewMedalScoreCalculator1(db, log);
			limitedMedalCalculator2 = new NewMedalScoreCalculator2(db, log);
		}

		public ScoreResult CalculateMedalScore(int customerId)
		{
			ScoreResult result1 = null;
			try
			{
				DateTime calculationTime = DateTime.UtcNow;
				result1 = limitedMedalCalculator1.CalculateMedalScore(customerId, calculationTime);
				ScoreResult result2 = limitedMedalCalculator2.CalculateMedalScore(customerId, calculationTime);

				if (CheckResultsForMatching(result1, result2))
				{
					Save(result1, customerId);
				}
				else
				{
					log.Error("Mismatch found in the 2 medal calculations of customer: {0}", customerId);
				}
			}
			catch (Exception e)
			{
				log.Warn("Offline medal calculation for customer {0} failed with exception:{1}", customerId, e);
			}

			return result1;
		}

		private bool CheckResultsForMatching(ScoreResult result1, ScoreResult result2)
		{
			if (result1.BusinessScore != result2.BusinessScore ||
				result1.BusinessScoreWeight != result2.BusinessScoreWeight ||
				result1.BusinessScoreGrade != result2.BusinessScoreGrade ||
				result1.BusinessScoreScore != result2.BusinessScoreScore ||
				result1.FreeCashFlow != result2.FreeCashFlow ||
				result1.FreeCashFlowWeight != result2.FreeCashFlowWeight ||
				result1.FreeCashFlowGrade != result2.FreeCashFlowGrade ||
				result1.FreeCashFlowScore != result2.FreeCashFlowScore ||
				result1.AnnualTurnover != result2.AnnualTurnover ||
				result1.AnnualTurnoverWeight != result2.AnnualTurnoverWeight ||
				result1.AnnualTurnoverGrade != result2.AnnualTurnoverGrade ||
				result1.AnnualTurnoverScore != result2.AnnualTurnoverScore ||
				result1.TangibleEquity != result2.TangibleEquity ||
				result1.TangibleEquityWeight != result2.TangibleEquityWeight ||
				result1.TangibleEquityGrade != result2.TangibleEquityGrade ||
				result1.TangibleEquityScore != result2.TangibleEquityScore ||
				result1.BusinessSeniority != result2.BusinessSeniority ||
				result1.BusinessSeniorityWeight != result2.BusinessSeniorityWeight ||
				result1.BusinessSeniorityGrade != result2.BusinessSeniorityGrade ||
				result1.BusinessSeniorityScore != result2.BusinessSeniorityScore ||
				result1.ConsumerScore != result2.ConsumerScore ||
				result1.ConsumerScoreWeight != result2.ConsumerScoreWeight ||
				result1.ConsumerScoreGrade != result2.ConsumerScoreGrade ||
				result1.ConsumerScoreScore != result2.ConsumerScoreScore ||
				result1.NetWorth != result2.NetWorth ||
				result1.NetWorthWeight != result2.NetWorthWeight ||
				result1.NetWorthGrade != result2.NetWorthGrade ||
				result1.NetWorthScore != result2.NetWorthScore ||
				result1.MaritalStatus != result2.MaritalStatus ||
				result1.MaritalStatusWeight != result2.MaritalStatusWeight ||
				result1.MaritalStatusGrade != result2.MaritalStatusGrade ||
				result1.MaritalStatusScore != result2.MaritalStatusScore ||
				result1.EzbobSeniority != result2.EzbobSeniority ||
				result1.EzbobSeniorityWeight != result2.EzbobSeniorityWeight ||
				result1.EzbobSeniorityGrade != result2.EzbobSeniorityGrade ||
				result1.EzbobSeniorityScore != result2.EzbobSeniorityScore ||
				result1.NumOfLoans != result2.NumOfLoans ||
				result1.NumOfLoansWeight != result2.NumOfLoansWeight ||
				result1.NumOfLoansGrade != result2.NumOfLoansGrade ||
				result1.NumOfLoansScore != result2.NumOfLoansScore ||
				result1.NumOfLateRepayments != result2.NumOfLateRepayments ||
				result1.NumOfLateRepaymentsWeight != result2.NumOfLateRepaymentsWeight ||
				result1.NumOfLateRepaymentsGrade != result2.NumOfLateRepaymentsGrade ||
				result1.NumOfLateRepaymentsScore != result2.NumOfLateRepaymentsScore ||
				result1.NumOfEarlyRepayments != result2.NumOfEarlyRepayments ||
				result1.NumOfEarlyRepaymentsWeight != result2.NumOfEarlyRepaymentsWeight ||
				result1.NumOfEarlyRepaymentsGrade != result2.NumOfEarlyRepaymentsGrade ||
				result1.NumOfEarlyRepaymentsScore != result2.NumOfEarlyRepaymentsScore ||
				result1.TotalScore != result2.TotalScore ||
				result1.TotalScoreNormalized != result2.TotalScoreNormalized ||
				result1.Medal != result2.Medal ||
				result1.Error != result2.Error)
			{
				return false;
			}

			return true;
		}

		private void Save(ScoreResult result, int customerId)
		{
			db.ExecuteNonQuery("StoreNewMedal", CommandSpecies.StoredProcedure,
			                   new QueryParameter("CustomerId", customerId),
			                   new QueryParameter("BusinessScore", result.BusinessScore),
			                   new QueryParameter("BusinessScoreWeight", result.BusinessScoreWeight),
			                   new QueryParameter("BusinessScoreGrade", result.BusinessScoreGrade),
			                   new QueryParameter("BusinessScoreScore", result.BusinessScoreScore),
			                   new QueryParameter("FreeCashFlow", result.FreeCashFlow),
			                   new QueryParameter("FreeCashFlowWeight", result.FreeCashFlowWeight),
			                   new QueryParameter("FreeCashFlowGrade", result.FreeCashFlowGrade),
			                   new QueryParameter("FreeCashFlowScore", result.FreeCashFlowScore),
			                   new QueryParameter("AnnualTurnover", result.AnnualTurnover),
			                   new QueryParameter("AnnualTurnoverWeight", result.AnnualTurnoverWeight),
			                   new QueryParameter("AnnualTurnoverGrade", result.AnnualTurnoverGrade),
			                   new QueryParameter("AnnualTurnoverScore", result.AnnualTurnoverScore),
			                   new QueryParameter("TangibleEquity", result.TangibleEquity),
			                   new QueryParameter("TangibleEquityWeight", result.TangibleEquityWeight),
			                   new QueryParameter("TangibleEquityGrade", result.TangibleEquityGrade),
			                   new QueryParameter("TangibleEquityScore", result.TangibleEquityScore),
			                   new QueryParameter("BusinessSeniority", result.BusinessSeniority.HasValue && result.BusinessSeniority.Value.Year > 1800 ? result.BusinessSeniority : null),
			                   new QueryParameter("BusinessSeniorityWeight", result.BusinessSeniorityWeight),
			                   new QueryParameter("BusinessSeniorityGrade", result.BusinessSeniorityGrade),
			                   new QueryParameter("BusinessSeniorityScore", result.BusinessSeniorityScore),
			                   new QueryParameter("ConsumerScore", result.ConsumerScore),
			                   new QueryParameter("ConsumerScoreWeight", result.ConsumerScoreWeight),
			                   new QueryParameter("ConsumerScoreGrade", result.ConsumerScoreGrade),
			                   new QueryParameter("ConsumerScoreScore", result.ConsumerScoreScore),
			                   new QueryParameter("NetWorth", result.NetWorth),
			                   new QueryParameter("NetWorthWeight", result.NetWorthWeight),
			                   new QueryParameter("NetWorthGrade", result.NetWorthGrade),
			                   new QueryParameter("NetWorthScore", result.NetWorthScore),
			                   new QueryParameter("MaritalStatus", result.MaritalStatus.ToString()),
			                   new QueryParameter("MaritalStatusWeight", result.MaritalStatusWeight),
			                   new QueryParameter("MaritalStatusGrade", result.MaritalStatusGrade),
			                   new QueryParameter("MaritalStatusScore", result.MaritalStatusScore),
			                   new QueryParameter("EzbobSeniority", result.EzbobSeniority),
			                   new QueryParameter("EzbobSeniorityWeight", result.EzbobSeniorityWeight),
			                   new QueryParameter("EzbobSeniorityGrade", result.EzbobSeniorityGrade),
			                   new QueryParameter("EzbobSeniorityScore", result.EzbobSeniorityScore),
			                   new QueryParameter("NumOfLoans", result.NumOfLoans),
			                   new QueryParameter("NumOfLoansWeight", result.NumOfLoansWeight),
			                   new QueryParameter("NumOfLoansGrade", result.NumOfLoansGrade),
			                   new QueryParameter("NumOfLoansScore", result.NumOfLoansScore),
			                   new QueryParameter("NumOfLateRepayments", result.NumOfLateRepayments),
			                   new QueryParameter("NumOfLateRepaymentsWeight", result.NumOfLateRepaymentsWeight),
			                   new QueryParameter("NumOfLateRepaymentsGrade", result.NumOfLateRepaymentsGrade),
			                   new QueryParameter("NumOfLateRepaymentsScore", result.NumOfLateRepaymentsScore),
			                   new QueryParameter("NumOfEarlyRepayments", result.NumOfEarlyRepayments),
			                   new QueryParameter("NumOfEarlyRepaymentsWeight", result.NumOfEarlyRepaymentsWeight),
			                   new QueryParameter("NumOfEarlyRepaymentsGrade", result.NumOfEarlyRepaymentsGrade),
			                   new QueryParameter("NumOfEarlyRepaymentsScore", result.NumOfEarlyRepaymentsScore),
			                   new QueryParameter("TotalScore", result.TotalScore),
			                   new QueryParameter("TotalScoreNormalized", result.TotalScoreNormalized),
			                   new QueryParameter("Medal", result.Medal.ToString()),
			                   new QueryParameter("Error", result.Error));
		}
	}
}
